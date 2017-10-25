using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using SmartStore.Core.Configuration;
using SmartStore.GTPay.Settings;
using SmartStore.Services.Payments;
using SmartStore.GTPay.Controllers;
using SmartStore.Services.Orders;
using SmartStore.Services;
using SmartStore.Core.Domain.Orders;
using System.Web.Routing;
using SmartStore.Core.Domain.Payments;

namespace SmartStore.GTPay.Providers
{
    public abstract class GTPayProviderBase<TSetting> : PaymentMethodBase where TSetting : GTPaySettings, ISettings, new()
    {
        public ICommonServices CommonServices { get; set; }
        public IOrderTotalCalculationService OrderTotalCalculationService { get; set; }

        public override Type GetControllerType()
        {
            return typeof(GTPayController);
        }

        public override PaymentMethodType PaymentMethodType
        {
            get
            {
                return PaymentMethodType.Standard;
            }
        }

        protected abstract string GetActionPrefix();

        public override decimal GetAdditionalHandlingFee(IList<OrganizedShoppingCartItem> cart)
        {
            var result = decimal.Zero;
            try
            {
                var settings = CommonServices.Settings.LoadSetting<TSetting>(CommonServices.StoreContext.CurrentStore.Id);

                result = this.CalculateAdditionalFee(OrderTotalCalculationService, cart, settings.AdditionalFee, settings.AdditionalFeePercentage);
            }
            catch (Exception)
            {
            }
            return result;
        }

        public override void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "{0}Configure".FormatInvariant(GetActionPrefix());
            controllerName = "GTPay";
            routeValues = new RouteValueDictionary() { { "area", "SmartStore.GTPay" } };
        }

        public override void GetPaymentInfoRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "{0}PaymentInfo".FormatInvariant(GetActionPrefix());
            controllerName = "GTPay";
            routeValues = new RouteValueDictionary() { { "area", "SmartStore.GTPay" } };
        }

        public override ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            result.NewPaymentStatus = PaymentStatus.Pending;
            return result;
        }


    }
}