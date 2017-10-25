using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using SmartStore.Core.Configuration;

namespace SmartStore.GTPay.Settings
{
    public class GTPaySettings : ISettings
    {
        public string DescriptionText { get; set; }
        public decimal AdditionalFee { get; set; }
        public bool AdditionalFeePercentage { get; set; }
        public string HashKey { get; set; }
        public string MerchantId { get; set; }
        public string GatewayPostUrl { get; set; }
        public string GatewayRequeryUrl { get; set; }
        public bool ShowGatewayInterface { get; set; }
        public bool ShowGatewayNameFirst { get; set; }
        
       
    }

}