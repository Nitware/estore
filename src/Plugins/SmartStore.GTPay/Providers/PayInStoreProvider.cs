using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using SmartStore.GTPay.Settings;
using SmartStore.Core.Plugins;

namespace SmartStore.GTPay.Providers
{
    [SystemName("Payments.GTPayPayInStore")]
    [FriendlyName("Pay In Store")]
    [DisplayOrder(1)]
    public class PayInStoreProvider : GTPayProviderBase<GTPayPayInStorePaymentSettings>, IConfigurable
    {
        protected override string GetActionPrefix()
        {
            return "PayInStore";
        }
    }
}