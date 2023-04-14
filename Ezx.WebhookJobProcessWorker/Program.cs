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

        // RabitMQ Implementation
        var result = await rabitMQ.ReceiveProductMessage<WebHookJobModel>("WebHookJob");

        SendHttpRequestMessage(result);
    }

    public static async void SendHttpRequestMessage(List<WebHookJobModel> webHookJobs)
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
            var client = new RestClient(job.Url);
            var request = new RestRequest(job.Url, Method.Post);

            //request.AddHeader("Payload", job.Payload);
            request.AddHeader("Content-Type", "application/json");

            var searchReqSerialize = JsonConvert.SerializeObject(job.Payload);
            request.AddParameter("application/json", searchReqSerialize, ParameterType.RequestBody);
            //execute request 
            var response = await client.ExecutePostAsync(request);


            //If a HTTP 200 is received, regard the job as successful
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Success");
            }
            //If not sucessfull , use DLX to delay requeing of the job for later (60 seconds)
        }
        catch (Exception ex)
        {


        }
       


    }

}
