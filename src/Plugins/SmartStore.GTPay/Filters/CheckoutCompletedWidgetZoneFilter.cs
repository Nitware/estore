using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using SmartStore.Web.Framework.UI;

namespace SmartStore.GTPay.Filters
{
    public class CheckoutCompletedWidgetZoneFilter : ActionFilterAttribute 
    {
        private readonly Lazy<HttpContextBase> _httpContext;
        private readonly Lazy<IWidgetProvider> _widgetProvider;

        public CheckoutCompletedWidgetZoneFilter(
            Lazy<HttpContextBase> httpContext,
            Lazy<IWidgetProvider> widgetProvider)
        {
            _httpContext = httpContext;
            _widgetProvider = widgetProvider;
        }

        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            if (filterContext.IsChildAction)
                return;

            // should only run on a full view rendering result
            var result = filterContext.Result as ViewResultBase;
            if (result == null)
                return;

            var action = filterContext.RouteData.Values["action"] as string;
            var controller = filterContext.RouteData.Values["controller"] as string;

            if (action.IsCaseInsensitiveEqual("Completed") && controller.IsCaseInsensitiveEqual("Checkout"))
            {
                _widgetProvider.Value.RegisterAction("checkout_completed_top", "PayGatewayErrorResponse", "GTPay", new { area = "SmartStore.GTPay" });
            }

        }




    }
}