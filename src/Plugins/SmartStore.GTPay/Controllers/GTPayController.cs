using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using SmartStore.Web.Framework.Controllers;
using System.ComponentModel.DataAnnotations;
using SmartStore.Services.Payments;
using SmartStore.Web.Framework.Security;
using Autofac;
using SmartStore.Core.Plugins;
using SmartStore.Web.Framework.Plugins;
using System.Collections.Specialized;
using Newtonsoft.Json;
using System.Text;
using SmartStore.Core.Logging;
using SmartStore.Core.Domain.Directory;
using SmartStore.Services.Configuration;
using SmartStore.GTPay.Interfaces;
using Telerik.Web.Mvc;
using SmartStore.GTPay.Models;
using System.Net.Http;
using SmartStore.GTPay.Settings;
using SmartStore.GTPay.Providers;
using SmartStore.Core;
using SmartStore.Core.Domain.Orders;
using SmartStore.Services.Customers;
using SmartStore.GTPay.Domain;
using SmartStore.Core.Domain.Payments;
using SmartStore.Services.Orders;
using SmartStore.Core.Domain.Common;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Settings;
using SmartStore.Services.Stores;

namespace SmartStore.GTPay.Controllers
{
    public class GTPayController : PaymentControllerBase
    {
        private readonly IComponentContext _ctx;
        private readonly HttpContextBase _httpContext;
        private readonly IPluginFinder _pluginFinder;
        private readonly PluginMediator _pluginMediator;
        private readonly IGatewayLuncher _gatewayLuncher;
        private readonly ISettingService _settingService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderService _orderService;
        private readonly ILogger _logger;
        private readonly ITransactionLogService _transactionLogService;
        private readonly IGTPayCurrencyService _supportedCurrencyService;
        private readonly AdminAreaSettings _adminAreaSettings;
        private readonly IStoreService _storeService;

        public GTPayController(
            HttpContextBase httpContext,
            IComponentContext ctx,
            IPluginFinder pluginFinder,
            PluginMediator pluginMediator,
            IGatewayLuncher gatewayLuncher,
            ILogger logger,
            ISettingService settingService,
            IWorkContext workContext,
            IStoreContext storeContext,
            IOrderProcessingService orderProcessingService,
            IOrderService orderService,
            IGTPayCurrencyService supportedCurrencyService,
            ITransactionLogService transactionLogService,
            AdminAreaSettings adminAreaSettings,
            IStoreService storeService)
        {
            _logger = logger;
            _workContext = workContext;
            _storeContext = storeContext;
            _gatewayLuncher = gatewayLuncher;
            _pluginMediator = pluginMediator;
            _pluginFinder = pluginFinder;
            _httpContext = httpContext;
            _settingService = settingService;
            _transactionLogService = transactionLogService;
            _orderProcessingService = orderProcessingService;
            _supportedCurrencyService = supportedCurrencyService;
            _adminAreaSettings = adminAreaSettings;
            _storeService = storeService;
            _orderService = orderService;
            _ctx = ctx;
        }

        public ActionResult FindTransactionBy(string transactionRef)
        {
            List<GTPayTransactionLog> transactionLogs = _transactionLogService.GetLatest500Transactions();
            List<TransactionLog> transactionLogModels = PrepareTransactionLogModel(transactionLogs);

            transactionLogModels = transactionLogModels.Take(7).ToList();

            ConfigurationModel model = new ConfigurationModel();
            var gridModel = new GridModel<TransactionLog>
            {
                Data = transactionLogModels,
                Total = transactionLogModels.Count
            };

            model.GridPageSize = _adminAreaSettings.GridPageSize;
            model.TransactionLogsForGrid = gridModel;


            return PartialView("_TransactionList", model);
        }

