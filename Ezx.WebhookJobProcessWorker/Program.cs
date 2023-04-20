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
    }

    public static async void ReceiveWebHookJob()
    {

        RabitMQService rabitMQ = new RabitMQService();

        // RabitMQ Implementation
        var result = await rabitMQ.ReceiveProductMessage<WebHookJobModel>("WebHookJob");

        var httpRequestMessage = await SendHttpRequestMessage(result);
    }

    public static async Task<string> SendHttpRequestMessage(List<WebHookJobModel> webHookJobs)
    {
        foreach (var webHookJob in webHookJobs)
        {
            var result = await ExecuteHttpClientRequest(webHookJob);
            return result;
        }
        return "";

    }
    //To process a WebHook job successfully, post the payload to the url specified in the job. 
    public static async Task<string> ExecuteHttpClientRequest(WebHookJobModel job)
    {
        try
        {
            var json = JsonConvert.SerializeObject(job.Payload);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

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

                var res = await rabitMQ.SendFailProductMessage(job.Payload, "FailedWebHookJobModel", 60000);
                return res;
            }

        }
        catch (Exception ex)
        {

            RabitMQService rabitMQ = new RabitMQService();

            var res = await rabitMQ.SendFailProductMessage(job.Payload, "FailedWebHookJobModel", 60000);
            return res;
        }



    }

}

internal class Person
{
    private string v1;
    private string v2;

    public Person(string v1, string v2)
    {
        this.v1 = v1;
        this.v2 = v2;
    }
}