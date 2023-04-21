using Ezx.WebhookJobCreator;
using Ezx.WebhookJobCreator.Model;
using Ezx.WebhookJobCreator.Services;

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
            Payload = "Payload",
            RetryCount = 3,
            Url = " https://localhost:61511/api/Coupon"
        };
        var json = await JsonHelper.SerializeAsync<WebHookJobModel>(webHookJob);


        // RabitMQ Implementation
        await rabitMQ.SendWebHookJobMessage(json, "WebHookJob", 60000);
    }

}
