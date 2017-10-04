using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using SmartStore.GTPay.Settings;
using SmartStore.Core.Plugins;
using System.Web.Mvc;
using SmartStore.Services.Payments;
using System.Net.Http;
using SmartStore.GTPay.Services;

namespace SmartStore.GTPay.Providers
{
    [SystemName("Payments.GTPay")]
    [FriendlyName("ATM Card")]
    [DisplayOrder(1)]
    public class ATMCardProvider : GTPayProviderBase<GTPayATMCardPaymentSettings>, IConfigurable
    {
        private readonly HttpContextBase _httpContext;
        private readonly IGatewayLuncher _gatewayLuncher;

        public ATMCardProvider(HttpContextBase httpContext, IGatewayLuncher gatewayLuncher)
        {
            _httpContext = httpContext;
            _gatewayLuncher = gatewayLuncher;
        }

        //public static List<SelectListItem> CardTypes
        //{
        //    get
        //    {
        //        var cardTypes = new List<SelectListItem>
        //        {
        //            new SelectListItem { Text = "Visa", Value = "Visa" },
        //            new SelectListItem { Text = "Master Card", Value = "MasterCard" },
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

        public override ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            var settings = CommonServices.Settings.LoadSetting<GTPayATMCardPaymentSettings>(processPaymentRequest.StoreId);

            result.AllowStoringCreditCardNumber = true;

            //switch (settings.TransactionStatus)
            //{
            //    case TransactionStatus.Pending:
            //        result.NewPaymentStatus = PaymentStatus.Pending;
            //        break;
            //    case TransactionStatus.Authorize:
            //        result.NewPaymentStatus = PaymentStatus.Authorized;
            //        break;
            //    case TransactionStatus.Paid:
            //        result.NewPaymentStatus = PaymentStatus.Paid;
            //        break;
            //    default:
            //        result.AddError(T("Common.Payment.TranactionTypeNotSupported"));
            //        return result;
            //}

            return result;
        }


       


        /// <summary>
		/// Post process payment (used by payment gateways that require redirecting to a third-party URL)
		/// </summary>
		/// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
		public override void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            if (postProcessPaymentRequest.Order.PaymentStatus == Core.Domain.Payments.PaymentStatus.Paid)
                return;

            //postProcessPaymentRequest.RedirectUrl = "https://sandbox.interswitchng.com/webpay/pay";
            

            _gatewayLuncher.Lunch(postProcessPaymentRequest, _httpContext);



            //var store = _services.StoreService.GetStoreById(postProcessPaymentRequest.Order.StoreId);
            //var settings = _services.Settings.LoadSetting<PayPalStandardPaymentSettings>(postProcessPaymentRequest.Order.StoreId);

            //var builder = new StringBuilder();
            //builder.Append(settings.GetPayPalUrl());

            //string orderNumber = postProcessPaymentRequest.Order.GetOrderNumber();
            //string cmd = (settings.PassProductNamesAndTotals ? "_cart" : "_xclick");

            //builder.AppendFormat("?cmd={0}&business={1}", cmd, HttpUtility.UrlEncode(settings.BusinessEmail));

            //if (settings.PassProductNamesAndTotals)
            //{
            //    builder.AppendFormat("&upload=1");

            //    int index = 0;
            //    decimal cartTotal = decimal.Zero;
            //    //var caValues = _checkoutAttributeParser.ParseCheckoutAttributeValues(postProcessPaymentRequest.Order.CheckoutAttributesXml);

            //    var lineItems = GetLineItems(postProcessPaymentRequest, out cartTotal);

            //    AdjustLineItemAmounts(lineItems, postProcessPaymentRequest);

            //    foreach (var item in lineItems.OrderBy(x => (int)x.Type))
            //    {
            //        ++index;
            //        builder.AppendFormat("&item_name_" + index + "={0}", HttpUtility.UrlEncode(item.Name));
            //        builder.AppendFormat("&amount_" + index + "={0}", item.AmountRounded.ToString("0.00", CultureInfo.InvariantCulture));
            //        builder.AppendFormat("&quantity_" + index + "={0}", item.Quantity);
            //    }

            //    if (cartTotal > postProcessPaymentRequest.Order.OrderTotal)
            //    {
            //        // Take the difference between what the order total is and what it should be and use that as the "discount".
            //        // The difference equals the amount of the gift card and/or reward points used.
            //        decimal discountTotal = cartTotal - postProcessPaymentRequest.Order.OrderTotal;
            //        discountTotal = Math.Round(discountTotal, 2);

            //        //gift card or rewared point amount applied to cart in SmartStore.NET - shows in Paypal as "discount"
            //        builder.AppendFormat("&discount_amount_cart={0}", discountTotal.ToString("0.00", CultureInfo.InvariantCulture));
            //    }
            //}
            //else
            //{
            //    //pass order total
            //    string totalItemName = "{0} {1}".FormatWith(T("Checkout.OrderNumber"), orderNumber);
            //    builder.AppendFormat("&item_name={0}", HttpUtility.UrlEncode(totalItemName));
            //    var orderTotal = Math.Round(postProcessPaymentRequest.Order.OrderTotal, 2);
            //    builder.AppendFormat("&amount={0}", orderTotal.ToString("0.00", CultureInfo.InvariantCulture));
            //}

