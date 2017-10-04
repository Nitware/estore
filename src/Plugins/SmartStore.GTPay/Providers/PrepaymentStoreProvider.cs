using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using SmartStore.GTPay.Settings;
using SmartStore.Core.Plugins;

namespace SmartStore.GTPay.Providers
{
    [SystemName("Payments.GTPayPrepayment")]
    [FriendlyName("Prepayment")]
    [DisplayOrder(1)]
    public class PrepaymentProvider : GTPayProviderBase<GTPayPrepaymentPaymentSettings>, IConfigurable
    {
        protected override string GetActionPrefix()
        {
            return "Prepayment";
        }
    }


}