using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using SmartStore.Core.Plugins;
using SmartStore.GTPay.Settings;

namespace SmartStore.GTPay.Providers
{
    [SystemName("Payments.GTPayCashOnDelivery")]
    [FriendlyName("Cash On Delivery (COD)")]
    [DisplayOrder(1)]
    public class CashOnDeliveryProvider : GTPayProviderBase<GTPayCashOnDeliveryPaymentSettings>, IConfigurable
    {
        protected override string GetActionPrefix()
        {
            return "CashOnDelivery";
        }
    }
}