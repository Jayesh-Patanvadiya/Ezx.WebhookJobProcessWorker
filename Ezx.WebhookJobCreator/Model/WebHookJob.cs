using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

    //public class Coupon
    //{
    //    public string Id { get; set; } // firebase unique id


    //    public string? CouponName { get; set; }


     
    //    public int CouponValue { get; set; }

    //    public string ExpirationDate { get; set; }
    //}

}
