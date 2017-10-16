using SmartStore.GTPay.Domain;
using SmartStore.Services.Payments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartStore.GTPay.Interfaces
{
    public interface IGatewayLuncher
    {
        //int OrderId { get; set; }
        bool IsSuccessful { get; set; }

        string GtpayMertId { get; }
        string GtpayMertIdValue { get; }
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
        string GtpayTranxAmtSmallDenom { get; }
        string ManInTheMiddleAttackMessage { get; }
        string IsManInTheMiddleAttack { get; }
        string ErrorOccurred { get; }
        string ErrorMessage { get; }
        string GtpayVerificationHash { get; }

        string CreateTransactionRef();
        string GenerateSHA512String(string inputString);
        void Lunch(PostProcessPaymentRequest postProcessPaymentRequest, GTPaySupportedCurrency supportedCurrency, HttpContextBase httpContext);
        string GetRedirectUrl(HttpRequestBase request, string action, string controller, int id = 0);

    }
}