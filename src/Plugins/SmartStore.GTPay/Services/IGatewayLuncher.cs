using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using SmartStore.Services.Payments;

namespace SmartStore.GTPay.Services
{
    public interface IGatewayLuncher
    {
        void Lunch(PostProcessPaymentRequest postProcessPaymentRequest, HttpContextBase httpContext);
    }
}