using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using SmartStore.Web.Framework.Controllers;
using System.ComponentModel.DataAnnotations;
using SmartStore.Services.Payments;
using SmartStore.GTPay.Providers;
using SmartStore.Core.Plugins;
using SmartStore.Web.Framework.Plugins;
using SmartStore.Web.Framework.Security;
using SmartStore.GTPay.Models;
using SmartStore.GTPay.Settings;
using SmartStore.Web.Framework.Settings;
using FluentValidation;
using Autofac;

namespace SmartStore.GTPay.Controllers
{
    
    public class GTPayController : PaymentControllerBase
    {
        private readonly IComponentContext _ctx;
        private readonly HttpContextBase _httpContext;
        private readonly IPluginFinder _pluginFinder;
        private readonly PluginMediator _pluginMediator;

        public GTPayController(HttpContextBase httpContext, IComponentContext ctx, IPluginFinder pluginFinder, PluginMediator pluginMediator)
        {
            _pluginMediator = pluginMediator;
            _pluginFinder = pluginFinder;
            _httpContext = httpContext;
            _ctx = ctx;
        }

        private List<SelectListItem> GetTransactModes()
        {
            var list = new List<SelectListItem>
            {
                //new SelectListItem { Text = T("Enums.SmartStore.Core.Domain.Payments.PaymentStatus.Pending"), Value = ((int)TransactMode.Pending).ToString() },
                //new SelectListItem { Text = T("Enums.SmartStore.Core.Domain.Payments.PaymentStatus.Authorized"), Value = ((int)TransactMode.Authorize).ToString() },
                //new SelectListItem { Text = T("Enums.SmartStore.Core.Domain.Payments.PaymentStatus.Paid"), Value = ((int)TransactMode.Paid).ToString() }
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
            //IValidator validator;
            //ValidationResult validationResult = null;
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

        #region ATMCard

        [AdminAuthorize, ChildActionOnly]
        public ActionResult CardConfigure()
        {
            var model = ConfigureGet<GTPayCardConfigurationModel, GTPayCardPaymentSettings>((m, s) =>
            {
                m.TransactionStatus = s.TransactionStatus;
                m.TransactionStatusValues = GetTransactModes();
                m.ExcludedATMCards = s.ExcludedCards.SplitSafe(",");

                //m.AvailableATMCards = ATMCardProvider.CardTypes
                //    .Select(x => new SelectListItem
                //    {
                //        Text = x.Text,
                //        Value = x.Value,
                //        Selected = m.ExcludedATMCards.Contains(x.Value)
                //    })
                //    .ToList();
            });

            return View(model);
        }

        [HttpPost, AdminAuthorize, ChildActionOnly, ValidateInput(false)]
        public ActionResult CardConfigure(GTPayCardConfigurationModel model, FormCollection form)
        {
            if (!ModelState.IsValid)
                return CardConfigure();

            ConfigurePost<GTPayCardConfigurationModel, GTPayCardPaymentSettings>(model, form, s =>
            {
                s.TransactionStatus = model.TransactionStatus;
                s.ExcludedCards = string.Join(",", model.ExcludedATMCards ?? new string[0]);
            });

            return CardConfigure();
        }

        public ActionResult CardPaymentInfo()
        {
            var model = PaymentInfoGet<GTPayCardPaymentInfoModel, GTPayCardPaymentSettings>((m, s) =>
            {
                var excludedCreditCards = s.ExcludedCards.SplitSafe(",");

                foreach (var card in CardProvider.CardTypes)
                {
                    if (!excludedCreditCards.Any(x => x.IsCaseInsensitiveEqual(card.Value)))
                    {
                        m.CardTypes.Add(new SelectListItem
                        {
                            Text = card.Text,
                            Value = card.Value
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

        public ActionResult PayGatewayResponse()
        {
            System.Collections.Specialized.NameValueCollection responseData = _httpContext.Request.Params;

            if (responseData != null)
            {
                ViewBag.gtpay_tranx_id = responseData["gtpay_tranx_id"];
                ViewBag.gtpay_tranx_status_code = responseData["gtpay_tranx_status_code"];
                ViewBag.gtpay_tranx_status_msg = responseData["gtpay_tranx_status_msg"];
                ViewBag.gtpay_tranx_amt = responseData["gtpay_tranx_amt"];
                ViewBag.gtpay_tranx_curr = responseData["gtpay_tranx_curr"];
                ViewBag.gtpay_cust_id = responseData["gtpay_cust_id"];
                ViewBag.gtpay_gway_name = responseData["gtpay_gway_name"];
                ViewBag.gtpay_echo_data = responseData["gtpay_echo_data"];
                ViewBag.gtpay_tranx_amt_small_denom = responseData["gtpay_tranx_amt_small_denom"];
                ViewBag.gtpay_verification_hash = responseData["gtpay_verification_hash"];
            }

            return View();
        }

        #endregion






    }
}