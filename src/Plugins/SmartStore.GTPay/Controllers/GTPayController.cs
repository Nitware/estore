using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using SmartStore.Web.Framework.Controllers;
using System.ComponentModel.DataAnnotations;
using SmartStore.Services.Payments;
using FluentValidation;
using SmartStore.Web.Framework.Security;
using SmartStore.GTPay.Models;
using SmartStore.GTPay.Settings;
using SmartStore.Web.Framework.Settings;
using Autofac;
using SmartStore.GTPay.Providers;
using SmartStore.Core.Plugins;
using SmartStore.Web.Framework.Plugins;
using SmartStore.GTPay.Services;
using System.Collections.Specialized;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text;

namespace SmartStore.GTPay.Controllers
{
    public class GTPayController : PaymentControllerBase
    {
        private readonly IComponentContext _ctx;
        private readonly HttpContextBase _httpContext;
        private readonly IPluginFinder _pluginFinder;
        private readonly PluginMediator _pluginMediator;
        private readonly IGatewayLuncher _gatewayLuncher;

        public GTPayController(HttpContextBase httpContext, IComponentContext ctx, IPluginFinder pluginFinder, PluginMediator pluginMediator, IGatewayLuncher gatewayLuncher)
        {
            _gatewayLuncher = gatewayLuncher;
            _pluginMediator = pluginMediator;
            _pluginFinder = pluginFinder;
            _httpContext = httpContext;
            _ctx = ctx;
        }

        private List<SelectListItem> GetAllTransactStatus()
        {
            var list = new List<SelectListItem>
            {
                new SelectListItem { Text = "Pending", Value = ((int)TransactionStatus.Pending).ToString() },
                new SelectListItem { Text = "Successful", Value = ((int)TransactionStatus.Successful).ToString() },
                new SelectListItem { Text = "Failed", Value = ((int)TransactionStatus.Failed).ToString() }


                //new SelectListItem { Text = T("Enums.SmartStore.Core.Domain.Payments.PaymentStatus.Pending"), Value = ((int)TransactMode.Pending).ToString() },
                //new SelectListItem { Text = T("Enums.SmartStore.Core.Domain.Payments.PaymentStatus.Authorized"), Value = ((int)TransactMode.Authorize).ToString() },
                //new SelectListItem { Text = T("Enums.SmartStore.Core.Domain.Payments.PaymentStatus.Paid"), Value = ((int)TransactMode.Paid).ToString() }
            };

            return list;
        }
        private List<SelectListItem> GetAllSupportedCards()
        {
            var list = new List<SelectListItem>
            {
                new SelectListItem { Text = "Naira Card", Value = "566" },
                new SelectListItem { Text = "Dollar Card", Value = "840" }
            };

            return list;
        }

        [NonAction]
        private TModel ConfigureGet<TModel, TSetting>(Action<TModel, TSetting> fn = null)
            where TModel : GTPayConfigurationModelBase, new()
            where TSetting : GTPayPaymentSettingsBase, new()
        {
            var model = new TModel();

            int storeScope = this.GetActiveStoreScopeConfiguration(Services.StoreService, Services.WorkContext);
            var settings = Services.Settings.LoadSetting<TSetting>(storeScope);

            model.DescriptionText = settings.DescriptionText;
            model.AdditionalFee = settings.AdditionalFee;
            model.AdditionalFeePercentage = settings.AdditionalFeePercentage;

            if (fn != null)
            {
                fn(model, settings);
            }

            var storeDependingSettingHelper = new StoreDependingSettingHelper(ViewData);
            storeDependingSettingHelper.GetOverrideKeys(settings, model, storeScope, Services.Settings);

            return model;
        }

        [NonAction]
        private void ConfigurePost<TModel, TSetting>(TModel model, FormCollection form, Action<TSetting> fn = null)
            where TModel : GTPayConfigurationModelBase, new()
            where TSetting : GTPayPaymentSettingsBase, new()
        {
            ModelState.Clear();

            var storeDependingSettingHelper = new StoreDependingSettingHelper(ViewData);
            int storeScope = this.GetActiveStoreScopeConfiguration(Services.StoreService, Services.WorkContext);
            var settings = Services.Settings.LoadSetting<TSetting>(storeScope);

            settings.DescriptionText = model.DescriptionText;
            settings.AdditionalFee = model.AdditionalFee;
            settings.AdditionalFeePercentage = model.AdditionalFeePercentage;

            if (fn != null)
            {
                fn(settings);
            }

            storeDependingSettingHelper.UpdateSettings(settings, form, storeScope, Services.Settings);

            NotifySuccess(Services.Localization.GetResource("Admin.Common.DataSuccessfullySaved"));
        }

