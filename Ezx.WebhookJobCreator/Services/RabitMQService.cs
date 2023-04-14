using Ezx.WebhookJobCreator.Model;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RestSharp;
using System.Text;

namespace Ezx.WebhookJobCreator.Services
{
    public class RabitMQService : IRabitMQService
    {
        public async Task<string> SendProductMessage<T>(T message, string routingKeyName, int ttl)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())


            using (var channel = connection.CreateModel())
            {

                //a “WebhookJob“ to a RabbitMQ FIFO Queue.A TTL should be set(24 hours).
                var args = new Dictionary<string, object>();
                args.Add("x-message-ttl", 86400000);


                channel.QueueDeclare(queue: routingKeyName,
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: args);


                string messagetemp = Convert.ToString(message);
                var body = Encoding.UTF8.GetBytes(messagetemp);

                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;

                channel.BasicPublish(exchange: "",
                                     routingKey: routingKeyName,
                                     basicProperties: properties,
                                     body: body);
                Console.WriteLine(" [x] Sent {0}", message);

                return messagetemp;
            }


        }


        public async Task<List<WebHookJob>> ReceiveProductMessage<WebHookJob>(string routingKeyName)
        {
            try
            {
                List<WebHookJob> webHookJobsList = new List<WebHookJob>();
                List<string> stringList = new List<string>();

                var factory = new ConnectionFactory() { HostName = "localhost" };

                using (var connection = factory.CreateConnection())
                {
                    using (var channel = connection.CreateModel())
                    {

                        channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                        var consumer = new EventingBasicConsumer(channel);
                        channel.BasicConsume(queue: routingKeyName, autoAck: false, consumer: consumer);

                        consumer.Received += async (model, ea) =>
                        {
                            try
                            {
                                var body = ea.Body.ToArray();
                                var message = Encoding.UTF8.GetString(body);
                                //Console.WriteLine(message);

                                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                                Console.WriteLine(" Recevier Ack  " + ea.DeliveryTag);



                                //If not sucessfull , use DLX to delay requeing of the job for later (60 seconds)

                                // Insert into List
                                stringList.Add(message);
                            }
                            catch (Exception e)
                            {
                                throw new Exception(e.Message);

                            }
                        };
                    }

                }
                Console.WriteLine(stringList.Count);
                foreach (var item in stringList)
                {
                    webHookJobsList.Add(JsonHelper.DeserializeAsync<WebHookJob>(item).Result);
                }

                return webHookJobsList;
            }
            catch (Exception ex)
            {
                Console.WriteLine(" [x] error {0}", ex.Message);
                return new List<WebHookJob>();
            }
        }



        public async Task<string> SendFailProductMessage<T>(T message, string routingKeyName, int ttl)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())


            using (var channel = connection.CreateModel())
            {

                //a “WebhookJob“ to a RabbitMQ FIFO Queue.A TTL should be set(24 hours).
                var args = new Dictionary<string, object>();
                args.Add("x-message-ttl", ttl);
                args.Add("x-dead-letter-exchange", "FailedExchange");
                args.Add("x-dead-letter-routing-key", "FailedExchange-routing-key");
                channel.ExchangeDeclare("FailedExchange", "direct");

                channel.QueueDeclare(queue: routingKeyName,
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: args);


                string messagetemp = Convert.ToString(message);
                var body = Encoding.UTF8.GetBytes(messagetemp);

                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;

                channel.BasicPublish(exchange: "FailedExchange",
                                     routingKey: routingKeyName,
                                     basicProperties: properties,
                                     body: body);
                Console.WriteLine(" [x] Sent {0}", message);

                return messagetemp;
            }


        }

    }
}
