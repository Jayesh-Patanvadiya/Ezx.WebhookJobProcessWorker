using Ezx.WebhookJobCreator.Model;
using Ezx.WebhookJobCreator.Services;

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
       rabitMQ.ReceiveProductMessage<WebHookJob>("WebHookJob");
    }

}
