using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezx.WebhookJobCreator.Model
{
    public class WebHookJobModel
    {
        public string Url { get; set; }
        public int RetryCount { get; set; }
        public string Payload { get; set; }
    }
}