            //builder.AppendFormat("&custom={0}", postProcessPaymentRequest.Order.OrderGuid);
            //builder.AppendFormat("&charset={0}", "utf-8");
            //builder.Append(string.Format("&no_note=1&currency_code={0}", HttpUtility.UrlEncode(store.PrimaryStoreCurrency.CurrencyCode)));
            //builder.AppendFormat("&invoice={0}", HttpUtility.UrlEncode(orderNumber));
            //builder.AppendFormat("&rm=2", new object[0]);

            //Address address = null;

            //if (postProcessPaymentRequest.Order.ShippingStatus != ShippingStatus.ShippingNotRequired)
            //{
            //    address = postProcessPaymentRequest.Order.ShippingAddress ?? postProcessPaymentRequest.Order.BillingAddress;

            //    // 0 means the buyer is prompted to include a shipping address.
            //    builder.AppendFormat("&no_shipping={0}", settings.IsShippingAddressRequired ? "2" : "1");
            //}
            //else
            //{
            //    address = postProcessPaymentRequest.Order.BillingAddress;

            //    builder.AppendFormat("&no_shipping=1", new object[0]);
            //}

            //var returnUrl = _services.WebHelper.GetStoreLocation(store.SslEnabled) + "Plugins/SmartStore.PayPal/PayPalStandard/PDTHandler";
            //var cancelReturnUrl = _services.WebHelper.GetStoreLocation(store.SslEnabled) + "Plugins/SmartStore.PayPal/PayPalStandard/CancelOrder";
            //builder.AppendFormat("&return={0}&cancel_return={1}", HttpUtility.UrlEncode(returnUrl), HttpUtility.UrlEncode(cancelReturnUrl));

            ////Instant Payment Notification (server to server message)
            //if (settings.EnableIpn)
            //{
            //    string ipnUrl;
            //    if (String.IsNullOrWhiteSpace(settings.IpnUrl))
            //        ipnUrl = _services.WebHelper.GetStoreLocation(store.SslEnabled) + "Plugins/SmartStore.PayPal/PayPalStandard/IPNHandler";
            //    else
            //        ipnUrl = settings.IpnUrl;
            //    builder.AppendFormat("&notify_url={0}", ipnUrl);
            //}

            //// Address
            //builder.AppendFormat("&address_override={0}", settings.UsePayPalAddress ? "0" : "1");
            //builder.AppendFormat("&first_name={0}", HttpUtility.UrlEncode(address.FirstName));
            //builder.AppendFormat("&last_name={0}", HttpUtility.UrlEncode(address.LastName));
            //builder.AppendFormat("&address1={0}", HttpUtility.UrlEncode(address.Address1));
            //builder.AppendFormat("&address2={0}", HttpUtility.UrlEncode(address.Address2));
            //builder.AppendFormat("&city={0}", HttpUtility.UrlEncode(address.City));
            ////if (!String.IsNullOrEmpty(address.PhoneNumber))
            ////{
            ////    //strip out all non-digit characters from phone number;
            ////    string billingPhoneNumber = System.Text.RegularExpressions.Regex.Replace(address.PhoneNumber, @"\D", string.Empty);
            ////    if (billingPhoneNumber.Length >= 10)
            ////    {
            ////        builder.AppendFormat("&night_phone_a={0}", HttpUtility.UrlEncode(billingPhoneNumber.Substring(0, 3)));
            ////        builder.AppendFormat("&night_phone_b={0}", HttpUtility.UrlEncode(billingPhoneNumber.Substring(3, 3)));
            ////        builder.AppendFormat("&night_phone_c={0}", HttpUtility.UrlEncode(billingPhoneNumber.Substring(6, 4)));
            ////    }
            ////}
            //if (address.StateProvince != null)
            //    builder.AppendFormat("&state={0}", HttpUtility.UrlEncode(address.StateProvince.Abbreviation));
            //else
            //    builder.AppendFormat("&state={0}", "");

            //if (address.Country != null)
            //    builder.AppendFormat("&country={0}", HttpUtility.UrlEncode(address.Country.TwoLetterIsoCode));
            //else
            //    builder.AppendFormat("&country={0}", "");

            //builder.AppendFormat("&zip={0}", HttpUtility.UrlEncode(address.ZipPostalCode));
            //builder.AppendFormat("&email={0}", HttpUtility.UrlEncode(address.Email));

            //postProcessPaymentRequest.RedirectUrl = builder.ToString();
        }


        public override ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            var settings = CommonServices.Settings.LoadSetting<GTPayATMCardPaymentSettings>(processPaymentRequest.StoreId);

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
            return "ATMCard";
        }



       



    }
}