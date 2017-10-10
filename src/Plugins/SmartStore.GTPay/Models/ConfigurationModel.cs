using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Framework;
using System.Web.Mvc;
using SmartStore.GTPay.Settings;

namespace SmartStore.GTPay.Models
{
    public abstract class GTPayConfigurationModelBase : ModelBase
    {
        [AllowHtml]
        [SmartResourceDisplayName("Plugins.SmartStore.GTPay.DescriptionText")]
        public string DescriptionText { get; set; }

        [SmartResourceDisplayName("Plugins.SmartStore.GTPay.AdditionalFee")]
        public decimal AdditionalFee { get; set; }

        [SmartResourceDisplayName("Plugins.SmartStore.GTPay.AdditionalFeePercentage")]
        public bool AdditionalFeePercentage { get; set; }
    }
    
    public class GTPayCardConfigurationModel : GTPayConfigurationModelBase
    {
        public GTPayCardConfigurationModel()
        {
            SupportedCardValues = new List<SelectListItem>();
            TransactionStatusValues = new List<SelectListItem>();
        }

        [SmartResourceDisplayName("Plugins.Payments.GTPay.Fields.TransactionStatus")]
        public TransactionStatus TransactionStatus { get; set; }
        public List<SelectListItem> TransactionStatusValues { get; set; }

        //[SmartResourceDisplayName("Plugins.Payments.GTPay.ExcludedATMCards")]
        //public string[] ExcludedATMCards { get; set; }

        [SmartResourceDisplayName("Plugins.Payments.GTPay.Fields.SupportedCard")]
        public string SupportedCard { get; set; }
        public List<SelectListItem> SupportedCardValues { get; set; }
        
        [SmartResourceDisplayName("Plugins.Payments.GTPay.Fields.HashKey")]
        public string HashKey { get; set; }

        [SmartResourceDisplayName("Plugins.Payments.GTPay.Fields.MerchantID")]
        public string MerchantID { get; set; }

        //[SmartResourceDisplayName("Plugins.Payments.GTPay.Fields.SupportedCurrency")]
        //public TransactionStatus SupportedCurrency { get; set; }
        //public List<SelectListItem> SupportedCurrencyValues { get; set; }

        [SmartResourceDisplayName("Plugins.Payments.GTPay.Fields.GatewayUrl")]
        public string GatewayUrl { get; set; }
        
    }

  
    

    //public class GTPayPayInStoreConfigurationModel : GTPayConfigurationModelBase { }
    //public class GTPayPrepaymentConfigurationModel : GTPayConfigurationModelBase { }
    //public class GTPayCashOnDeliveryConfigurationModel : GTPayConfigurationModelBase { }



}