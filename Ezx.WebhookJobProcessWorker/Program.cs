using Ezx.WebhookJobCreator.Model;
using Ezx.WebhookJobCreator.Services;
using Newtonsoft.Json;
using System.Text;

public class Program
{
    static void Main(string[] args)
    {
        //Ezx.WebhookJobProcessWorker which is responsible for receiving WebhookJob’s from the queue and processing them
        ReceiveWebHookJob();

        Thread.Sleep(60000);
        ReceiveWebHookJobFail();
    }

    public static async void ReceiveWebHookJob()
    {

        RabitMQService rabitMQ = new RabitMQService();

        // RabitMQ Implementation
        var result = await rabitMQ.ReceiveWebHookJobMessage<WebHookJobModel>("WebHookJob");

        var httpRequestMessage = await SendHttpRequestMessage(result);
    }


    //taken all jobs and send to the api using URL and payload
    public static async Task<string> SendHttpRequestMessage(List<WebHookJobModel> webHookJobs)
    {
        //send webHookJobs to API one by one
        foreach (var webHookJob in webHookJobs)
        {
            var result = await ExecuteHttpClientRequest(webHookJob);
        }
        return "";

    }


    //To process a WebHook job successfully, post the payload to the url specified in the job. 
    public static async Task<string> ExecuteHttpClientRequest(WebHookJobModel job)
    {
        var jsonPayload = JsonConvert.SerializeObject(job.Payload);

        //if failed then, use DLX
        var failedObject = JsonConvert.SerializeObject(job);
        try
        {
            var data = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var url = job.Url;
            using var client = new HttpClient();

            var response = client.PostAsync(url, data);
            response.Wait();

            Console.WriteLine(response.Result);

            //If a HTTP 200 is received, regard the job as successful
            if (response.Result.IsSuccessStatusCode)
            {
                Console.WriteLine("Success");
                Console.WriteLine(response.Result);
                return "Success";
            }
            else
            {
                //If not sucessfull , use DLX to delay requeing of the job for later (60 seconds)

                RabitMQService rabitMQ = new RabitMQService();
                //if failed then, use DLX

                var res = await rabitMQ.SendFailWebHookJobMessage(failedObject, "FailedWebHookJobModel", 86400000);
                Thread.Sleep(60000);

                return res;
            }

        }
        catch (Exception ex)
        {

            RabitMQService rabitMQ = new RabitMQService();
            //if failed then, use DLX

            var res = await rabitMQ.SendFailWebHookJobMessage(failedObject, "FailedWebHookJobModel", 86400000);
            Thread.Sleep(60000);

            return res;
        }



    }


    // to get failed WebHookJob
    public static async void ReceiveWebHookJobFail()
    {

        RabitMQService rabitMQ = new RabitMQService();

        // RabitMQ Implementation
        var result = await rabitMQ.ReceiveSendFailWebHookJobMessage<WebHookJobModel>("FailedWebHookJobModel");

        var httpRequestMessage = await SendHttpRequestMessage(result);
    }



}

