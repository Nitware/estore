using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using SmartStore.Core.Plugins;
using SmartStore.Services;
using SmartStore.GTPay.Interfaces;
using SmartStore.GTPay.Domain;
using System.Data.Entity.Migrations;
using SmartStore.GTPay.Data.Migrations;
using SmartStore.Services.Localization;
using SmartStore.Services.Configuration;
using SmartStore.Core.Logging;
using SmartStore.GTPay.Settings;

namespace SmartStore.GTPay
{
    public class Plugin : BasePlugin
    {
        private readonly ICommonServices _services;
        private readonly IGTPayCurrencyService _supportedCurrencyService;
        private readonly ITransactionStatusService _transactionStatusService;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly ILogger _logger;

        public Plugin(ICommonServices services, 
            IGTPayCurrencyService supportedCurrencyService, 
            ITransactionStatusService transactionStatusService,
            ILocalizationService localizationService,
            ISettingService settingService,
            ILogger logger)
        {
            _services = services;
            _transactionStatusService = transactionStatusService;
            _supportedCurrencyService = supportedCurrencyService;
            _localizationService = localizationService;
            _settingService = settingService;
            _logger = logger;
        }

        public override void Install()
        {
            List<GTPaySupportedCurrency> currencies = new List<GTPaySupportedCurrency>()
            {
                new GTPaySupportedCurrency()
                {
                    Alias = "NGN",
                    Name = "Naira",
                    Code = 566,
                    Gateway = "webpay",
                    LeastValueUnitMultiplier = 100,
                    IsSupported = true,
                },
                 new GTPaySupportedCurrency()
                {
                    Alias = "USD",
                    Name = "Dollar",
                    Code = 840,
                    Gateway = "Migs",
                    LeastValueUnitMultiplier = 100,
                    IsSupported = false,
                }
            };

            List<GTPayTransactionStatus> tPayTransactionStatusList = new List<Domain.GTPayTransactionStatus>()
            {
                new GTPayTransactionStatus() { StatusName = "Pending" },
                new GTPayTransactionStatus() { StatusName = "Successfull" },
                new GTPayTransactionStatus() { StatusName = "Failed" },
            };

            _transactionStatusService.AddRange(tPayTransactionStatusList);
            _supportedCurrencyService.AddRange(currencies);

            //save settings
            _settingService.SaveSetting(new GTPaySettings()
            {
                MerchantId = "8692",
                ShowGatewayInterface = true,
                ShowGatewayNameFirst = true,
                SendMailOnFailedTransaction = false,
                GatewayPostUrl = "http://gtweb2.gtbank.com/orangelocker/gtpaym/tranx.aspx",
                GatewayRequeryUrl = "https://gtweb2.gtbank.com/GTPayService/gettransactionstatus.json",
                HashKey = "D3D1D05AFE42AD50818167EAC73C109168A0F108F32645C8B59E897FA930DA44F9230910DAC9E20641823799A107A02068F7BC0F4CC41D2952E249552255710F",
                TransactionSuccessCode = "00",
                AdditionalFeePercentage = false,
                AdditionalFee = 0,

                //DescriptionText = "@Plugins.Payments.Manual.PaymentInfoDescription"
            });

            // add resources
            _localizationService.ImportPluginResourcesFromXml(this.PluginDescriptor);

            base.Install();

            _logger.Info(string.Format(_localizationService.GetResource("Plugins.SmartStore.GTPay.PluginInstalled"), PluginDescriptor.SystemName, PluginDescriptor.Version, PluginDescriptor.FriendlyName));
        }

        public override void Uninstall()
        {
            _settingService.DeleteSetting<GTPaySettings>();
           
            var migrator = new DbMigrator(new Configuration());
            migrator.Update(DbMigrator.InitialDatabase);

            _localizationService.DeleteLocaleStringResources(PluginDescriptor.ResourceRootKey);

            base.Uninstall();

            _logger.Info(string.Format(_localizationService.GetResource("Plugins.SmartStore.GTPay.PluginUninstalled"), PluginDescriptor.SystemName, PluginDescriptor.Version, PluginDescriptor.FriendlyName));
        }





    }
}