        [NonAction]
        private TModel PaymentInfoGet<TModel, TSetting>(Action<TModel, TSetting> fn = null)
            where TModel : GTPayInfoModelBase, new()
            where TSetting : GTPayPaymentSettingsBase, new()
        {
            var settings = _ctx.Resolve<TSetting>();
            var model = new TModel();
            model.DescriptionText = GetLocalizedText(settings.DescriptionText);

            if (fn != null)
            {
                fn(model, settings);
            }

            return model;
        }

        private string GetLocalizedText(string text)
        {
            if (text.EmptyNull().StartsWith("@"))
            {
                return T(text.Substring(1));
            }

            return text;
        }

        [NonAction]
        public override IList<string> ValidatePaymentForm(FormCollection form)
        {
            IValidator validator;
            ValidationResult validationResult = null;
            var warnings = new List<string>();

            string type = form["GTPayMethodType"].NullEmpty();
            //string type = form["OfflinePaymentMethodType"].NullEmpty();

            //if (type.HasValue())
            //{
            //    if (type == "Manual")
            //    {
            //        validator = new ManualPaymentInfoValidator(Services.Localization);
            //        var model = new ManualPaymentInfoModel
            //        {
            //            CardholderName = form["CardholderName"],
            //            CardNumber = form["CardNumber"],
            //            CardCode = form["CardCode"]
            //        };
            //        validationResult = validator.Validate(model);
            //    }
            //    else if (type == "DirectDebit")
            //    {
            //        validator = new DirectDebitPaymentInfoValidator(Services.Localization);
            //        var model = new DirectDebitPaymentInfoModel
            //        {
            //            EnterIBAN = form["EnterIBAN"],
            //            DirectDebitAccountHolder = form["DirectDebitAccountHolder"],
            //            DirectDebitAccountNumber = form["DirectDebitAccountNumber"],
            //            DirectDebitBankCode = form["DirectDebitBankCode"],
            //            DirectDebitCountry = form["DirectDebitCountry"],
            //            DirectDebitBankName = form["DirectDebitBankName"],
            //            DirectDebitIban = form["DirectDebitIban"],
            //            DirectDebitBic = form["DirectDebitBic"]
            //        };
            //        validationResult = validator.Validate(model);
            //    }

            //    if (validationResult != null && !validationResult.IsValid)
            //    {
            //        validationResult.Errors.Each(x => warnings.Add(x.ErrorMessage));
            //    }
            //}

            return warnings;
        }

        [NonAction]
        public override ProcessPaymentRequest GetPaymentInfo(FormCollection form)
        {
            var paymentInfo = new ProcessPaymentRequest();

            string type = form["OfflinePaymentMethodType"].NullEmpty();

            if (type.HasValue())
            {
                if (type == "Manual")
                {
                    paymentInfo.CreditCardType = form["CreditCardType"];
                    paymentInfo.CreditCardName = form["CardholderName"];
                    paymentInfo.CreditCardNumber = form["CardNumber"];
                    paymentInfo.CreditCardExpireMonth = int.Parse(form["ExpireMonth"]);
                    paymentInfo.CreditCardExpireYear = int.Parse(form["ExpireYear"]);
                    paymentInfo.CreditCardCvv2 = form["CardCode"];
                }
                else if (type == "DirectDebit")
                {
                    paymentInfo.DirectDebitAccountHolder = form["DirectDebitAccountHolder"];
                    paymentInfo.DirectDebitAccountNumber = form["DirectDebitAccountNumber"];
                    paymentInfo.DirectDebitBankCode = form["DirectDebitBankCode"];
                    paymentInfo.DirectDebitBankName = form["DirectDebitBankName"];
                    paymentInfo.DirectDebitBic = form["DirectDebitBic"];
                    paymentInfo.DirectDebitCountry = form["DirectDebitCountry"];
                    paymentInfo.DirectDebitIban = form["DirectDebitIban"];
                }
                else if (type == "PurchaseOrderNumber")
                {
                    paymentInfo.PurchaseOrderNumber = form["PurchaseOrderNumber"];
                }
            }

            return paymentInfo;
        }

        [NonAction]
        public override string GetPaymentSummary(FormCollection form)
        {
            string type = form["OfflinePaymentMethodType"].NullEmpty();

            if (type.HasValue())
            {
                if (type == "Manual")
                {
                    var number = form["CardNumber"];
                    return "{0}, {1}, {2}".FormatCurrent(
                        form["CreditCardType"],
                        form["CardholderName"],
                        number.Mask(4)
                    );
                }
                else if (type == "DirectDebit")
                {
                    if (form["DirectDebitAccountNumber"].HasValue() && (form["DirectDebitBankCode"].HasValue()) && form["DirectDebitAccountHolder"].HasValue())
                    {
                        var number = form["DirectDebitAccountNumber"];
                        return "{0}, {1}, {2}".FormatCurrent(
                            form["DirectDebitAccountHolder"],
                            form["DirectDebitBankName"].NullEmpty() ?? form["DirectDebitBankCode"],
                            number.Mask(4)
                        );
                    }
                    else if (form["DirectDebitIban"].HasValue())
                    {
                        var number = form["DirectDebitIban"];
                        return number.Mask(8);
                    }
                }
                else if (type == "PurchaseOrderNumber")
                {
                    return form["PurchaseOrderNumber"];
                }
            }

            return null;
        }

