using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using SmartStore.Web.Framework.UI;

namespace SmartStore.GTPay.Filters
{
    public class CheckoutConfirmWidgetZoneFilter : IActionFilter, IResultFilter
    {
        private readonly Lazy<HttpContextBase> _httpContext;
        private readonly Lazy<IWidgetProvider> _widgetProvider;

        public CheckoutConfirmWidgetZoneFilter(
            Lazy<HttpContextBase> httpContext,
            Lazy<IWidgetProvider> widgetProvider)
        {
            _httpContext = httpContext;
            _widgetProvider = widgetProvider;
        }

        public void OnResultExecuted(ResultExecutedContext filterContext)
        {
        }
        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
        }
        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
        }

        public void OnResultExecuting(ResultExecutingContext filterContext)
        {
            if (filterContext.IsChildAction)
                return;

            // should only run on a full view rendering result
            var result = filterContext.Result as ViewResultBase;
            if (result == null)
                return;

            var controller = filterContext.RouteData.Values["controller"] as string;
            var action = filterContext.RouteData.Values["action"] as string;

            //if (action.IsCaseInsensitiveEqual("Completed") && controller.IsCaseInsensitiveEqual("Checkout"))

            if (action.IsCaseInsensitiveEqual("Confirm") && controller.IsCaseInsensitiveEqual("Checkout"))
            {
                ////var instruct = _httpContext.Value.Session[PayPalPlusProvider.CheckoutCompletedKey] as string;

                //var instruct = "I love this Plugin idea!";

                //if (instruct.HasValue())
                //{
                _widgetProvider.Value.RegisterAction("checkout_confirm_before_summary", "ConfirmCheckout", "GTPay", new { area = "SmartStore.GTPay" });


                //_widgetProvider.Value.RegisterAction("checkout_completed_top", "CheckoutCompleted", "PayPalPlus", new { area = Plugin.SystemName });
                //}
            }
        }



    }
}