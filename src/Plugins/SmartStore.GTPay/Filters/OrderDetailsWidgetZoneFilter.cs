using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using SmartStore.Web.Framework.UI;
using SmartStore.GTPay.Interfaces;

namespace SmartStore.GTPay.Filters
{
    public class OrderDetailsWidgetZoneFilter : ActionFilterAttribute
    {
        private readonly Lazy<HttpContextBase> _httpContext;
        private readonly Lazy<IWidgetProvider> _widgetProvider;
        private IGatewayLuncher _gatewayLuncher;

        public OrderDetailsWidgetZoneFilter(
            Lazy<HttpContextBase> httpContext,
            Lazy<IWidgetProvider> widgetProvider,
            IGatewayLuncher gatewayLuncher)
        {
            _httpContext = httpContext;
            _widgetProvider = widgetProvider;
            _gatewayLuncher = gatewayLuncher;
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

            if (action.IsCaseInsensitiveEqual("Details") && controller.IsCaseInsensitiveEqual("Order"))
            {
                _widgetProvider.Value.RegisterAction("orderdetails_page_overview", "PayGatewayResponse", "GTPay", new { area = "SmartStore.GTPay" });
            }
        }

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            if (!_gatewayLuncher.IsSuccessful)
            {
                filterContext.Result = new RedirectToRouteResult
                   (
                       new System.Web.Routing.RouteValueDictionary
                       {
                            { "Controller", "Checkout" },
                            { "Action", "Completed" },
                            { "Area", "" }
                       }
                   );

                filterContext.Result.ExecuteResult(filterContext.Controller.ControllerContext);
            }
        }




    }
}