        #region Card

        [AdminAuthorize, ChildActionOnly]
        public ActionResult CardConfigure()
        {
            var model = new GTPayCardConfigurationModel();
            
            model.SupportedCardValues = GetAllSupportedCards();
            model.TransactionStatusValues = GetAllTransactStatus();

            //var model = ConfigureGet<GTPayCardConfigurationModel, GTPayCardPaymentSettings>((m, s) =>
            //{
            //    m.TransactionStatus = s.TransactionStatus;
            //    m.TransactionStatusValues = GetAllTransactStatus();

            //    //m.ExcludedATMCards = s.ExcludedCards.SplitSafe(",");

            //    m..Cards = ATMCardProvider.CardTypes
            //        .Select(x => new SelectListItem
            //        {
            //            Text = x.Text,
            //            Value = x.Value,
            //            Selected = m.ExcludedATMCards.Contains(x.Value)
            //        })
            //        .ToList();
            //});

            return View(model);
        }

        [HttpPost, AdminAuthorize, ChildActionOnly, ValidateInput(false)]
        public ActionResult CardConfigure(GTPayCardConfigurationModel model, FormCollection form)
        {
            if (!ModelState.IsValid)
                return CardConfigure();

            //ConfigurePost<GTPayCardConfigurationModel, GTPayCardPaymentSettings>(model, form, s =>
            //{
            //    s.TransactionStatus = model.TransactionStatus;
            //    s.ExcludedCards = string.Join(",", model.ExcludedATMCards ?? new string[0]);
            //});

            return CardConfigure();
        }

        public ActionResult CardPaymentInfo()
        {
            _httpContext.Session[_gatewayLuncher.GatewayMessage] = null;
            _httpContext.Session[_gatewayLuncher.TransactionRef] = null;

            var model = PaymentInfoGet<GTPayCardPaymentInfoModel, GTPayCardPaymentSettings>((m, s) =>
            {
                var excludedCreditCards = s.ExcludedCards.SplitSafe(",");

                foreach (var creditCard in CardProvider.CardTypes)
                {
                    if (!excludedCreditCards.Any(x => x.IsCaseInsensitiveEqual(creditCard.Value)))
                    {
                        m.CardTypes.Add(new SelectListItem
                        {
                            Text = creditCard.Text,
                            Value = creditCard.Value
                        });
                    }
                }
            });

            PluginDescriptor pluginDescriptor = _pluginFinder.GetPluginDescriptorBySystemName("SmartStore.GTPay");
            if (pluginDescriptor != null)
            {
                model.IconUrl = _pluginMediator.GetIconUrl(pluginDescriptor);
            }


            //// years
            //for (int i = 0; i < 15; i++)
            //{
            //    string year = Convert.ToString(DateTime.Now.Year + i);
            //    model.ExpireYears.Add(new SelectListItem { Text = year, Value = year });
            //}

            //// months
            //for (int i = 1; i <= 12; i++)
            //{
            //    string text = (i < 10) ? "0" + i.ToString() : i.ToString();
            //    model.ExpireMonths.Add(new SelectListItem { Text = text, Value = i.ToString() });
            //}

            // set postback values
            var paymentData = _httpContext.GetCheckoutState().PaymentData;
            //model.CardholderName = (string)paymentData.Get("CardholderName");
            //model.CardNumber = (string)paymentData.Get("CardNumber");
            //model.CardCode = (string)paymentData.Get("CardCode");

            //var creditCardType = (string)paymentData.Get("CreditCardType");
            //var selectedCcType = model.ATMCardTypes.Where(x => x.Value.Equals(creditCardType, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            //if (selectedCcType != null)
            //    selectedCcType.Selected = true;

            //var expireMonth = (string)paymentData.Get("ExpireMonth");
            //var selectedMonth = model.ExpireMonths.Where(x => x.Value.Equals(expireMonth, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            //if (selectedMonth != null)
            //    selectedMonth.Selected = true;

            //var expireYear = (string)paymentData.Get("ExpireYear");
            //var selectedYear = model.ExpireYears.Where(x => x.Value.Equals(expireYear, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            //if (selectedYear != null)
            //    selectedYear.Selected = true;

            return PartialView(model);
        }

