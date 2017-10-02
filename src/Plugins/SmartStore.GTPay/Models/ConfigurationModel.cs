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

    //public class GTPayCashOnDeliveryConfigurationModel : GTPayConfigurationModelBase { }

    public class GTPayATMCardConfigurationModel : GTPayConfigurationModelBase
    {
        public List<SelectListItem> TransactionStatusValues { get; set; }
        [SmartResourceDisplayName("Plugins.Payments.GTPay.Fields.TransactionStatus")]
        public TransactionStatus TransactionStatus { get; set; }

        [SmartResourceDisplayName("Plugins.Payments.GTPay.ExcludedATMCards")]
        public string[] ExcludedATMCards { get; set; }
        public List<SelectListItem> AvailableATMCards { get; set; }
    }

    //public class GTPayPayInStoreConfigurationModel : GTPayConfigurationModelBase { }

    //public class GTPayPrepaymentConfigurationModel : GTPayConfigurationModelBase { }
  


}