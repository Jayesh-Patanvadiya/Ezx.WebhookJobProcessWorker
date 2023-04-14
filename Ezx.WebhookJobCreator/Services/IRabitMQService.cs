using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezx.WebhookJobCreator.Services
{
    public interface IRabitMQService
    {
        public Task<string> SendProductMessage<T>(T message, string routingKeyName,int ttl);

        Task<List<WebHookJob>> ReceiveProductMessage<WebHookJob>(string routingKeyName);
    }
}