        #endregion

        
        public ActionResult ConfirmCheckout()
        {
            //string transactionRef = _gatewayLuncher.CreateTransactionRef();
            //return PartialView(transactionRef);
            

            string transactionRef = _gatewayLuncher.CreateTransactionRef();
            if (transactionRef.HasValue())
            {
                transactionRef = "SRI" + transactionRef;
                _httpContext.Session[_gatewayLuncher.TransactionRef] = transactionRef;

                string tranxRefWidget = "<div class=\"comment-box card mb-3 \">";
                tranxRefWidget += "<div class=\"card-header h5\">";
                tranxRefWidget += "Please note the below Transaction Reference Number, because you will need it to track your payment.";
                tranxRefWidget += "</div>";
                tranxRefWidget += "<div class=\"card-block\">";
                tranxRefWidget += "<div class=\"row \">";
                tranxRefWidget += "<div class=\"col-md-4 h4 \">";
                tranxRefWidget += "Transaction Ref.No.:";
                tranxRefWidget += "</div>";
                tranxRefWidget += "<div class=\"col-md-4 h3\">";
                tranxRefWidget += "<label class=\"badge badge-warning\">" + transactionRef + "</label>";
                tranxRefWidget += "</div>";
                tranxRefWidget += "<div class=\"col-md-4\">";
                tranxRefWidget += "</div>";
                tranxRefWidget += "</div>";
                tranxRefWidget += "</div>";
                tranxRefWidget += "</div>";

                return Content(tranxRefWidget);
            }

            return new EmptyResult();
        }

        //public ActionResult PayGatewayResponse()
        //{
        //    NameValueCollection gatewayMessage = _httpContext.Request.Params;

        //    if (gatewayMessage != null)
        //    {
        //        //mertid + tranxid + hashkey
        //        _httpContext.Session["gatewayMessage"] = gatewayMessage;

        //        string hash = null;
        //        string[] gtpay_echo_data = gatewayMessage["gtpay_echo_data"].Split(';');
        //        if (gtpay_echo_data != null && gtpay_echo_data.Length > 0)
        //        {
        //            string textToHash = gtpay_echo_data[0] + gatewayMessage["gtpay_tranx_id"] + gtpay_echo_data[1];
        //            hash = _gatewayLuncher.GenerateSHA512String(textToHash);
        //        }

        //        HttpClient client = new HttpClient();
        //        string jsonResult = client.GetStringAsync(string.Format("https://ibank.gtbank.com/GTPayService/gettransactionstatus.json?mertid={0}&amount={1}&tranxid={2}&hash={3}", "8692", gatewayMessage["gtpay_tranx_amt_small_denom"], gatewayMessage["gtpay_tranx_id"], hash)).Result;
        //        GTPayGatewayResponse gatewayResponse = JsonConvert.DeserializeObject<GTPayGatewayResponse>(jsonResult);


        //        string alertType = "";
        //        string borderColor = "";
        //        string transactionSummary = null;
        //        string thankYou = null;

        //        if (gatewayMessage["gtpay_tranx_status_code"].HasValue() && gatewayMessage["gtpay_tranx_status_code"] == "00")
        //        {
        //            borderColor = "blue";
        //            //borderColor = "#78909C";
        //            alertType = "info";
        //            thankYou = "Your transaction was successful";
        //            transactionSummary = string.Format("An email has been sent to your registered email ({0}). Transaction Reference: {1}", gtpay_echo_data[3], gatewayMessage["gtpay_tranx_id"]);

        //            _gatewayLuncher.IsSuccessful = true;
        //        }
        //        else
        //        {
        //            borderColor = "red";
        //            alertType = "danger";
        //            thankYou = "Your transaction failed!";
        //            transactionSummary = string.Format("It failed with Reason: {0}. Transaction Reference: {1}. Click on Retry link to try again", gatewayMessage["gtpay_tranx_status_msg"], gatewayMessage["gtpay_tranx_id"]);

        //            _gatewayLuncher.IsSuccessful = false;
        //        }

        //        _gatewayLuncher.IsSuccessful = false;


        //        StringBuilder response = new StringBuilder();

        //        //response.Append("<div class=\"heading mt-3\">");
        //        //response.Append("<h1 class=\"heading-title font-weight-light\">" + orderReceived + "</h1>");
        //        //response.Append("</div>");
        //        //response.Append("<h3 class=\"text-muted font-weight-light\">");
        //        //response.Append("@T(\"Checkout.ThankYou\")");
        //        //response.Append("</h3>");

        //        response.Append("<div class=\"card card-block order-review-data-box mb-3\" style=\"border-color:" + borderColor + "\">");
        //        response.Append("<div class=\"terms-of-service alert alert-" + alertType + " mb-3\">");
        //        response.Append("<label class=\"mb-0 form-check-label\">");
        //        response.Append("<span style=\"font-size:25px\">" + thankYou + "</span>");
        //        response.Append("<br />");
        //        response.Append("<span>" + transactionSummary + "</span>");
        //        response.Append("</label>");
        //        response.Append("</div>");
        //        response.Append("<div class=\"row\">");
        //        response.Append("<div class=\"col-md-12\">");

