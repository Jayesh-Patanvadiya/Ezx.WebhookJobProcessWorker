using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Reflection;
using Ezx.WebhookJobCreator.Model;
using System.Collections;
using System.Net.Http;
using Newtonsoft.Json;

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


        public async void ReceiveProductMessage<T>(string routingKeyName)
        {
            try
            {
                var factory = new ConnectionFactory() { HostName = "localhost" };
                using (var connection = factory.CreateConnection())

                using (var channel = connection.CreateModel())
                {
                    var args = new Dictionary<string, object>();
                    args.Add("x-message-ttl", 86400000);
                    channel.QueueDeclare(queue: routingKeyName,
                                         durable: true,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: args);

                    channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                    Console.WriteLine(" [*] Waiting for messages.");

                    var consumer = new EventingBasicConsumer(channel);
                    List<string> data = new List<string>();
                    consumer.Received += async (sender, ea) =>
                    {


                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);


                        var jsonDes = await JsonHelper.DeserializeAsync<T>(message);


                        //We can call sms , email or any notification message here
                        Console.WriteLine(" [x] Received {0}", message);
                        Console.WriteLine(" [x] Done");
                        if (message.Length > 0)
                        {
                            var data = await JsonHelper.DeserializeAsync<WebHookJob>(message);


                            if (!string.IsNullOrEmpty(data.Url))
                            {
                                var json = JsonConvert.SerializeObject(data.Payload);
                                var payload = new StringContent(json, Encoding.UTF8, "application/json");

                                var url = data.Url;
                                using var client = new HttpClient();

                                var response = await client.PostAsync(url, payload);

                                var result = await response.Content.ReadAsStringAsync();
                                Console.WriteLine(result);
                                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                                {

                                }
                                else
                                {

                                }
                            }

                        }
                        // Note: it is possible to access the channel via
                        //       ((EventingBasicConsumer)sender).Model here
                        channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);

                    };


                    channel.BasicConsume(queue: routingKeyName,
                                            autoAck: false,
                                            consumer: consumer);

                    Console.ReadKey();
                    Console.WriteLine(" Press [enter] to exit.");
                    Console.ReadLine();

                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(" [x] error {0}", ex.Message);

            }
        }
    }




}
