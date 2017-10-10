using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartStore.GTPay.Domain
{
    public class GTPaySetting
    {
        public int Id { get; set; }
       

        public string MerchantId { get; set; }
        public string HttPostUrl { get; set; }
        public string HashKey { get; set; }
    }
}