        //        //response.Append("<div class=\"comment-box card mb-3\">");
        //        //response.Append("<div class=\"card-header h5 \">");
        //        //response.Append("Payment Confirmation");
        //        //response.Append("</div>");
        //        //response.Append("<div class=\"card-block\">");

        //        response.Append("<div style=\"margin-bottom:15px\">");
        //        //response.Append("<h2>" + thankYou + "</h2>");
        //        response.Append("<table style=\"margin-top:5px\">");
        //        response.Append("<thead>");
        //        response.Append("<tr>");
        //        response.Append("<td></td>");
        //        response.Append("<td></td>");
        //        response.Append("</tr>");
        //        response.Append("</thead>");
        //        response.Append("<tbody>");

        //        //response.Append("<tr>");
        //        //response.Append("<td>1</td>");
        //        //response.Append("<td>gtpay_tranx_id</td>");
        //        //response.Append("<td>" + gatewayMessage["gtpay_tranx_id"] + "</td>");
        //        //response.Append("</tr>");
        //        //response.Append("<tr>");
        //        //response.Append("<td>2</td>");
        //        //response.Append("<td>gtpay_tranx_status_code</td>");
        //        //response.Append("<td>" + gatewayMessage["gtpay_tranx_status_code"] + "</td>");
        //        //response.Append("</tr>");

        //        //response.Append("<tr>");
        //        ////response.Append("<td>11</td>");
        //        //response.Append("<td>Customer</td>");
        //        //response.Append("<td>" + gtpay_echo_data[2] + "</td>");
        //        //response.Append("</tr>");

        //        //response.Append("<tr>");
        //        ////response.Append("<td>12</td>");
        //        //response.Append("<td>Email</td>");
        //        //response.Append("<td>" + gtpay_echo_data[3] + "</td>");
        //        //response.Append("</tr>");

        //        response.Append("<tr>");
        //        //response.Append("<td>3</td>");
        //        response.Append("<td>Transaction Status</td>");
        //        response.Append("<td>" + gatewayMessage["gtpay_tranx_status_msg"] + "</td>");
        //        response.Append("</tr>");

        //        //response.Append("<tr>");
        //        ////response.Append("<td>4</td>");
        //        //response.Append("<td>Amount</td>");
        //        //response.Append("<td>" + gatewayMessage["gtpay_tranx_amt"] + "</td>");
        //        //response.Append("</tr>");

        //        response.Append("<tr>");
        //        //response.Append("<td>5</td>");
        //        response.Append("<td>Currency</td>");
        //        response.Append("<td>" + gatewayMessage["gtpay_tranx_curr"] + "</td>");
        //        response.Append("</tr>");

        //        //response.Append("<tr>");
        //        //response.Append("<td>6</td>");
        //        //response.Append("<td>gtpay_cust_id</td>");
        //        //response.Append("<td>" + gatewayMessage["gtpay_cust_id"] + "</td>");
        //        //response.Append("</tr>");

        //        //response.Append("<tr>");
        //        //response.Append("<td>7</td>");
        //        //response.Append("<td>gtpay_gway_name</td>");
        //        //response.Append("<td>" + gatewayMessage["gtpay_gway_name"] + "</td>");
        //        //response.Append("</tr>");

        //        //response.Append("<tr>");
        //        //response.Append("<td>8</td>");
        //        //response.Append("<td>gtpay_echo_data</td>");
        //        //response.Append("<td>" + gatewayMessage["gtpay_echo_data"] + "</td>");
        //        //response.Append("</tr>");

        //        //response.Append("<tr>");
        //        //response.Append("<td>9</td>");
        //        //response.Append("<td>gtpay_tranx_amt_small_denom</td>");
        //        //response.Append("<td>" + gatewayMessage["gtpay_tranx_amt_small_denom"] + "</td>");
        //        //response.Append("</tr>");
        //        //response.Append("<tr>");
        //        //response.Append("<td>10</td>");
        //        //response.Append("<td>gtpay_verification_hash</td>");
        //        //response.Append("<td>" + gatewayMessage["gtpay_verification_hash"] + "</td>");
        //        //response.Append("</tr>");

        //        response.Append("<tr>");
        //        //response.Append("<td>13</td>");
        //        response.Append("<td>Merchant Reference</td>");
        //        response.Append("<td>" + gatewayResponse.MerchantReference + "</td>");
        //        response.Append("</tr>");

        //        response.Append("<tr>");
        //        //response.Append("<td>14</td>");
        //        response.Append("<td>Response Code</td>");
        //        response.Append("<td>" + gatewayResponse.ResponseCode + "</td>");
        //        response.Append("</tr>");

