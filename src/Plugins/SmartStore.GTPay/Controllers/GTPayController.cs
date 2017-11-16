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
using SmartStore.Core.Email;
using SmartStore.Services.Messages;
using SmartStore.Core.Domain.Localization;

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
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly LocalizationSettings _localizationSettings;

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
            IStoreService storeService,
            IWorkflowMessageService workflowMessageService,
            LocalizationSettings localizationSettings
            )
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
            _workflowMessageService = workflowMessageService;
            _localizationSettings = localizationSettings;

            _ctx = ctx;
        }

        public ActionResult GetSupportedCurrencies()
        {
            List<GTPayCurrencyModel> currencies = new List<GTPayCurrencyModel>();
            List<GTPaySupportedCurrency> supportedCurrencies = _supportedCurrencyService.GetAllCurrencies();
            if (supportedCurrencies != null && supportedCurrencies.Count > 0)
            {
                foreach (GTPaySupportedCurrency currency in supportedCurrencies)
                {
                    currencies.Add(new GTPayCurrencyModel() { Id = currency.Id, Alias = currency.Alias, Code = currency.Code, Gateway = currency.Gateway, IsSupported = currency.IsSupported, Name = currency.Name, LeastValueUnitMultiplier = currency.LeastValueUnitMultiplier });
                }
            }

            GridModel<GTPayCurrencyModel> gridModel = new GridModel<GTPayCurrencyModel>
            {
                Data = currencies,
                Total = currencies.Count
            };

            return new JsonResult { Data = gridModel };
        }
        
        public ActionResult LoadTransactionList()
        {
            return PartialView("_TransactionList", new ConfigurationModel());
        }
      
        [GridAction(EnableCustomBinding = true)]
        public ActionResult LoadLatestTransactionList(TransactionLog model, GridCommand command)
        {
            var gridModel = new GridModel<TransactionLog>();
            gridModel.Data = Enumerable.Empty<TransactionLog>();

            string transactionRef = TempData["transactionRef"] as string;
            DateTime? transactionDate = TempData["transactionDate"] as DateTime?;

            List<GTPayTransactionLog> transactionLogs = new List<GTPayTransactionLog>();
            if (transactionRef.HasValue() || transactionDate.HasValue)
            {
                if (transactionDate.HasValue && transactionRef.HasValue())
                {
                    transactionLogs = _transactionLogService.GetBy(transactionRef, transactionDate.Value);
                }
                else if (transactionDate.HasValue)
                {
                    transactionLogs = _transactionLogService.GetBy(transactionDate.Value);
                }
                else if (transactionRef.HasValue())
                {
                    GTPayTransactionLog transactionLog = _transactionLogService.GetBy(transactionRef);
                    if (transactionLog != null)
                    {
                        transactionLogs.Add(transactionLog);
                    }
                }
            }
            else
            {
                transactionLogs = _transactionLogService.GetLatest500Transactions();
            }

            List<TransactionLog> transactionLogModels = PrepareTransactionLogModel(transactionLogs);

            if (transactionLogModels != null && transactionLogModels.Count > 0)
            {
                var tmp2 = transactionLogModels.ForCommand(command);
                gridModel.Data = tmp2;
                gridModel.Total = tmp2.Count();
            }

            return new JsonResult { Data = gridModel };
        }

        public ActionResult FindTransactionBy(string transactionRef, DateTime? transactionDate)
        {
            TempData["transactionRef"] = transactionRef;
            TempData["transactionDate"] = transactionDate;
            return PartialView("_TransactionListGrid", new ConfigurationModel());
        }

        private ConfigurationModel PrepareConfigurationModel(List<TransactionLog> transactionLogModels)
        {
            GridModel<TransactionLog> gridModel = new GridModel<TransactionLog>();
            if (transactionLogModels == null || transactionLogModels.Count <= 0)
            {
                gridModel.Data = Enumerable.Empty<TransactionLog>();
                gridModel.Total = 0;
            }
            else
            {
                gridModel.Data = transactionLogModels;
                gridModel.Total = transactionLogModels.Count;
            }

            ConfigurationModel model = new ConfigurationModel();
            model.GridPageSize = _adminAreaSettings.GridPageSize;
            model.TransactionLogsForGrid = gridModel;

            return model;
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult UpdateTransactionLog(TransactionLog model, GridCommand command)
        {
            bool success = true;
            GTPaySettings gtpaySettings = GetGTPaySettings();

            if (gtpaySettings == null)
                throw new Exception(T("Plugins.SmartStore.GTPay.GTPaySettingRetrievalFailedMessage"));
            //throw new Exception("GTPay settings failed on retrieval!");

            try
            {
                //re-query transaction
                string hash = CreateTransactionRequeryHashKey(gtpaySettings.MerchantId, model.TransactionRefNo, gtpaySettings.HashKey);
                GTPayGatewayResponse gtpayResponse = GetTransactionDetailBy(gtpaySettings.MerchantId, model.AmountInUnit.ToString(), model.TransactionRefNo, hash);

                UpdateTransactionLog(gtpayResponse, model.TransactionRefNo, gtpaySettings.TransactionSuccessCode);
            }
            catch (Exception ex)
            {
                success = false;
                NotifyError(T("Plugins.SmartStore.GTPay.GTPayTransactionLogUpdateFailedMessage") + ex.Message);
                //NotifyError("GTPay Transaction Log update operation failed! " + ex.Message);
            }

            //re-load transaction log to get the update
            List<GTPayTransactionLog> transactionLogs = _transactionLogService.GetLatest500Transactions();
            List<TransactionLog> transactionLogModels = PrepareTransactionLogModel(transactionLogs);

            var tmp2 = transactionLogModels.ForCommand(command);
            var gridModel = new GridModel<TransactionLog>
            {
                Data = tmp2,
                Total = tmp2.Count()
            };

            if (success)
            {
                NotifySuccess(T("Plugins.SmartStore.GTPay.GTPayTransactionLogUpdateSuccessfulMessage"));
                //NotifySuccess("GTPay Transaction Log update operation was successful");
            }

            return new JsonResult { Data = gridModel };
        }

        private void UpdateTransactionLog(GTPayGatewayResponse gtpayResponse, string tranxId, string transactionSuccessCode)
        {
            GTPayTransactionLog transactionLog = _transactionLogService.GetBy(tranxId);
            transactionLog.ResponseCode = gtpayResponse.ResponseCode;
            transactionLog.MerchantReference = gtpayResponse.MerchantReference;
            transactionLog.ResponseDescription = gtpayResponse.ResponseDescription;
            transactionLog.AmountInUnit = (long)gtpayResponse.Amount;
            transactionLog.ApprovedAmount = gtpayResponse.Amount / 100;
            transactionLog.GTPayTransactionStatusId = gtpayResponse.ResponseCode == transactionSuccessCode ? (int)TransactionStatus.Successful : (int)TransactionStatus.Failed;
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

            //NotifySuccess("GTPay Supported Currency update operation was successful");

            NotifySuccess(T("Plugins.SmartStore.GTPay.GTPayCurrencyUpdateSuccessfulMessage"));
            return new JsonResult { Data = gridModel };
        }

        //private string GetLocalizedText(string text)
        //{
        //    if (text.EmptyNull().StartsWith("@"))
        //    {
        //        return T(text.Substring(1));
        //    }

        //    return text;
        //}

        [NonAction]
        public override IList<string> ValidatePaymentForm(FormCollection form)
        {
            //IValidator validator;
            //ValidationResult validationResult = null;
            var warnings = new List<string>();

            string type = form["GTPayMethodType"].NullEmpty();

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
                    _httpContext.Session[_gatewayLuncher.SelectedCurrencyId] = form["CardType"];
                    _httpContext.Session[_gatewayLuncher.GTPaySettings] = GetGTPaySettings();
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
                }
            }

            return null;
        }

        #region Card

        private List<TransactionLog> PrepareTransactionLogModel(List<GTPayTransactionLog> transactionLogs)
        {
            List<TransactionLog> transactionLogModels = new List<TransactionLog>();
            if (transactionLogs != null && transactionLogs.Count > 0)
            {
                foreach (GTPayTransactionLog transactionLog in transactionLogs)
                {
                    transactionLogModels.Add(new TransactionLog()
                    {
                        TransactionRefNo = transactionLog.TransactionRefNo,
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

            return transactionsGridData;
        }

        private string TranslateTransactionStatusBy(int transactionStatusId)
        {
            string status = null;

            switch (transactionStatusId)
            {
                case (int)TransactionStatus.Failed:
                    {
                        status = TransactionStatus.Failed.ToString();
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
            //GridModel<TransactionLog> transactionLogGridModels = GetLatestTransactionList();

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
            //model.TransactionLogsForGrid = transactionLogGridModels;

            // get and set current setting
            GTPaySettings gtpaySettings = GetGTPaySettings();
            if (gtpaySettings != null)
            {
                model.AdditionalFee = gtpaySettings.AdditionalFee;
                model.TransactionSuccessCode = gtpaySettings.TransactionSuccessCode;
                model.SendMailOnFailedTransaction = gtpaySettings.SendMailOnFailedTransaction;
                model.AdditionalFeePercentage = gtpaySettings.AdditionalFeePercentage;
                model.ShowGatewayInterface = gtpaySettings.ShowGatewayInterface;
                model.ShowGatewayNameFirst = gtpaySettings.ShowGatewayNameFirst;
                model.GatewayRequeryUrl = gtpaySettings.GatewayRequeryUrl;
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

            // set settings with new values of the configuration model
            gtpaySettings.AdditionalFee = model.AdditionalFee;
            gtpaySettings.TransactionSuccessCode = model.TransactionSuccessCode;
            gtpaySettings.SendMailOnFailedTransaction = model.SendMailOnFailedTransaction;
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

            //NotifySuccess("GTPay setting update operation was successful");

            NotifySuccess(T("Plugins.SmartStore.GTPay.GTPaySettingUpdateSuccessfulMessage"));
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
                ViewBag.CurrencyLoadMessage = T("Plugins.SmartStore.GTPay.GTPayCurrenyFailedOnLoadMessage");

                //ViewBag.CurrencyLoadMessage = "Supported curreny failed on load! Click the back button and try again.";
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
                tranxRefWidget += T("Plugins.SmartStore.GTPay.NoteTransactionReferenceNumberMessage");
                //tranxRefWidget += "Please note the below Transaction Reference Number, because you will need it to track your payment.";
                tranxRefWidget += "</div>";
                tranxRefWidget += "<div class=\"card-block\">";
                tranxRefWidget += "<div class=\"row \">";
                tranxRefWidget += "<div class=\"col-md-4 h4 \">";
                tranxRefWidget += T("Plugins.SmartStore.GTPay.TransactionRefNo");
                //tranxRefWidget += "Transaction Ref. No.:";
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

            GTPaySettings gtpaySetting = _httpContext.Session[_gatewayLuncher.GTPaySettings] as GTPaySettings;

            try
            {
                gatewayMessage = GetGatewayResponse();
                if (gatewayMessage != null)
                {
                    _httpContext.Session[_gatewayLuncher.GatewayMessage] = gatewayMessage;
                    gtpay_echo_data = gatewayMessage[_gatewayLuncher.GtpayEchoData].Split(';');

                    
                    NameValueCollection paymentRequest = _httpContext.Session[_gatewayLuncher.PamentRequestParameter] as NameValueCollection;
                    if (paymentRequest != null)
                    {
                        string manInTheMiddleAttacked = NoManInTheMiddleAttack(paymentRequest, gatewayMessage, gtpay_echo_data[0]);
                        if (!manInTheMiddleAttacked.HasValue())
                        {
                            //mertid + tranxid + hashkey

                            string hash = null;
                            if (gtpay_echo_data != null && gtpay_echo_data.Length > 0)
                            {
                                hash = CreateTransactionRequeryHashKey(paymentRequest[_gatewayLuncher.GtpayMertId], paymentRequest[_gatewayLuncher.GtpayTranxId], gtpay_echo_data[1]);
                                //hash = CreateTransactionRequeryHashKey(gtpay_echo_data[0], gatewayMessage[_gatewayLuncher.GtpayTranxId], gtpay_echo_data[1]);
                            }

                            string merchantId = paymentRequest[_gatewayLuncher.GtpayMertId]; //gtpay_echo_data[0];
                            string homePageUrl = _gatewayLuncher.GetRedirectUrl(_httpContext.Request, "Index", "Home");

                            gatewayResponse = GetTransactionDetailBy(merchantId, (string)paymentRequest[_gatewayLuncher.GtpayTranxAmt], paymentRequest[_gatewayLuncher.GtpayTranxId], hash);
                            manInTheMiddleAttacked = NoManInTheMiddleAttack(gatewayResponse, gatewayMessage, merchantId);
                            if (!manInTheMiddleAttacked.HasValue())
                            {
                                string alertType = "";
                                string borderColor = "";
                                string transactionSummary = null;
                                string thankYou = null;
                                
                                //if (gatewayResponse.ResponseCode == "00")
                                if (gatewayResponse.ResponseCode == gtpaySetting.TransactionSuccessCode)
                                {
                                    transactionStatus = TransactionStatus.Successful;
                                }
                                else
                                {
                                    transactionStatus = TransactionStatus.Failed;
                                }

                                //if (gatewayMessage[_gatewayLuncher.GtpayTranxStatusCode].HasValue() && gatewayMessage[_gatewayLuncher.GtpayTranxStatusCode] == "00")
                                if (gatewayMessage[_gatewayLuncher.GtpayTranxStatusCode].HasValue() && gatewayMessage[_gatewayLuncher.GtpayTranxStatusCode] == gtpaySetting.TransactionSuccessCode)
                                {
                                    alertType = "info";
                                    borderColor = "blue";
                                    thankYou = T("Plugins.SmartStore.GTPay.SuccessfulTransactionMessage");
                                    transactionSummary = string.Format(T("Plugins.SmartStore.GTPay.EmailHasBeenSentMessage"), gtpay_echo_data[3], string.Format("<a href=\"{0}\"><i class=\"fa fa-home\"></i><span>", homePageUrl), "</span></a>");
                                    
                                    //thankYou = "Your transaction was successful";
                                    //transactionSummary = string.Format("An email has been sent to your registered email ({0}). You can print or download your invoice by clicking the above 'Print' or 'Order as PDF' button respectively. To do a re-order, click the 'Re-order' button below or click <a href=\"{2}\"><i class=\"fa fa-home\"></i><span> here</span></a> to go back to home page", gtpay_echo_data[3], gatewayMessage[_gatewayLuncher.GtpayTranxId], homePageUrl);
                                    _gatewayLuncher.IsSuccessful = true;
                                }
                                else
                                {
                                    borderColor = "red";
                                    alertType = "danger";
                                    thankYou = T("Plugins.SmartStore.GTPay.YourTransactionFailedMessage");
                                    //transactionSummary = string.Format(T("Plugins.SmartStore.GTPay.PaymentFailedMessage"), gatewayMessage[_gatewayLuncher.GtpayTranxStatusMsg], string.Format("<a href=\"{0}\"><i class=\"fa fa-home\"></i><span> ", homePageUrl), "</span></a>");

                                    transactionSummary = string.Format(T("Plugins.SmartStore.GTPay.PaymentFailedMessage"), gatewayMessage[_gatewayLuncher.GtpayTranxStatusMsg]);
                                    _httpContext.Session[_gatewayLuncher.ErrorMessage] = transactionSummary;
                                    
                                    //thankYou = "Your transaction failed!";
                                    //transactionSummary = string.Format("Your payment failed with reason: {0}. Transaction Reference No.: {1}. Click on 'Continue shopping' button below to try again, or click <a href=\"{2}\"><i class=\"fa fa-home\"></i><span> here</span></a> to go back to home page", gatewayMessage[_gatewayLuncher.GtpayTranxStatusMsg], gatewayMessage[_gatewayLuncher.GtpayTranxId], homePageUrl);
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
                                response.Append("<table style=\"margin-top:5px\">");
                                response.Append("<thead>");
                                response.Append("<tr>");
                                response.Append("<td></td>");
                                response.Append("<td></td>");
                                response.Append("</tr>");
                                response.Append("</thead>");
                                response.Append("<tbody>");

                                response.Append("<tr>");
                                //response.Append("<td><b>Currency</b></td>");
                                response.Append("<td><b>" + T("Plugins.SmartStore.GTPay.Field.Currency") + "</b></td>");
                                response.Append("<td>" + gatewayMessage[_gatewayLuncher.GtpayTranxCurr] + "</td>");
                                response.Append("</tr>");

                                response.Append("<tr>");
                                //response.Append("<td><b>Response Code</b></td>");
                                response.Append("<td><b>" + T("Plugins.SmartStore.GTPay.Field.ResponseCode") + "</b></td>");
                                response.Append("<td>" + gatewayResponse.ResponseCode + "</td>");
                                response.Append("</tr>");

                                response.Append("<tr>");
                                //response.Append("<td><b>Response Description</b></td>");
                                response.Append("<td><b>" + T("Plugins.SmartStore.GTPay.Field.ResponseDescription") + "</b></td>");
                                response.Append("<td>" + gatewayResponse.ResponseDescription + "</td>");
                                response.Append("</tr>");

                                response.Append("<tr>");
                                //response.Append("<td><b>Merchant Reference</b></td>");
                                response.Append("<td><b>" + T("Plugins.SmartStore.GTPay.Field.MerchantReference") + "</b></td>");
                                response.Append("<td>" + gatewayResponse.MerchantReference + "</td>");
                                response.Append("</tr>");

                                response.Append("<tr>");
                                //response.Append("<td><b>Transaction Ref. No.</b></td>");
                                response.Append("<td><b>" + T("Plugins.SmartStore.GTPay.TransactionRefNo") + "</b></td>");
                                response.Append("<td>" + gatewayMessage[_gatewayLuncher.GtpayTranxId] + "</td>");
                                response.Append("</tr>");

                                response.Append("<tr>");
                                //response.Append("<td><b>Transaction Status</b></td>");
                                response.Append("<td><b>" + T("Plugins.SmartStore.GTPay.Field.TransactionStatus") + "</b></td>");
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
                                _httpContext.Session[_gatewayLuncher.ErrorMessage] = manInTheMiddleAttacked;
                            }
                        }
                        else
                        {
                            transactionStatus = TransactionStatus.Failed;
                            _httpContext.Session[_gatewayLuncher.ErrorMessage] = manInTheMiddleAttacked;
                        }
                    }
                    else
                    {
                        transactionStatus = TransactionStatus.Failed;
                        _httpContext.Session[_gatewayLuncher.ErrorMessage] = T("Plugins.SmartStore.GTPay.PaymentRequestSessionClearedMessage");

                        //_httpContext.Session[_gatewayLuncher.ErrorMessage] = "Payment Request session was accidentally cleared! Please contact your system administrator";
                    }
                }
                else
                {
                    transactionStatus = TransactionStatus.Failed;
                    _httpContext.Session[_gatewayLuncher.ErrorMessage] = T("Plugins.SmartStore.GTPay.EmptyGatewayResponseMessage");
                    
                    //_httpContext.Session[_gatewayLuncher.ErrorMessage] = "Gateway response is empty! Please click on the 'Continue button' below to initiate another transaction.";
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);

                transactionStatus = TransactionStatus.Failed;
                _httpContext.Session[_gatewayLuncher.ErrorMessage] = ex.Message;
            }

            try
            {
                LogTransaction(gtpay_echo_data, gatewayResponse, gatewayMessage, gtpaySetting.TransactionSuccessCode, transactionStatus);

                Order order = _orderService.GetOrderById(GTPay.Services.GatewayLuncher.OrderId);
                SetOrderStatus(order, transactionStatus);

                bool sendMail = gtpaySetting.SendMailOnFailedTransaction == true || _gatewayLuncher.IsSuccessful == true;
                SendEmail(order, sendMail);


                //if (_gatewayLuncher.IsSuccessful)
                //{
                //    SendEmail(order, _gatewayLuncher.IsSuccessful);
                //}
            }
            catch (Exception ex)
            {
                _logger.Error(ex);

                transactionStatus = TransactionStatus.Failed;
                _httpContext.Session[_gatewayLuncher.ErrorMessage] = ex.Message;
            }

            return Content(response.ToString());
        }
        private void LogTransaction(string[] gtpay_echo_data, GTPayGatewayResponse gatewayResponse, NameValueCollection gatewayMessage, string transactionSuccessCode, TransactionStatus transactionStatus = TransactionStatus.Pending)
        {
            if (gtpay_echo_data == null)
            {
                //throw new ArgumentNullException("Order or Customer cannot be retreived!");
                throw new ArgumentNullException(T("Plugins.SmartStore.GTPay.OrderOrCustomerCannotBeRetreivedMessage"));
            }

            GTPayTransactionLog transactionLog = _transactionLogService.GetBy(gatewayMessage[_gatewayLuncher.GtpayTranxId]);
            if (transactionLog == null || !transactionLog.TransactionRefNo.HasValue())
            {
                throw new ArgumentNullException(T("Plugins.SmartStore.GTPay.TransactionLogRetrievalFailedMessage"));
                //throw new ArgumentNullException("Transaction log retrieval failed!");
            }

            //bool amountMismatch = true;
            int amount = Convert.ToInt32(gatewayMessage[_gatewayLuncher.GtpayTranxAmtSmallDenom]);
            bool amountMismatch = (gatewayResponse != null && amount != 0) ? gatewayResponse.Amount != amount : true;
            
            //if (gatewayResponse != null && amount != 0)
            //{
            //    amountMismatch = gatewayResponse.Amount != amount;
            //}

            transactionLog.GTPayTransactionStatusId = (int)transactionStatus;
            transactionLog.AmountInUnit = Convert.ToInt64(gatewayMessage[_gatewayLuncher.GtpayTranxAmtSmallDenom]);
            transactionLog.ApprovedAmount = Convert.ToDecimal(gatewayMessage[_gatewayLuncher.GtpayTranxAmt]);
            transactionLog.ResponseCode = gatewayMessage[_gatewayLuncher.GtpayTranxStatusCode];
            transactionLog.ResponseDescription = gatewayMessage[_gatewayLuncher.GtpayTranxStatusMsg];
            transactionLog.VerificationHash = gatewayMessage[_gatewayLuncher.GtpayVerificationHash];
            transactionLog.FullVerificationHash = gatewayMessage[_gatewayLuncher.GtpayFullVerificationHash];
            transactionLog.MerchantReference = gatewayResponse != null ? gatewayResponse.MerchantReference : null;
            transactionLog.IsAmountMismatch = amountMismatch; // ? true : false,
            transactionLog.CurrencyAlias = gatewayMessage[_gatewayLuncher.GtpayTranxCurr];
            transactionLog.Gateway = gatewayMessage[_gatewayLuncher.GtpayGwayName];
            transactionLog.CustomerId = Convert.ToInt32(gtpay_echo_data[4]);
            transactionLog.OrderId = Convert.ToInt32(gtpay_echo_data[5]);

            if (transactionLog.ResponseCode == transactionSuccessCode)
            {
                transactionLog.DatePaid = DateTime.UtcNow;
            }

            _transactionLogService.Update(transactionLog);
        }
        private string NoManInTheMiddleAttack(GTPayGatewayResponse gtpayRequeryResponse, NameValueCollection gtpayResponse, string merchId)
        {
            StringBuilder messageBuilder = new StringBuilder();
            if (gtpayRequeryResponse.Amount != Convert.ToDecimal(gtpayResponse.Get(_gatewayLuncher.GtpayTranxAmtSmallDenom)))
            {
                messageBuilder.AppendLine(string.Format("<li>" + T("Plugins.SmartStore.GTPay.Attacked.GTPayTransactionAmountModifiedMessage") + "</li>", Convert.ToDecimal(gtpayResponse.Get(_gatewayLuncher.GtpayTranxAmtSmallDenom)), gtpayRequeryResponse.Amount));

                //messageBuilder.AppendLine(string.Format("<li>GTPay Transaction Amount was modified from {0} to {1} in transit</li>", Convert.ToDecimal(gtpayResponse.Get(_gatewayLuncher.GtpayTranxAmtSmallDenom)), gtpayRequeryResponse.Amount));
            }
            if (gtpayRequeryResponse.MertID != merchId)
            {
                messageBuilder.AppendLine("<li>" + T("Plugins.SmartStore.GTPay.Attacked.GTPayMerchantIDModifiedMessage") + "</li>");
                //messageBuilder.AppendLine("<li>GTPay Merchant ID was modified in transit</li>");
            }
            if (gtpayRequeryResponse.ResponseCode != gtpayResponse[_gatewayLuncher.GtpayTranxStatusCode])
            {
                messageBuilder.AppendLine("<li>" + T("Plugins.SmartStore.GTPay.Attacked.GTPayResponseCodeModifiedMessage") + "</li>");
                //messageBuilder.AppendLine("<li>GTPay Response Code was modified in transit</li>");
            }
            if (gtpayRequeryResponse.ResponseDescription != gtpayResponse[_gatewayLuncher.GtpayTranxStatusMsg])
            {
                messageBuilder.AppendLine("<li>" + T("Plugins.SmartStore.GTPay.Attacked.GTPayResponseDescriptionModifiedMessage") + "</li>");
                //messageBuilder.AppendLine("<li>GTPay Response Description was modified in transit</li>");
            }

            if (messageBuilder.Length > 0)
            {
                messageBuilder.Append("</ol>");
                messageBuilder.Append("<div>" + T("Plugins.SmartStore.GTPay.ReportIssueToSystemAdminstratorMessage") + "</div>");
                messageBuilder.Insert(0, "<div>" + T("Plugins.SmartStore.GTPay.ManInTheMiddleAttackOccurredMessage") + " </div><ol>");

                //messageBuilder.Append("<div>Kindly report this issue to your system adminstrator immediately</div>");
                //messageBuilder.Insert(0, "<div>The following 'Man In The Middle Attack' occured during the transction processing! </div><ol>");
            }

            return messageBuilder.ToString();
        }
        private bool TransactionCurrencyIsInvalid(string requestCurrency, string responseCurrency)
        {
            bool isInvalid = true;

            GTPaySupportedCurrency currency = _supportedCurrencyService.GetSupportedCurrencyByCode(requestCurrency.ToInt());
            if (currency == null || currency.Id <= 0)
                throw new Exception(T("Plugins.SmartStore.GTPay.GTPayCurrenyFailedOnRetrievalMessage"));

            if (requestCurrency == currency.Code.ToString() && (responseCurrency == currency.Alias || responseCurrency == currency.Code.ToString()))
            {
                isInvalid = false;
            }
           

            //if (requestCurrency == "566" && (responseCurrency == "NGN" || responseCurrency == "566"))
            //{
            //    isInvalid = false;
            //}
            //else if (requestCurrency == "840" && (responseCurrency == "USD" || responseCurrency == "840"))
            //{
            //    isInvalid = false;
            //}

            return isInvalid;
        }
        private string NoManInTheMiddleAttack(NameValueCollection paymentRequest, NameValueCollection gatewayResponse, string merchantId)
        {
            StringBuilder messageBuilder = new StringBuilder();

            if (paymentRequest[_gatewayLuncher.GtpayMertId] != merchantId)
            {
                messageBuilder.AppendLine("<li>" + T("Plugins.SmartStore.GTPay.Attacked.GTPayMerchantIDModifiedMessage") + "</li>");
                //messageBuilder.Append("<li>GTPay Merchant ID was modified in transit</li>");
            }
            if (TransactionCurrencyIsInvalid(paymentRequest[_gatewayLuncher.GtpayTranxCurr], gatewayResponse[_gatewayLuncher.GtpayTranxCurr]))
            {
                messageBuilder.Append(string.Format("<li>" + T("Plugins.SmartStore.GTPay.Attacked.GTPayTransactionCurrencyModifiedMessage") + "</li>", paymentRequest[_gatewayLuncher.GtpayTranxCurr], gatewayResponse[_gatewayLuncher.GtpayTranxCurr]));
                //messageBuilder.Append(string.Format("<li>GTPay Transaction Currency was modified in transit from '{0}' to '{1}'</li>", paymentRequest[_gatewayLuncher.GtpayTranxCurr], gatewayResponse[_gatewayLuncher.GtpayTranxCurr]));
            }
            if (paymentRequest[_gatewayLuncher.GtpayCustId] != gatewayResponse[_gatewayLuncher.GtpayCustId])
            {
                messageBuilder.Append(string.Format("<li>" + T("Plugins.SmartStore.GTPay.Attacked.GTPayCustomerIDModifiedMessage") + "</li>", paymentRequest[_gatewayLuncher.GtpayCustId], gatewayResponse[_gatewayLuncher.GtpayCustId]));
                //messageBuilder.Append(string.Format("<li>Customer ID was modified in transit from {0} to {1}</li>", paymentRequest[_gatewayLuncher.GtpayCustId], gatewayResponse[_gatewayLuncher.GtpayCustId]));
            }
            if (paymentRequest[_gatewayLuncher.GtpayTranxNotiUrl] != gatewayResponse[_gatewayLuncher.SiteRedirectUrl])
            {
                messageBuilder.Append(string.Format("<li>" + T("Plugins.SmartStore.GTPay.Attacked.GTPayRedirectURLModifiedMessage") + "</li>", paymentRequest[_gatewayLuncher.GtpayTranxNotiUrl], gatewayResponse[_gatewayLuncher.SiteRedirectUrl]));
                //messageBuilder.Append(string.Format("<li>GTPay Redirect Url was modified from in transit from '{0}' to '{1}'</li>", paymentRequest[_gatewayLuncher.GtpayTranxNotiUrl], gatewayResponse[_gatewayLuncher.SiteRedirectUrl]));
            }

            if (messageBuilder.Length > 0)
            {
                messageBuilder.Append("</ol>");
                messageBuilder.Append("<div>" + T("Plugins.SmartStore.GTPay.ReportIssueToSystemAdminstratorMessage") + "</div>");
                messageBuilder.Insert(0, "<div>" + T("Plugins.SmartStore.GTPay.ManInTheMiddleAttackOccurredMessage") + " </div><ol>");

                //messageBuilder.Append("<div>Kindly report this issue to your system adminstrator immediately</div>");
                //messageBuilder.Insert(0, "<div>The following 'Man In The Middle Attack' occured during the transction processing! </div><ol>");
            }

            return messageBuilder.ToString();

        }

        private GTPayGatewayResponse GetTransactionDetailBy(string mertId, string amount, string tranxId, string hash)
        {
            try
            {
                GTPaySettings gtpaySettings = GetGTPaySettings();
                if (gtpaySettings == null)
                {
                    throw new Exception(T("Plugins.SmartStore.GTPay.GTPaySettingRetrievalFailedMessage"));
                    //throw new Exception("GTPay settings could not be retrieved!");
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
            //string errorTitle = "Error occured!";
            //string errorMessage = "Error occured during request processing! Plesae try again";

            string errorTitle = T("Plugins.SmartStore.GTPay.ErrorOccured");
            string errorMessage = T("Plugins.SmartStore.GTPay.ErrorOccuredMessage");
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
                string borderColor = "red";
                string alertType = "danger";
                string errorTitle = T("Plugins.SmartStore.GTPay.YourTransactionFailedMessage");
                //string errorTitle = "Your transaction failed!";

                //string otherError = _httpContext.Session[_gatewayLuncher.ErrorMessage] != null ? " other" : "";
                string otherError = _httpContext.Session[_gatewayLuncher.ErrorMessage] != null ? " " + T("Plugins.SmartStore.GTPay.Other") : "";
                string error = _httpContext.Session[_gatewayLuncher.ErrorMessage] != null ? _httpContext.Session[_gatewayLuncher.ErrorMessage].ToString() : "";
                //string transactionError = string.Format("{0} See below for{1} error details. Click the 'Continue shopping' button below to continue shopping", error, otherError);

                string homePageUrl = _gatewayLuncher.GetRedirectUrl(_httpContext.Request, "Index", "Home");
                string transactionError = string.Format(T("Plugins.SmartStore.GTPay.ErrorDetailMessage"), error, otherError, string.Format("<a href=\"{0}\"><i class=\"fa fa-home\"></i><span>", homePageUrl), "</span></a>");
                transactionError = transactionError.Trim();

                NameValueCollection gatewayMessage = GetGatewayResponse();
                if (gatewayMessage != null)
                {
                    //string transactionSummary = string.Format("Reason: {0} See below for error details. Click the 'Continue shopping' button below to try again", gatewayMessage[_gatewayLuncher.GtpayTranxStatusMsg]);
                    string[] gtpay_echo_data = gatewayMessage[_gatewayLuncher.GtpayEchoData].Split(';');

                    response.Append("<div class=\"heading mt-3\">");
                    response.Append("<h1 class=\"heading-title font-weight-light\">" + T("Plugins.SmartStore.GTPay.ErrorOccured") + "</h1>");
                    //response.Append("<h1 class=\"heading-title font-weight-light\">Error occurred!</h1>");
                    response.Append("</div>");

                    response.Append("<div class=\"card card-block order-review-data-box mb-3\" style=\"border-color:" + borderColor + "\">");
                    response.Append("<div class=\"terms-of-service alert alert-" + alertType + " mb-3\">");
                    response.Append("<label class=\"mb-0 form-check-label\">");
                    response.Append("<span style=\"font-size:25px\">" + errorTitle + "</span>");
                    response.Append("<br />");
                    response.Append("<span>" + transactionError + "</span>");
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
                    //response.Append("<td><b>Transaction Status Code</b></td>");
                    response.Append("<td><b>" + T("Plugins.SmartStore.GTPay.Field.TransactionStatusCode") + "</b></td>");
                    response.Append("<td>" + gatewayMessage[_gatewayLuncher.GtpayTranxStatusCode] + "</td>");
                    response.Append("</tr>");

                    response.Append("<tr>");
                    //response.Append("<td><b>Failure Reason</b></td>");
                    response.Append("<td><b>" + T("Plugins.SmartStore.GTPay.FailureReason") + "</b></td>");
                    response.Append("<td>" + gatewayMessage[_gatewayLuncher.GtpayTranxStatusMsg] + "</td>");
                    response.Append("</tr>");

                    response.Append("<tr>");
                    //response.Append("<td><b>Transaction Ref. No.</b></td>");
                    response.Append("<td><b>" + T("Plugins.SmartStore.GTPay.TransactionRefNo") + "</b></td>");
                    response.Append("<td>" + gatewayMessage[_gatewayLuncher.GtpayTranxId] + "</td>");
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
                    response = BuildErrorHtml(transactionError);
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

        private void SetOrderStatus(Order order, TransactionStatus transactionStatus)
        {
            try
            {

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
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
            }
        }

        private void SendEmail(Order order, bool paid)
        {
            try
            {
                //send email notifications
                int orderPlacedStoreOwnerNotificationQueuedEmailId = _workflowMessageService.SendOrderPlacedStoreOwnerNotification(order, _localizationSettings.DefaultAdminLanguageId);
                if (orderPlacedStoreOwnerNotificationQueuedEmailId > 0)
                {
                    _orderService.AddOrderNote(order, T("Admin.OrderNotice.MerchantEmailQueued", orderPlacedStoreOwnerNotificationQueuedEmailId));
                }

                if (paid)
                {
                    int orderPlacedCustomerNotificationQueuedEmailId = _workflowMessageService.SendOrderPlacedCustomerNotification(order, order.CustomerLanguageId);
                    if (orderPlacedCustomerNotificationQueuedEmailId > 0)
                    {
                        _orderService.AddOrderNote(order, T("Admin.OrderNotice.CustomerEmailQueued", orderPlacedCustomerNotificationQueuedEmailId));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);

                //model.TestEmailShortErrorMessage = ex.ToAllMessages();
                //model.TestEmailFullErrorMessage = ex.ToString();
            }
        }








    }
}