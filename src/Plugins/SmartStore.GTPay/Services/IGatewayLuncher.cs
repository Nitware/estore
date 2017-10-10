using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using SmartStore.Services.Payments;

namespace SmartStore.GTPay.Services
{
    public interface IGatewayLuncher
    {
        //int OrderId { get; set; }
        bool IsSuccessful { get; set; }

        string GtpayMertId { get; }
        string GtpayTranxId { get; }
        string GtpayTranxAmt { get; }
        string GtpayTranxCurr { get; }
        string GtpayCustId { get; }
        string GtpayCustName { get; }
        string GtpayTranxMemo { get; }
        string GtpayNoShowGtbank { get; }
        string GtpayEchoData { get; }
        string GtpayGwayFirst { get; }
        string GtpayGwayName { get; }
        string GtpayHash { get; }
        string GtpayTranxNotiUrl { get; }
        string GtpayTranxStatusCode { get; }
        string GtpayTranxStatusMsg { get; }
        string GatewayMessage { get; }
        string TransactionRef { get; }

        string CreateTransactionRef();
        string GenerateSHA512String(string inputString);
        void Lunch(PostProcessPaymentRequest postProcessPaymentRequest, HttpContextBase httpContext);
        string GetRedirectUrl(HttpRequestBase request, string action, string controller, int id);
    }
}