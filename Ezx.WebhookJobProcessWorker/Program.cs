using Ezx.WebhookJobCreator.Model;
using Ezx.WebhookJobCreator.Services;
using Newtonsoft.Json;
using RestSharp;

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
        //await rabitMQ.SendFailProductMessage("FailedExchange", "FailedWebHookJobModel", 60000);

        // RabitMQ Implementation
        var result = await rabitMQ.ReceiveProductMessage<WebHookJobModel>("WebHookJob");

       await SendHttpRequestMessage(result);
    }

    public static async Task SendHttpRequestMessage(List<WebHookJobModel> webHookJobs)
    {
        foreach (var webHookJob in webHookJobs)
        {
            await ExecuteHttpClientRequest(webHookJob);
        }

    }
    //To process a WebHook job successfully, post the payload to the url specified in the job. 
    public static async Task ExecuteHttpClientRequest(WebHookJobModel job)
    {
        try
        {
           
            var options = new RestClientOptions(job.Url)
            {
                MaxTimeout = -1,
            };
            var client = new RestClient(options);
            var request = new RestRequest("/api/Coupon", Method.Post);

            var searchReqSerialize = JsonConvert.SerializeObject(job.Payload);

            request.AddHeader("Content-Type", "application/json");
            var body = searchReqSerialize;
            request.AddStringBody(body, DataFormat.Json);
            var response = await client.PostAsync(request);
            Console.WriteLine(response.Content);

            //If a HTTP 200 is received, regard the job as successful
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Success");
            }
            //If not sucessfull , use DLX to delay requeing of the job for later (60 seconds)

            RabitMQService rabitMQ = new RabitMQService();

            await rabitMQ.SendFailProductMessage(job.Payload, "FailedWebHookJobModel", 60000);

        }
        catch (Exception ex)
        {

            throw new Exception(ex.Message);
        }



    }

}
