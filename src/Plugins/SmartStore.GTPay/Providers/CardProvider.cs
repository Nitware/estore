using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using SmartStore.Core.Plugins;
using SmartStore.GTPay.Interfaces;
using SmartStore.GTPay.Settings;
using SmartStore.Services.Payments;
using System.Web.Mvc;
using SmartStore.GTPay.Domain;

namespace SmartStore.GTPay.Providers
{
    [SystemName("Payments.GTPay")]
    [FriendlyName("Card")]
    [DisplayOrder(1)]
    public class CardProvider : GTPayProviderBase<GTPaySettings>, IConfigurable
    {
        private readonly HttpContextBase _httpContext;
        private readonly IGatewayLuncher _gatewayLuncher;
        private readonly IGTPayCurrencyService _supportedCurrencyService;

        public CardProvider(HttpContextBase httpContext, IGatewayLuncher gatewayLuncher, IGTPayCurrencyService supportedCurrencyService)
        {
            _httpContext = httpContext;
            _gatewayLuncher = gatewayLuncher;
            _supportedCurrencyService = supportedCurrencyService;
        }

        //public static List<SelectListItem> CardTypes
        //{
        //    get
        //    {
        //        var cardTypes = new List<SelectListItem>
        //        {
        //            new SelectListItem { Text = "Naira Card", Value = "566" },
        //            new SelectListItem { Text = "Dollar Card", Value = "840" },
        //        };
        //        return cardTypes;
        //    }
        //}

        public override bool RequiresInteraction
        {
            get
            {
                return false;
                //return true;
            }
        }
        public override RecurringPaymentType RecurringPaymentType
        {
            get
            {
                return RecurringPaymentType.Manual;
            }
        }
        public override PaymentMethodType PaymentMethodType
        {
            get
            {
                return PaymentMethodType.Redirection;
            }
        }

        //public override ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        //{
        //    var result = new ProcessPaymentResult();
        //    var settings = CommonServices.Settings.LoadSetting<GTPaySettings>(processPaymentRequest.StoreId);

        //    result.AllowStoringCreditCardNumber = true;

        //    switch (settings.TransactionStatus)
        //    {
        //        case TransactionStatus.Pending:
        //            result.NewPaymentStatus = PaymentStatus.Pending;
        //            break;
        //        case TransactionStatus.Authorize:
        //            result.NewPaymentStatus = PaymentStatus.Authorized;
        //            break;
        //        case TransactionStatus.Paid:
        //            result.NewPaymentStatus = PaymentStatus.Paid;
        //            break;
        //        default:
        //            result.AddError(T("Common.Payment.TranactionTypeNotSupported"));
        //            return result;
        //    }

        //    return result;
        //}


        /// <summary>
        /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        public override void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            if (postProcessPaymentRequest.Order.PaymentStatus == Core.Domain.Payments.PaymentStatus.Paid)
                return;
                        
            int selectedCurrencyId = Convert.ToInt32(_httpContext.Session[_gatewayLuncher.SelectedCurrencyId]);
            GTPaySupportedCurrency supportedCurrency = _supportedCurrencyService.GetSupportedCurrencyById(selectedCurrencyId);
            if (supportedCurrency == null || supportedCurrency.Id <= 0)
            {
                throw new ArgumentNullException("supportedCurrency retreival failed! Please try again");
            }

            GTPaySettings gtpaysettings = _httpContext.Session[_gatewayLuncher.GTPaySettings] as GTPaySettings;
            if (gtpaysettings == null)
            {
                throw new ArgumentNullException("GTPaySettings could not be retreived! Please try again, but contact your system administrator after three unsuccessful trials");
            }

            _gatewayLuncher.Lunch(postProcessPaymentRequest, supportedCurrency, gtpaysettings, _httpContext);
        }
        
        public override ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            var settings = CommonServices.Settings.LoadSetting<GTPaySettings>(processPaymentRequest.StoreId);

            result.AllowStoringCreditCardNumber = true;


            //switch (settings.TransactMode)
            //{
            //    case TransactMode.Pending:
            //        result.NewPaymentStatus = PaymentStatus.Pending;
            //        break;
            //    case TransactMode.Authorize:
            //        result.NewPaymentStatus = PaymentStatus.Authorized;
            //        break;
            //    case TransactMode.Paid:
            //        result.NewPaymentStatus = PaymentStatus.Paid;
            //        break;
            //    default:
            //        {
            //            result.AddError(T("Common.Payment.TranactionTypeNotSupported"));
            //            return result;
            //        }
            //}

            return result;
        }

        public override CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            return new CancelRecurringPaymentResult();
        }

        protected override string GetActionPrefix()
        {
            //return "Manual";
            //return "Card";
            return "";
        }






    }
}