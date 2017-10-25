using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using SmartStore.Core.Configuration;
using SmartStore.Web.Framework;
using System.Web.Mvc;
using Telerik.Web.Mvc;
using SmartStore.GTPay.Domain;

namespace SmartStore.GTPay.Models
{
    public class ConfigurationModel
    {
        public ConfigurationModel()
        {
            GTPayCurrencyGrid = new GridModel<GTPayCurrencyModel>();
            TransactionLogsForGrid = new GridModel<TransactionLog>();
            TransactionRequest = new TransactionRequest();
            //TransactionRequest.Date = DateTime.UtcNow;
        }

        [AllowHtml]
        [SmartResourceDisplayName("Plugins.SmartStore.GTPay.DescriptionText")]
        public string DescriptionText { get; set; }

        [SmartResourceDisplayName("Plugins.SmartStore.GTPay.AdditionalFee")]
        public decimal AdditionalFee { get; set; }

        [SmartResourceDisplayName("Plugins.SmartStore.GTPay.AdditionalFeePercentage")]
        public bool AdditionalFeePercentage { get; set; }
        
        [SmartResourceDisplayName("Plugins.Payments.GTPay.Fields.HashKey")]
        public string HashKey { get; set; }

        [SmartResourceDisplayName("Plugins.Payments.GTPay.Fields.MerchantID")]
        public string MerchantId { get; set; }

        [SmartResourceDisplayName("Plugins.Payments.GTPay.Fields.GatewayPostUrl")]
        public string GatewayPostUrl { get; set; }

        [SmartResourceDisplayName("Plugins.Payments.GTPay.Fields.GatewayRequeryUrl")]
        public string GatewayRequeryUrl { get; set; }

        [SmartResourceDisplayName("Plugins.Payments.GTPay.Fields.ShowGatewayInterface")]
        public bool ShowGatewayInterface { get; set; }

        [SmartResourceDisplayName("Plugins.Payments.GTPay.Fields.ShowGatewayNameFirst")]
        public bool ShowGatewayNameFirst { get; set; }

        public List<GTPayCurrencyModel> SupportedCurrencies { get; set; }
        public GTPayCurrencyModel SupportedCurrency { get; set; }

        public int GridPageSize { get; set; }
        public GridModel<GTPayCurrencyModel> GTPayCurrencyGrid { get; set; }

        public GridModel<TransactionLog> TransactionLogsForGrid { get; set; }
        public TransactionRequest TransactionRequest { get; set; }
    }



}