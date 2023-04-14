using Ezx.WebhookJobCreator;
using Ezx.WebhookJobCreator.Model;
using Ezx.WebhookJobCreator.Services;
using System.Collections;

public class Program
{
    static void Main(string[] args)
    {
        //a “WebhookJob“ to a RabbitMQ FIFO Queue. A TTL should be set (24 hours).
        SendWebHookJob();
    }

    public static async void SendWebHookJob()
    {

        RabitMQService rabitMQ = new RabitMQService();

        WebHookJobModel webHookJob = new WebHookJobModel()
        {
            Payload = "testPayload",
            RetryCount = 2,
            Url = "textURL"
        };
         var json = await JsonHelper.SerializeAsync<WebHookJobModel>(webHookJob);

       
        // RabitMQ Implementation
        await rabitMQ.SendProductMessage(json, "WebHookJob",60000);
    }

}