        public ActionResult TransactionLog()
        {
            List<GTPayTransactionLog> transactionLogs = _transactionLogService.GetLatest500Transactions();
            List<TransactionLog> transactionLogModels = PrepareTransactionLogModel(transactionLogs);

            ConfigurationModel model = new ConfigurationModel();
            var gridModel = new GridModel<TransactionLog>
            {
                Data = transactionLogModels,
                Total = transactionLogModels.Count
            };

            model.GridPageSize = _adminAreaSettings.GridPageSize;
            model.TransactionLogsForGrid = gridModel;

            return View(model);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult TransactionLog(TransactionLog model, GridCommand command)
        {
            GTPaySettings gtpaySettings = GetGTPaySettings();

            if (gtpaySettings == null)
                throw new Exception("GTPay settings failed on retrieval!");

            //re-query transaction
            string hash = CreateTransactionRequeryHashKey(gtpaySettings.MerchantId, model.TransactionRefNo, gtpaySettings.HashKey);
            GTPayGatewayResponse gtpayResponse = GetTransactionDetailBy(gtpaySettings.MerchantId, model.AmountInUnit.ToString(), model.TransactionRefNo, hash);

            //update local transaction log
            UpdateTransactionLog(gtpayResponse, model.TransactionRefNo);

            //re-load transaction log to get the update
            List<GTPayTransactionLog> transactionLogs = _transactionLogService.GetLatest500Transactions();
            List<TransactionLog> transactionLogModels = PrepareTransactionLogModel(transactionLogs);

            var tmp2 = transactionLogModels.ForCommand(command);
            var gridModel = new GridModel<TransactionLog>
            {
                Data = tmp2,
                Total = tmp2.Count()
            };

            //model.tra

            //return View(model);

            return new JsonResult
            {
                Data = gridModel,
                //JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        //[GridAction(EnableCustomBinding = true)]
        //public ActionResult UpdateTransaction(string tranxId, long amount, GridCommand command)
        //{
        //    GTPaySettings gtpaySettings = GetGTPaySettings();

        //    if (gtpaySettings == null)
        //        throw new Exception("GTPay settings failed on retrieval!");

        //    //re-query transaction
        //    string hash = CreateTransactionRequeryHashKey(gtpaySettings.MerchantId, tranxId, gtpaySettings.HashKey);
        //    GTPayGatewayResponse gtpayResponse = GetTransactionDetailBy(gtpaySettings.MerchantId, amount.ToString(), tranxId, hash);

        //    //update local transaction log
        //    UpdateTransactionLog(gtpayResponse, tranxId);

        //    //re-load transaction log to get the update
        //    List<GTPayTransactionLog> transactionLogs = _transactionLogService.GetLatest500Transactions();
        //    List<TransactionLog> transactionLogModels = PrepareTransactionLogModel(transactionLogs);

        //    var tmp2 = transactionLogModels.ForCommand(command);
        //    var gridModel = new GridModel<TransactionLog>
        //    {
        //        Data = tmp2,
        //        Total = tmp2.Count()
        //    };

        //    return new JsonResult
        //    {
        //        Data = gridModel,
        //        //JsonRequestBehavior = JsonRequestBehavior.AllowGet
        //    };
        //}

        private void UpdateTransactionLog(GTPayGatewayResponse gtpayResponse, string tranxId)
        {
            //public decimal Amount { get; set; }
            //public string MerchantReference { get; set; }
            //public string MertID { get; set; }
            //public string ResponseCode { get; set; }
            //public string ResponseDescription { get; set; }

            GTPayTransactionLog transactionLog = _transactionLogService.GetByTransactionReference(tranxId);
            transactionLog.ResponseCode = gtpayResponse.ResponseCode;
            transactionLog.MerchantReference = gtpayResponse.MerchantReference;
            transactionLog.ResponseDescription = gtpayResponse.ResponseDescription;
            transactionLog.AmountInUnit = (long)gtpayResponse.Amount;
            transactionLog.ApprovedAmount = gtpayResponse.Amount / 100;
            transactionLog.GTPayTransactionStatusId = gtpayResponse.ResponseCode == "00" ? 2 : 3;
            _transactionLogService.Update(transactionLog);
        }
        
       
        [GridAction(EnableCustomBinding = true)]
        public ActionResult CurrencyUpdate(GTPayCurrencyModel model, GridCommand command)
        {
            GTPaySupportedCurrency supportedCurrency = _supportedCurrencyService.GetSupportedCurrencyById(model.Id);
            supportedCurrency.Code = model.Code;
            supportedCurrency.Alias = model.Alias;
            supportedCurrency.Name = model.Name;
            supportedCurrency.Gateway = model.Gateway;
            supportedCurrency.IsSupported = model.IsSupported;
            supportedCurrency.LeastValueUnitMultiplier = model.LeastValueUnitMultiplier;
            _supportedCurrencyService.UpdateSupportedCurrency(supportedCurrency);
            
            List<GTPayCurrencyModel> currencies = new List<GTPayCurrencyModel>();
            foreach (GTPaySupportedCurrency currency in _supportedCurrencyService.GetAllCurrencies())
            {
                currencies.Add(new GTPayCurrencyModel()
                {
                    Id = currency.Id,
                    Code = currency.Code,
                    Alias = currency.Alias,
                    Name = currency.Name,
                    Gateway = currency.Gateway,
                    LeastValueUnitMultiplier = currency.LeastValueUnitMultiplier,
                    IsSupported = currency.IsSupported
                });
            }

            var tmp2 = currencies.ForCommand(command);
            var gridModel = new GridModel<GTPayCurrencyModel>
            {
                Data = tmp2,
                Total = tmp2.Count()
            };

            return new JsonResult { Data = gridModel };
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

            string type = form["GTPayMethodType"].NullEmpty();

            if (type.HasValue())
            {
                if (type == "CardType")
                {
                    //paymentInfo.CreditCardType = form["CardType"];
                    _httpContext.Session[_gatewayLuncher.SelectedCurrencyId] = form["CardType"];
                    _httpContext.Session[_gatewayLuncher.GTPaySettings] = GetGTPaySettings();
                    
                    //paymentInfo.CreditCardName = form["CardholderName"];
                    //paymentInfo.CreditCardNumber = form["CardNumber"];
                    //paymentInfo.CreditCardExpireMonth = int.Parse(form["ExpireMonth"]);
                    //paymentInfo.CreditCardExpireYear = int.Parse(form["ExpireYear"]);
                    //paymentInfo.CreditCardCvv2 = form["CardCode"];
                }
            }

            return paymentInfo;
        }

        [NonAction]
        public override string GetPaymentSummary(FormCollection form)
        {
            string type = form["GTPayMethodType"].NullEmpty();

            if (type.HasValue())
            {
                if (type == "CardType")
                {
                    return form["CardType"];
                    

                    //var number = form["CardNumber"];
                    //return "{0}, {1}, {2}".FormatCurrent(
                    //    form["CreditCardType"],
                    //    form["CardholderName"],
                    //    number.Mask(4)
                    //);
                }
               
            }

            return null;
        }

        #region Card

        //[AdminAuthorize, ChildActionOnly]
        //public ActionResult TransactionList(ConfigurationModel model)

        private List<TransactionLog> PrepareTransactionLogModel(List<GTPayTransactionLog> transactionLogs)
        {
            List<TransactionLog> transactionLogModels = new List<TransactionLog>();
            foreach (GTPayTransactionLog transactionLog in transactionLogs)
            {
                transactionLogModels.Add(new TransactionLog()
                {
                    TransactionRefNo = transactionLog.TransactionRefNo,
                    //GTPayTransactionStatusId = transactionLog.GTPayTransactionStatusId,
                    TransactionStatus = TranslateTransactionStatusBy(transactionLog.GTPayTransactionStatusId),
                    ApprovedAmount = transactionLog.ApprovedAmount,
                    AmountInUnit = transactionLog.AmountInUnit,
                    OrderId = transactionLog.OrderId,
                    ResponseCode = transactionLog.ResponseCode,
                    ResponseDescription = transactionLog.ResponseDescription,
                    DatePaid = transactionLog.DatePaid,
                    MerchantReference = transactionLog.MerchantReference,
                    IsAmountMismatch = transactionLog.IsAmountMismatch,
                    TransactionDate = transactionLog.TransactionDate,
                    CurrencyAlias = transactionLog.CurrencyAlias,
                    Gateway = transactionLog.Gateway,
                    VerificationHash = transactionLog.VerificationHash,
                    FullVerificationHash = transactionLog.FullVerificationHash
                });
            }

            return transactionLogModels;
        }

        [NonAction]
        public GridModel<TransactionLog> GetLatestTransactionList()
        {
            List<TransactionLog> transactionLogModels = null;
            List<GTPayTransactionLog> transactionLogs = _transactionLogService.GetLatest500Transactions();
                        
            GridModel<TransactionLog> transactionsGridData = new GridModel<TransactionLog>();
            if (transactionLogs != null && transactionLogs.Count > 0)
            {
                transactionLogModels = PrepareTransactionLogModel(transactionLogs);
                transactionsGridData = new GridModel<TransactionLog>
                {
                    Data = transactionLogModels,
                    Total = transactionLogModels.Count
                };
            }
            else
            {
                transactionsGridData = new GridModel<TransactionLog>
                {
                    Data = new List<TransactionLog>(),
                    Total = 0
                };
            }
           
            //model.TransactionLogsForGrid = latestTransactionsGridData;
            //model.GridPageSize = _adminAreaSettings.GridPageSize;
            
            return transactionsGridData;
        }

        private string TranslateTransactionStatusBy(int transactionStatusId)
        {
            string status = null;

            switch (transactionStatusId)
            {
                case (int)TransactionStatus.Failed:
                    {
                        status =TransactionStatus.Failed.ToString();
                        break;
                    }
                case (int)TransactionStatus.Pending:
                    {
                        status = TransactionStatus.Pending.ToString();
                        break;
                    }
                case (int)TransactionStatus.Successful:
                    {
                        status = TransactionStatus.Successful.ToString();
                        break;
                    }
            }

            return status;
        }

        [AdminAuthorize, ChildActionOnly]
        public ActionResult Configure()
        {
            List<GTPaySupportedCurrency> supportedCurrencies = _supportedCurrencyService.GetAllCurrencies();
            GridModel<TransactionLog> transactionLogGridModels = GetLatestTransactionList();

            ConfigurationModel model = new ConfigurationModel();
            model.SupportedCurrencies = new List<GTPayCurrencyModel>();
            if (supportedCurrencies != null && supportedCurrencies.Count > 0)
            {
                foreach (GTPaySupportedCurrency currency in supportedCurrencies)
                {
                    model.SupportedCurrencies.Add(new GTPayCurrencyModel() { Id = currency.Id, Alias = currency.Alias, Code = currency.Code, Gateway = currency.Gateway, IsSupported = currency.IsSupported, Name = currency.Name, LeastValueUnitMultiplier = currency.LeastValueUnitMultiplier });
                }
            }
            
            var gridModel = new GridModel<GTPayCurrencyModel>
            {
                Data = model.SupportedCurrencies,
                Total = model.SupportedCurrencies.Count
            };

            model.GTPayCurrencyGrid = gridModel;
            model.GridPageSize = _adminAreaSettings.GridPageSize;
            model.TransactionLogsForGrid = transactionLogGridModels;

            // get and set current setting
            GTPaySettings gtpaySettings = GetGTPaySettings();
            if (gtpaySettings != null)
            {
                model.AdditionalFee = gtpaySettings.AdditionalFee;
                model.AdditionalFeePercentage = gtpaySettings.AdditionalFeePercentage;
                model.ShowGatewayInterface = gtpaySettings.ShowGatewayInterface;
                model.ShowGatewayNameFirst = gtpaySettings.ShowGatewayNameFirst;
                model.GatewayRequeryUrl= gtpaySettings.GatewayRequeryUrl;
                model.DescriptionText = gtpaySettings.DescriptionText;
                model.GatewayPostUrl = gtpaySettings.GatewayPostUrl;
                model.MerchantId = gtpaySettings.MerchantId;
                model.HashKey = gtpaySettings.HashKey;
            }

            return View(model);
        }

        [HttpPost, AdminAuthorize, ChildActionOnly, ValidateInput(false)]
        public ActionResult Configure(ConfigurationModel model, FormCollection form)
        {
            if (!ModelState.IsValid)
                return Configure();

            ModelState.Clear();

            // get current setting
            int storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            GTPaySettings gtpaySettings = _settingService.LoadSetting<GTPaySettings>(storeScope);
            if (gtpaySettings == null)
            {
                gtpaySettings = new GTPaySettings();
            }

            // update settings with new values of the configuration model
            gtpaySettings.AdditionalFee = model.AdditionalFee;
            gtpaySettings.AdditionalFeePercentage = model.AdditionalFeePercentage;
            gtpaySettings.ShowGatewayInterface = model.ShowGatewayInterface;
            gtpaySettings.ShowGatewayNameFirst = model.ShowGatewayNameFirst;
            gtpaySettings.GatewayRequeryUrl = model.GatewayRequeryUrl;
            gtpaySettings.DescriptionText = model.DescriptionText;
            gtpaySettings.GatewayPostUrl = model.GatewayPostUrl;
            gtpaySettings.MerchantId = model.MerchantId;
            gtpaySettings.HashKey = model.HashKey;
            
            // update settings
            var storeDependingSettingHelper = new StoreDependingSettingHelper(ViewData);
            storeDependingSettingHelper.UpdateSettings(gtpaySettings, form, storeScope, _settingService);
            _settingService.ClearCache();

            return Configure();
        }

        private GTPaySettings GetGTPaySettings()
        {
            int storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            return _settingService.LoadSetting<GTPaySettings>(storeScope);
        }

        public ActionResult PaymentInfo()
        {
            _httpContext.Session[_gatewayLuncher.GatewayMessage] = null;
            _httpContext.Session[_gatewayLuncher.TransactionRef] = null;
            _httpContext.Session[_gatewayLuncher.ErrorMessage] = null;
            _httpContext.Session[_gatewayLuncher.ErrorOccurred] = null;
            _httpContext.Session[_gatewayLuncher.IsManInTheMiddleAttack] = null;
            _httpContext.Session[_gatewayLuncher.SelectedCurrencyId] = null;
            _httpContext.Session[_gatewayLuncher.GTPaySettings] = null;

            PluginDescriptor pluginDescriptor = _pluginFinder.GetPluginDescriptorBySystemName("SmartStore.GTPay");
            GTPayCardPaymentInfoModel model = new GTPayCardPaymentInfoModel(pluginDescriptor.PhysicalPath);
            if (pluginDescriptor != null)
            {
                model.IconUrl = _pluginMediator.GetIconUrl(pluginDescriptor);
            }

            model.SupportedCurrencies = _supportedCurrencyService.GetSupportedCurrencies();
            if (model.SupportedCurrencies != null && model.SupportedCurrencies.Count > 0)
            {
                foreach (GTPaySupportedCurrency currency in model.SupportedCurrencies)
                {
                    model.CardTypes.Add(new SelectListItem { Text = currency.Name, Value = currency.Id.ToString() });
                }
            }
            else
            {
                ViewBag.CurrencyLoadMessage = "Supported curreny failed on load! Click the back button and try again.";
            }

            return PartialView(model);
        }

        #endregion

        private string LogTransactionReference()
        {
            string transactionRef = _gatewayLuncher.CreateTransactionRef();
            GTPayTransactionLog transactionLog = new GTPayTransactionLog()
            {
                TransactionDate = DateTime.UtcNow,
                TransactionRefNo = transactionRef,
                GTPayTransactionStatusId = (int)TransactionStatus.Pending,
                //GTPaySupportedCurrencyId = Convert.ToInt32(_httpContext.Session[_gatewayLuncher.SelectedCurrencyId])
            };

            _transactionLogService.Save(transactionLog);

            return transactionRef;
        }

        public ActionResult ConfirmCheckout()
        {
            string transactionRef = LogTransactionReference();
            if (transactionRef.HasValue())
            {
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
       
        private string CreateTransactionRequeryHashKey(string mertid, string tranxid, string hashkey)
        {
            string textToHash = mertid + tranxid + hashkey;
            return _gatewayLuncher.GenerateSHA512String(textToHash);
        }

        public ActionResult PayGatewayResponse()
        {
            _httpContext.Session[_gatewayLuncher.ErrorOccurred] = false;
            _httpContext.Session[_gatewayLuncher.IsManInTheMiddleAttack] = false;

            string[] gtpay_echo_data = null;
            NameValueCollection gatewayMessage = null;
            GTPayGatewayResponse gatewayResponse = null;
            TransactionStatus transactionStatus = TransactionStatus.Pending;
            StringBuilder response = new StringBuilder();

            try
            {
                gatewayMessage = GetGatewayResponse();
                if (gatewayMessage != null)
                {
                    //mertid + tranxid + hashkey
                    _httpContext.Session[_gatewayLuncher.GatewayMessage] = gatewayMessage;

                    string hash = null;
                    gtpay_echo_data = gatewayMessage[_gatewayLuncher.GtpayEchoData].Split(';');
                    if (gtpay_echo_data != null && gtpay_echo_data.Length > 0)
                    {
                        hash = CreateTransactionRequeryHashKey(gtpay_echo_data[0], gatewayMessage[_gatewayLuncher.GtpayTranxId], gtpay_echo_data[1]);
                    }

                    string merchantId = gtpay_echo_data[0];
                    string homePageUrl = _gatewayLuncher.GetRedirectUrl(_httpContext.Request, "Index", "Home");
                    //gatewayResponse = GetTransactionDetailBy(_gatewayLuncher.GtpayMertIdValue, (string)gatewayMessage[_gatewayLuncher.GtpayTranxAmtSmallDenom], gatewayMessage[_gatewayLuncher.GtpayTranxId], hash);

                    gatewayResponse = GetTransactionDetailBy(merchantId, (string)gatewayMessage[_gatewayLuncher.GtpayTranxAmtSmallDenom], gatewayMessage[_gatewayLuncher.GtpayTranxId], hash);
                    if (NoManInTheMiddleAttack(gatewayResponse, gatewayMessage, merchantId))
                    {
                        string alertType = "";
                        string borderColor = "";
                        string transactionSummary = null;
                        string thankYou = null;

                        if (gatewayResponse.ResponseCode == "00")
                        {
                            transactionStatus = TransactionStatus.Successful;
                        }
                        else
                        {
                            transactionStatus = TransactionStatus.Failed;
                        }

                        if (gatewayMessage[_gatewayLuncher.GtpayTranxStatusCode].HasValue() && gatewayMessage[_gatewayLuncher.GtpayTranxStatusCode] == "00")
                        {
                            //borderColor = "#78909C";
                            alertType = "info";
                            borderColor = "blue";
                            thankYou = "Your transaction was successful";
                            transactionSummary = string.Format("An email has been sent to your registered email ({0}). You can print or download your invoice by clicking the above 'Print' or 'Order as PDF' button respectively. To do a re-order, click the 'Re-order' button below or click <a href=\"{2}\"><i class=\"fa fa-home\"></i><span> here</span></a> to go back to home page", gtpay_echo_data[3], gatewayMessage[_gatewayLuncher.GtpayTranxId], homePageUrl);
                            //transactionSummary = string.Format("An email has been sent to your registered email ({0}). Transaction Reference No.: {1}. You can print or download your invoice by clicking the above 'Print' or 'Order as PDF' button respectively. To do a re-order, click the 'Re-order' button below or click <a href=\"{2}\"><i class=\"fa fa-home\"></i><span> here</span></a> to go back to home page", gtpay_echo_data[3], gatewayMessage[_gatewayLuncher.GtpayTranxId], homePageUrl);

                            _gatewayLuncher.IsSuccessful = true;
                        }
                        else
                        {
                            borderColor = "red";
                            alertType = "danger";
                            thankYou = "Your transaction failed!";
                            transactionSummary = string.Format("Your payment failed with reason: {0}. Transaction Reference No.: {1}. Click on 'Continue shopping' button below to try again, or click <a href=\"{2}\"><i class=\"fa fa-home\"></i><span> here</span></a> to go back to home page", gatewayMessage[_gatewayLuncher.GtpayTranxStatusMsg], gatewayMessage[_gatewayLuncher.GtpayTranxId], homePageUrl);

                            _gatewayLuncher.IsSuccessful = false;
                        }
                       
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
                        //response.Append("<h2>" + thankYou + "</h2>");
                        response.Append("<table style=\"margin-top:5px\">");
                        response.Append("<thead>");
                        response.Append("<tr>");
                        response.Append("<td></td>");
                        response.Append("<td></td>");
                        response.Append("</tr>");
                        response.Append("</thead>");
                        response.Append("<tbody>");

                        response.Append("<tr>");
                        response.Append("<td>Currency</td>");
                        response.Append("<td>" + gatewayMessage[_gatewayLuncher.GtpayTranxCurr] + "</td>");
                        response.Append("</tr>");

                        response.Append("<tr>");
                        response.Append("<td>Response Code</td>");
                        response.Append("<td>" + gatewayResponse.ResponseCode + "</td>");
                        response.Append("</tr>");

                        response.Append("<tr>");
                        response.Append("<td>Response Description</td>");
                        response.Append("<td>" + gatewayResponse.ResponseDescription + "</td>");
                        response.Append("</tr>");

                        response.Append("<tr>");
                        response.Append("<td>Merchant Reference</td>");
                        response.Append("<td>" + gatewayResponse.MerchantReference + "</td>");
                        response.Append("</tr>");

                        response.Append("<tr>");
                        response.Append("<td>Transaction Ref. No.</td>");
                        response.Append("<td>" + gatewayMessage[_gatewayLuncher.GtpayTranxId] + "</td>");
                        response.Append("</tr>");

                        response.Append("<tr>");
                        response.Append("<td>Transaction Status</td>");
                        response.Append("<td>" + transactionStatus.ToString() + "</td>");
                        response.Append("</tr>");

                        response.Append("</tbody>");
                        response.Append("</table>");
                        response.Append("</div>");

                        response.Append("</div>");
                        response.Append("</div>");
                        response.Append("</div>");
                    }
                    else
                    {
                        transactionStatus = TransactionStatus.Failed;
                        _httpContext.Session[_gatewayLuncher.IsManInTheMiddleAttack] = true;
                        response = BuildErrorHtml(string.Format(_gatewayLuncher.ManInTheMiddleAttackMessage, homePageUrl));

                        //PayGatewayErrorResponse();
                    }
                }
                else
                {
                    transactionStatus = TransactionStatus.Failed;
                    //LogTransaction(null, gatewayMessage, transactionStatus);
                    _httpContext.Session[_gatewayLuncher.ErrorOccurred] = true;
                    _httpContext.Session[_gatewayLuncher.ErrorMessage] = "Gateway response is empty! Please click on the 'Continue button' below to initiate another transaction.";
                    response = BuildErrorHtml(null);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                _httpContext.Session[_gatewayLuncher.ErrorOccurred] = true;
                _httpContext.Session[_gatewayLuncher.ErrorMessage] = ex.Message;

                transactionStatus = TransactionStatus.Failed;
                response = BuildErrorHtml(ex.Message);
            }

            LogTransaction(gtpay_echo_data, gatewayResponse, gatewayMessage, transactionStatus);
            SetOrderStatus(transactionStatus);

            return Content(response.ToString());
        }
        private void LogTransaction(string[] gtpay_echo_data, GTPayGatewayResponse gatewayResponse, NameValueCollection gatewayMessage, TransactionStatus transactionStatus = TransactionStatus.Pending)
        {
            if (gtpay_echo_data == null)
            {
                throw new ArgumentNullException("Order or Customer was not specified!");
            }

            GTPayTransactionLog transactionLog = _transactionLogService.GetByTransactionReference(gatewayMessage[_gatewayLuncher.GtpayTranxId]);
            if (transactionLog == null || !transactionLog.TransactionRefNo.HasValue())
            {
                throw new ArgumentNullException("Transaction log retreival failed!");
            }
            
            transactionLog.GTPayTransactionStatusId = (int)transactionStatus;
            transactionLog.AmountInUnit = Convert.ToInt64(gatewayMessage[_gatewayLuncher.GtpayTranxAmtSmallDenom]);
            transactionLog.ApprovedAmount = Convert.ToDecimal(gatewayMessage[_gatewayLuncher.GtpayTranxAmt]);
            transactionLog.ResponseCode = gatewayMessage[_gatewayLuncher.GtpayTranxStatusCode];
            transactionLog.ResponseDescription = gatewayMessage[_gatewayLuncher.GtpayTranxStatusMsg];
            transactionLog.VerificationHash = gatewayMessage[_gatewayLuncher.GtpayVerificationHash];
            transactionLog.FullVerificationHash = gatewayMessage[_gatewayLuncher.GtpayFullVerificationHash];
            transactionLog.MerchantReference = gatewayResponse != null ? gatewayResponse.MerchantReference : null;
            transactionLog.IsAmountMismatch = gatewayResponse.Amount != Convert.ToInt32(gatewayMessage[_gatewayLuncher.GtpayTranxAmtSmallDenom]); // ? true : false,
            transactionLog.CurrencyAlias = gatewayMessage[_gatewayLuncher.GtpayTranxCurr];
            transactionLog.Gateway = gatewayMessage[_gatewayLuncher.GtpayGwayName];
            transactionLog.CustomerId = Convert.ToInt32(gtpay_echo_data[4]);
            transactionLog.OrderId = Convert.ToInt32(gtpay_echo_data[5]);

            if (transactionLog.ResponseCode == "00")
            {
                transactionLog.DatePaid = DateTime.UtcNow;
            }

            _transactionLogService.Update(transactionLog);
        }
        private bool NoManInTheMiddleAttack(GTPayGatewayResponse gtpayRequeryResponse, NameValueCollection gtpayResponse, string merchId)
        {
            decimal requeryAmount = gtpayRequeryResponse.Amount;
            string requeryMerchantId = gtpayRequeryResponse.MertID;
            string requeryResponseCode = gtpayRequeryResponse.ResponseCode;
            string requeryResponseDescription = gtpayRequeryResponse.ResponseDescription;

            decimal amount = Convert.ToDecimal(gtpayResponse.Get(_gatewayLuncher.GtpayTranxAmtSmallDenom));
            string merchantId = merchId; // _gatewayLuncher.GtpayMertIdValue; //gtpayResponse[_gatewayLuncher.GtpayMertId]; //; 
            string transactionStatusCode = gtpayResponse[_gatewayLuncher.GtpayTranxStatusCode];
            string transactionStatusDescription = gtpayResponse[_gatewayLuncher.GtpayTranxStatusMsg];

            return requeryAmount == amount && requeryMerchantId == merchantId && requeryResponseCode == transactionStatusCode && requeryResponseDescription == transactionStatusDescription;
        }
        private GTPayGatewayResponse GetTransactionDetailBy(string mertId, string amount, string tranxId, string hash)
        {
            try
            {
                GTPaySettings gtpaySettings = GetGTPaySettings();
                if (gtpaySettings == null)
                {
                    throw new Exception("GTPay settings could not be retrieved!");
                }

                HttpClient client = new HttpClient();
                string jsonResult = client.GetStringAsync(string.Format("{0}?mertid={1}&amount={2}&tranxid={3}&hash={4}", gtpaySettings.GatewayRequeryUrl, mertId, amount, tranxId, hash)).Result;
                return JsonConvert.DeserializeObject<GTPayGatewayResponse>(jsonResult);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                throw;
            }
        }

        private StringBuilder BuildErrorHtml(string error)
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

            if (error.HasValue())
            {
                response.Append("<span>" + error + "</span>");
            }
            else
            {
                response.Append("<span>" + errorMessage + "</span>");
            }

            response.Append("</label>");
            response.Append("</div>");
            response.Append("</div>");

            return response;
        }

        private NameValueCollection GetGatewayResponse()
        {

            NameValueCollection gatewayMessage = _httpContext.Request.Params;
            if (gatewayMessage == null || !gatewayMessage.Get(_gatewayLuncher.GtpayTranxId).HasValue())
            {
                gatewayMessage = (NameValueCollection)_httpContext.Session[_gatewayLuncher.GatewayMessage];
            }

            return gatewayMessage;
        }

        public ActionResult PayGatewayErrorResponse()
        {
            StringBuilder response = new StringBuilder();

            try
            {
                //bool isManInTheMiddleAttack = (bool)_httpContext.Session[_gatewayLuncher.IsManInTheMiddleAttack];
                //if (isManInTheMiddleAttack)
                //{
                //    response = BuildErrorHtml(string.Format(_gatewayLuncher.ManInTheMiddleAttackMessage, _gatewayLuncher.GetRedirectUrl(_httpContext.Request, "Index", "Home")));
                //    return Content(response.ToString());
                //}

                //bool errorOcurred = (bool)_httpContext.Session[_gatewayLuncher.ErrorOccurred];
                //if (errorOcurred)
                //{
                //    response = BuildErrorHtml(_httpContext.Session[_gatewayLuncher.ErrorMessage].ToString());
                //    return Content(response.ToString());
                //}

                NameValueCollection gatewayMessage = GetGatewayResponse();
                if (gatewayMessage != null)
                {
                    string borderColor = "red";
                    string alertType = "danger";
                    string thankYou = "Your transaction failed!";
                    //string transactionSummary = string.Format("Reason: {0}. See below for the details of the error, or click the 'Continue button' below to try again", gatewayMessage[_gatewayLuncher.GtpayTranxStatusMsg], gatewayMessage[_gatewayLuncher.GtpayTranxId]);

                    string transactionSummary = string.Format("Reason: {0} See below for error details. Click the 'Continue shopping' button below to try again", gatewayMessage[_gatewayLuncher.GtpayTranxStatusMsg]);
                    string[] gtpay_echo_data = gatewayMessage[_gatewayLuncher.GtpayEchoData].Split(';');

                    response.Append("<div class=\"heading mt-3\">");
                    response.Append("<h1 class=\"heading-title font-weight-light\">Error occurred!</h1>");
                    response.Append("</div>");
                    //response.Append("<hr/>");

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
                    response.Append("<td>Transaction Ref. No.</td>");
                    response.Append("<td>" + gatewayMessage[_gatewayLuncher.GtpayTranxId] + "</td>");
                    response.Append("</tr>");

                    //response.Append("<tr>");
                    //response.Append("<td>Customer</td>");
                    //response.Append("<td>" + gtpay_echo_data[2] + "</td>");
                    //response.Append("</tr>");

                    //response.Append("<tr>");
                    //response.Append("<td>Email</td>");
                    //response.Append("<td>" + gtpay_echo_data[3] + "</td>");
                    //response.Append("</tr>");

                    response.Append("<tr>");
                    response.Append("<td>Transaction Status</td>");
                    response.Append("<td>" + gatewayMessage[_gatewayLuncher.GtpayTranxStatusMsg] + "</td>");
                    response.Append("</tr>");

                    response.Append("<tr>");
                    response.Append("<td>Transaction Status Code</td>");
                    response.Append("<td>" + gatewayMessage[_gatewayLuncher.GtpayTranxStatusCode] + "</td>");
                    response.Append("</tr>");

                    //response.Append("<tr>");
                    //response.Append("<td>Currency</td>");
                    //response.Append("<td>" + gatewayMessage[_gatewayLuncher.GtpayTranxCurr] + "</td>");
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
                    response = BuildErrorHtml(null);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                response = BuildErrorHtml(ex.Message);
            }

            return Content(response.ToString());
        }

        protected PaymentStatus GetPaymentStatus(TransactionStatus transactionStatus)
        {
            PaymentStatus paymentStatus = PaymentStatus.Pending;

            switch (transactionStatus)
            {
                case TransactionStatus.Failed:
                case TransactionStatus.Pending:
                    {
                        paymentStatus = PaymentStatus.Pending;
                        break;
                    }

                case TransactionStatus.Successful:
                    {
                        paymentStatus = PaymentStatus.Paid;
                        break;
                    }

                default:
                    break;
            }

            return paymentStatus;
        }

        private void SetOrderStatus(TransactionStatus transactionStatus)
        {
            try
            {
                Order order = _orderService.GetOrderById(GTPay.Services.GatewayLuncher.OrderId);
                if (order == null || order.Id <= 0)
                {
                    throw new ArgumentNullException("order");
                }

                if (transactionStatus == TransactionStatus.Successful)
                {
                    if (_orderProcessingService.CanMarkOrderAsPaid(order))
                    {
                        _orderProcessingService.MarkOrderAsPaid(order);
                    }
                }
            }
            catch(Exception ex)
            {
                _logger.Error(ex);
                
            }
        }

        //private void SendMail(int OrderId)
        //{
        //    if (!_permissionService.Authorize(StandardPermissionProvider.ManageEmailAccounts))
        //        return AccessDeniedView();

        //    var emailAccount = _emailAccountService.GetEmailAccountById(model.Id);
        //    if (emailAccount == null)
        //        return RedirectToAction("List");

        //    try
        //    {
        //        if (model.SendTestEmailTo.IsEmpty())
        //        {
        //            NotifyError(T("Admin.Common.EnterEmailAdress"));
        //        }
        //        else
        //        {
        //            var to = new EmailAddress(model.SendTestEmailTo);
        //            var from = new EmailAddress(emailAccount.Email, emailAccount.DisplayName);
        //            var subject = string.Concat(_storeContext.CurrentStore.Name, ". ", T("Admin.Configuration.EmailAccounts.TestingEmail"));
        //            var body = T("Admin.Common.EmailSuccessfullySent");

        //            var msg = new EmailMessage(to, subject, body, from);

        //            _emailSender.SendEmail(new SmtpContext(emailAccount), msg);

        //            NotifySuccess(T("Admin.Configuration.EmailAccounts.SendTestEmail.Success"), false);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        //model.TestEmailShortErrorMessage = ex.ToAllMessages();
        //        //model.TestEmailFullErrorMessage = ex.ToString();
        //    }
        //}








    }
}