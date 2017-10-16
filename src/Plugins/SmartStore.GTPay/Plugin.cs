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
            GTPaySupportedCurrency gTPaySupportedCurrency = new GTPaySupportedCurrency()
            {
                Alias = "NGN",
                Name = "Naira",
                Code = 566,
                Gateway = "webpay",
                IsSupported = true,
            };

            List<GTPayTransactionStatus> tPayTransactionStatusList = new List<Domain.GTPayTransactionStatus>()
            {
                new GTPayTransactionStatus() { StatusName = "Pending" },
                new GTPayTransactionStatus() { StatusName = "Successfull" },
                new GTPayTransactionStatus() { StatusName = "Failed" },
            };
            
            _transactionStatusService.AddRange(tPayTransactionStatusList);
            _supportedCurrencyService.Add(gTPaySupportedCurrency);

            //save settings
            _settingService.SaveSetting(new GTPaySettings()
            {
                DescriptionText = "@Plugins.Payments.Manual.PaymentInfoDescription"
            });

            // add resources
            _localizationService.ImportPluginResourcesFromXml(this.PluginDescriptor);

            base.Install();

            _logger.Info(string.Format("Plugin installed: SystemName: {0}, Version: {1}, Description: '{2}'", PluginDescriptor.SystemName, PluginDescriptor.Version, PluginDescriptor.FriendlyName));

        }

        public override void Uninstall()
        {
            //_transactionStatusService.DeleteAllGTPayTransactionStatus();
            //_supportedCurrencyService.DeleteAllGTPaySupportedCurrency();
            
            //var settings = _services.Settings;
            //var loc = _services.Localization;

            //// delete settings
            //settings.DeleteSetting<CashOnDeliveryPaymentSettings>();
            //settings.DeleteSetting<InvoicePaymentSettings>();
            //settings.DeleteSetting<PayInStorePaymentSettings>();
            //settings.DeleteSetting<PrepaymentPaymentSettings>();
            //settings.DeleteSetting<ManualPaymentSettings>();
            //settings.DeleteSetting<DirectDebitPaymentSettings>();

            //// delete resources
            //loc.DeleteLocaleStringResources(this.PluginDescriptor.ResourceRootKey);
            //loc.DeleteLocaleStringResources("Plugins.Payment.CashOnDelivery");
            //loc.DeleteLocaleStringResources("Plugins.Payment.Invoice");
            //loc.DeleteLocaleStringResources("Plugins.Payment.PayInStore");
            //loc.DeleteLocaleStringResources("Plugins.Payment.Prepayment");
            //loc.DeleteLocaleStringResources("Plugins.Payments.Manual");
            //loc.DeleteLocaleStringResources("Plugins.Payments.DirectDebit");


            _settingService.DeleteSetting<GTPaySettings>();
           
            var migrator = new DbMigrator(new Configuration());
            migrator.Update(DbMigrator.InitialDatabase);

            _localizationService.DeleteLocaleStringResources(PluginDescriptor.ResourceRootKey);

            base.Uninstall();

            _logger.Info(string.Format("Plugin uninstalled: SystemName: {0}, Version: {1}, Description: '{2}'", PluginDescriptor.SystemName, PluginDescriptor.Version, PluginDescriptor.FriendlyName));

        }





    }
}