        //        response.Append("<tr>");
        //        //response.Append("<td>15</td>");
        //        response.Append("<td>Response Description</td>");
        //        response.Append("<td>" + gatewayResponse.ResponseDescription + "</td>");
        //        response.Append("</tr>");

        //        response.Append("</tbody>");
        //        response.Append("</table>");
        //        response.Append("</div>");

        //        //response.Append("</div>");
        //        //response.Append("</div>");

        //        response.Append("</div>");
        //        response.Append("</div>");
        //        response.Append("</div>");

        //        return Content(response.ToString());
        //    }

        //    return new EmptyResult();
        //}


        public ActionResult PayGatewayResponse()
        {
            NameValueCollection gatewayMessage = GetGatewayResponse();

            StringBuilder response = new StringBuilder();
            if (gatewayMessage != null)
            {
                //mertid + tranxid + hashkey
                _httpContext.Session[_gatewayLuncher.GatewayMessage] = gatewayMessage;

                string hash = null;
                string[] gtpay_echo_data = gatewayMessage[_gatewayLuncher.GtpayEchoData].Split(';');
                if (gtpay_echo_data != null && gtpay_echo_data.Length > 0)
                {
                    string textToHash = gtpay_echo_data[0] + gatewayMessage[_gatewayLuncher.GtpayTranxId] + gtpay_echo_data[1];
                    hash = _gatewayLuncher.GenerateSHA512String(textToHash);
                }

                HttpClient client = new HttpClient();
                string jsonResult = client.GetStringAsync(string.Format("https://ibank.gtbank.com/GTPayService/gettransactionstatus.json?mertid={0}&amount={1}&tranxid={2}&hash={3}", "8692", gatewayMessage["gtpay_tranx_amt_small_denom"], gatewayMessage["gtpay_tranx_id"], hash)).Result;
                GTPayGatewayResponse gatewayResponse = JsonConvert.DeserializeObject<GTPayGatewayResponse>(jsonResult);


                string alertType = "";
                string borderColor = "";
                string transactionSummary = null;
                string thankYou = null;

                if (gatewayMessage[_gatewayLuncher.GtpayTranxStatusCode].HasValue() && gatewayMessage[_gatewayLuncher.GtpayTranxStatusCode] == "00")
                {
                    borderColor = "blue";
                    //borderColor = "#78909C";
                    alertType = "info";
                    thankYou = "Your transaction was successful";
                    transactionSummary = string.Format("An email has been sent to your registered email ({0}). Transaction Reference: {1}", gtpay_echo_data[3], gatewayMessage[_gatewayLuncher.GtpayTranxId]);

                    _gatewayLuncher.IsSuccessful = true;
                }
                else
                {
                    borderColor = "red";
                    alertType = "danger";
                    thankYou = "Your transaction failed!";
                    transactionSummary = string.Format("It failed with Reason: {0}. Transaction Reference: {1}. Click on Retry link to try again", gatewayMessage[_gatewayLuncher.GtpayTranxStatusMsg], gatewayMessage[_gatewayLuncher.GtpayTranxId]);

                    _gatewayLuncher.IsSuccessful = false;
                }
                
                //response.Append("<div class=\"heading mt-3\">");
                //response.Append("<h1 class=\"heading-title font-weight-light\">" + orderReceived + "</h1>");
                //response.Append("</div>");
                //response.Append("<h3 class=\"text-muted font-weight-light\">");
                //response.Append("@T(\"Checkout.ThankYou\")");
                //response.Append("</h3>");

                response.Append("<div class=\"card card-block order-review-data-box mb-3\" style=\"border-color:" + borderColor + "\">");
                response.Append("<div class=\"terms-of-service alert alert-" + alertType + " mb-3\">");
                response.Append("<label class=\"mb-0 form-check-label\">");
                response.Append("<span style=\"font-size:25px\">" + thankYou + "</span>");
                response.Append("<br />");
                response.Append("<span>" + transactionSummary + "</span>");
                response.Append("</label>");
                response.Append("</div>");
                response.Append("<div class=\"row\">");
                response.Append("<div class=\"col-md-12\">");

                //response.Append("<div class=\"comment-box card mb-3\">");
                //response.Append("<div class=\"card-header h5 \">");
                //response.Append("Payment Confirmation");
                //response.Append("</div>");
                //response.Append("<div class=\"card-block\">");

                response.Append("<div style=\"margin-bottom:15px\">");
                //response.Append("<h2>" + thankYou + "</h2>");
                response.Append("<table style=\"margin-top:5px\">");
                response.Append("<thead>");
                response.Append("<tr>");
                response.Append("<td></td>");
                response.Append("<td></td>");
                response.Append("</tr>");
                response.Append("</thead>");
                response.Append("<tbody>");

                //response.Append("<tr>");
                //response.Append("<td>1</td>");
                //response.Append("<td>gtpay_tranx_id</td>");
                //response.Append("<td>" + gatewayMessage["gtpay_tranx_id"] + "</td>");
                //response.Append("</tr>");
                //response.Append("<tr>");
                //response.Append("<td>2</td>");
                //response.Append("<td>gtpay_tranx_status_code</td>");
                //response.Append("<td>" + gatewayMessage["gtpay_tranx_status_code"] + "</td>");
                //response.Append("</tr>");

                //response.Append("<tr>");
                ////response.Append("<td>11</td>");
                //response.Append("<td>Customer</td>");
                //response.Append("<td>" + gtpay_echo_data[2] + "</td>");
                //response.Append("</tr>");

                //response.Append("<tr>");
                ////response.Append("<td>12</td>");
                //response.Append("<td>Email</td>");
                //response.Append("<td>" + gtpay_echo_data[3] + "</td>");
                //response.Append("</tr>");

                response.Append("<tr>");
                //response.Append("<td>13</td>");
                response.Append("<td>Merchant Reference</td>");
                response.Append("<td>" + gatewayResponse.MerchantReference + "</td>");
                response.Append("</tr>");

                response.Append("<tr>");
                //response.Append("<td>3</td>");
                response.Append("<td>Transaction Status</td>");
                response.Append("<td>" + gatewayMessage[_gatewayLuncher.GtpayTranxStatusMsg] + "</td>");
                response.Append("</tr>");

                //response.Append("<tr>");
                ////response.Append("<td>4</td>");
                //response.Append("<td>Amount</td>");
                //response.Append("<td>" + gatewayMessage["gtpay_tranx_amt"] + "</td>");
                //response.Append("</tr>");

                response.Append("<tr>");
                //response.Append("<td>5</td>");
                response.Append("<td>Currency</td>");
                response.Append("<td>" + gatewayMessage[_gatewayLuncher.GtpayTranxCurr] + "</td>");
                response.Append("</tr>");

                //response.Append("<tr>");
                //response.Append("<td>6</td>");
                //response.Append("<td>gtpay_cust_id</td>");
                //response.Append("<td>" + gatewayMessage["gtpay_cust_id"] + "</td>");
                //response.Append("</tr>");

                //response.Append("<tr>");
                //response.Append("<td>7</td>");
                //response.Append("<td>gtpay_gway_name</td>");
                //response.Append("<td>" + gatewayMessage["gtpay_gway_name"] + "</td>");
                //response.Append("</tr>");

                //response.Append("<tr>");
                //response.Append("<td>8</td>");
                //response.Append("<td>gtpay_echo_data</td>");
                //response.Append("<td>" + gatewayMessage["gtpay_echo_data"] + "</td>");
                //response.Append("</tr>");

                //response.Append("<tr>");
                //response.Append("<td>9</td>");
                //response.Append("<td>gtpay_tranx_amt_small_denom</td>");
                //response.Append("<td>" + gatewayMessage["gtpay_tranx_amt_small_denom"] + "</td>");
                //response.Append("</tr>");
                //response.Append("<tr>");
                //response.Append("<td>10</td>");
                //response.Append("<td>gtpay_verification_hash</td>");
                //response.Append("<td>" + gatewayMessage["gtpay_verification_hash"] + "</td>");
                //response.Append("</tr>");

               

                response.Append("<tr>");
                //response.Append("<td>14</td>");
                response.Append("<td>Response Code</td>");
                response.Append("<td>" + gatewayResponse.ResponseCode + "</td>");
                response.Append("</tr>");

                response.Append("<tr>");
                //response.Append("<td>15</td>");
                response.Append("<td>Response Description</td>");
                response.Append("<td>" + gatewayResponse.ResponseDescription + "</td>");
                response.Append("</tr>");

                response.Append("</tbody>");
                response.Append("</table>");
                response.Append("</div>");

                //response.Append("</div>");
                //response.Append("</div>");

                response.Append("</div>");
                response.Append("</div>");
                response.Append("</div>");

                //return Content(response.ToString());
            }
            else
            {
                response = BuildErrorHtml();
            }

            return Content(response.ToString());
        }

