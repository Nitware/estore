using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Web.Framework.Routing;

namespace SmartStore.GTPay
{
    public class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("SmartStore.GTPay",
                 "Plugins/SmartStore.GTPay/{action}",
                 new { controller = "GTPay", action = "Index" },
                 new[] { "SmartStore.GTPay.Controllers" }
            )
            .DataTokens["area"] = "SmartStore.GTPay";
        }
        public int Priority
        {
            get
            {
                return 0;
            }
        }



    }
}