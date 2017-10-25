using SmartStore.Core.Domain.Customers;
using SmartStore.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartStore.GTPay.Models
{
    public class TransactionLog
    {
        [SmartResourceDisplayName("Plugins.Payments.GTPay.Fields.TransactionRefNo")]
        public string TransactionRefNo { get; set; }

        [SmartResourceDisplayName("Plugins.Payments.GTPay.Fields.TransactionStatusId")]
        public int GTPayTransactionStatusId { get; set; }

        [SmartResourceDisplayName("Plugins.Payments.GTPay.Fields.TransactionStatus")]
        public string TransactionStatus { get; set; }

        [SmartResourceDisplayName("Plugins.Payments.GTPay.Fields.ApprovedAmount")]
        public decimal ApprovedAmount { get; set; }

        [SmartResourceDisplayName("Plugins.Payments.GTPay.Fields.AmountInUnit")]
        public long AmountInUnit { get; set; }

        [SmartResourceDisplayName("Plugins.Payments.GTPay.Fields.OrderId")]
        public int OrderId { get; set; }

        [SmartResourceDisplayName("Plugins.Payments.GTPay.Fields.CustomerId")]
        public int? CustomerId { get; set; }
        public Customer Custmomer { get; set; }

        [SmartResourceDisplayName("Plugins.Payments.GTPay.Fields.ResponseCode")]
        public string ResponseCode { get; set; }

        [SmartResourceDisplayName("Plugins.Payments.GTPay.Fields.ResponseDescription")]
        public string ResponseDescription { get; set; }

        [SmartResourceDisplayName("Plugins.Payments.GTPay.Fields.DatePaid")]
        public DateTime? DatePaid { get; set; }

        [SmartResourceDisplayName("Plugins.Payments.GTPay.Fields.MerchantReference")]
        public string MerchantReference { get; set; }

        [SmartResourceDisplayName("Plugins.Payments.GTPay.Fields.IsAmountMismatch")]
        public bool IsAmountMismatch { get; set; }

        [SmartResourceDisplayName("Plugins.Payments.GTPay.Fields.TransactionDate")]
        public DateTime TransactionDate { get; set; }

        [SmartResourceDisplayName("Plugins.Payments.GTPay.Fields.CurrencyAlias")]
        public string CurrencyAlias { get; set; }

        [SmartResourceDisplayName("Plugins.Payments.GTPay.Fields.Gateway")]
        public string Gateway { get; set; }

        [SmartResourceDisplayName("Plugins.Payments.GTPay.Fields.VerificationHash")]
        public string VerificationHash { get; set; }

        [SmartResourceDisplayName("Plugins.Payments.GTPay.Fields.FullVerificationHash")]
        public string FullVerificationHash { get; set; }
    }





}