        private StringBuilder BuildErrorHtml()
        {
            string errorTitle = "Error occured!";
            string errorMessage = "Error occured during request processing! Plesae try again";
            _gatewayLuncher.IsSuccessful = false;

            StringBuilder response = new StringBuilder();
            response.Append("<div class=\"card card-block order-review-data-box mb-3\" style=\"border-color:red\">");
            response.Append("<div class=\"terms-of-service alert alert-danger mb-3\">");
            response.Append("<label class=\"mb-0 form-check-label\">");
            response.Append("<span style=\"font-size:25px\">" + errorTitle + "</span>");
            response.Append("<br />");
            response.Append("<span>" + errorMessage + "</span>");
            response.Append("</label>");
            response.Append("</div>");
            response.Append("</div>");

            return response;
        }

        private NameValueCollection GetGatewayResponse()
        {
            
            NameValueCollection gatewayMessage = _httpContext.Request.Params;

            string value = gatewayMessage.Get(_gatewayLuncher.GtpayTranxId);
            if (gatewayMessage == null || !gatewayMessage.Get(_gatewayLuncher.GtpayTranxId).HasValue())
            {
                gatewayMessage = (NameValueCollection)_httpContext.Session[_gatewayLuncher.GatewayMessage];
            }

            return gatewayMessage;
        }

