using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezx.WebhookJobCreator.Services
{
    public interface IRabitMQService
    {
        public Task<string> SendWebHookJobMessage<T>(T message, string routingKeyName,int ttl);

        Task<List<WebHookJob>> ReceiveWebHookJobMessage<WebHookJob>(string routingKeyName);

        Task<string> SendFailWebHookJobMessage<T>(T message, string routingKeyName, int ttl);

        Task<List<WebHookJob>> ReceiveSendFailWebHookJobMessage<WebHookJob>(string routingKeyName);
    }
}