        public ActionResult PayGatewayErrorResponse()
        {
            StringBuilder response = new StringBuilder();
            NameValueCollection gatewayMessage = GetGatewayResponse();
            
            if (gatewayMessage != null)
            {
                string borderColor = "red";
                string alertType = "danger";
                string thankYou = "Your transaction failed!";
                string transactionSummary = string.Format("It failed with Reason: {0}. Transaction Reference: {1}. Click on Retry link to try again", gatewayMessage[_gatewayLuncher.GtpayTranxStatusMsg], gatewayMessage[_gatewayLuncher.GtpayTranxId]);
                string[] gtpay_echo_data = gatewayMessage[_gatewayLuncher.GtpayEchoData].Split(';');

                
                response.Append("<div class=\"card card-block order-review-data-box mb-3\" style=\"border-color:" + borderColor + "\">");
                response.Append("<div class=\"terms-of-service alert alert-" + alertType + " mb-3\">");
                response.Append("<label class=\"mb-0 form-check-label\">");
                response.Append("<span style=\"font-size:25px\">" + thankYou + "</span>");
                response.Append("<br />");
                response.Append("<span>" + transactionSummary + "</span>");
                response.Append("</label>");
                response.Append("</div>");
                response.Append("<div class=\"row\">");
                response.Append("<div class=\"col-md-12\">");

                response.Append("<div style=\"margin-bottom:15px\">");
                response.Append("<table style=\"margin-top:5px\">");
                response.Append("<thead>");
                response.Append("<tr>");
                response.Append("<td></td>");
                response.Append("<td></td>");
                response.Append("</tr>");
                response.Append("</thead>");
                response.Append("<tbody>");

                response.Append("<tr>");
                response.Append("<td>1</td>");
                response.Append("<td>gtpay_tranx_id</td>");
                response.Append("<td>" + gatewayMessage[_gatewayLuncher.GtpayTranxId] + "</td>");
                response.Append("</tr>");
                response.Append("<tr>");
                response.Append("<td>2</td>");
                response.Append("<td>gtpay_tranx_status_code</td>");
                response.Append("<td>" + gatewayMessage[_gatewayLuncher.GtpayTranxStatusCode] + "</td>");
                response.Append("</tr>");

                response.Append("<tr>");
                //response.Append("<td>11</td>");
                response.Append("<td>Customer</td>");
                response.Append("<td>" + gtpay_echo_data[2] + "</td>");
                response.Append("</tr>");

                response.Append("<tr>");
                //response.Append("<td>12</td>");
                response.Append("<td>Email</td>");
                response.Append("<td>" + gtpay_echo_data[3] + "</td>");
                response.Append("</tr>");

                response.Append("<tr>");
                response.Append("<td>Transaction Status</td>");
                response.Append("<td>" + gatewayMessage[_gatewayLuncher.GtpayTranxStatusMsg] + "</td>");
                response.Append("</tr>");

                response.Append("<tr>");
                response.Append("<td>Currency</td>");
                response.Append("<td>" + gatewayMessage[_gatewayLuncher.GtpayTranxCurr] + "</td>");
                response.Append("</tr>");

                //response.Append("<tr>");
                //response.Append("<td>Merchant Reference</td>");
                //response.Append("<td>" + gatewayResponse.MerchantReference + "</td>");
                //response.Append("</tr>");

                //response.Append("<tr>");
                //response.Append("<td>Response Code</td>");
                //response.Append("<td>" + gatewayResponse.ResponseCode + "</td>");
                //response.Append("</tr>");

                //response.Append("<tr>");
                //response.Append("<td>Response Description</td>");
                //response.Append("<td>" + gatewayResponse.ResponseDescription + "</td>");
                //response.Append("</tr>");

                response.Append("</tbody>");
                response.Append("</table>");
                response.Append("</div>");

                response.Append("</div>");
                response.Append("</div>");
                response.Append("</div>");
            }
            else
            {
                response = BuildErrorHtml();
            }

            return Content(response.ToString());

            //return new EmptyResult();
        }








    }
}