using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using SmartStore.Core.Domain.Orders;

namespace SmartStore.GTPay.Domain
{
    public class GTPayTransactionLog
    {
        public string TransactionRefNo { get; set; }
        public decimal ApprovedAmount { get; set; }
        public long AmountInUnit { get; set; }
        public string ResponseCode { get; set; }
        public string ResponseDescription { get; set; }
        public string StatusReason { get; set; }
        public DateTime DatePaid { get; set; }
        public string CardNo { get; set; }
        public string WebPayRefNo { get; set; }
        public string PayRefNo { get; set; }
        public bool IsAmountMismatch { get; set; }
        public DateTime TransactionDate { get; set; }
        public int CurrencyCode { get; set; }
        public string Gateway { get; set; }

        public int OrderId { get; set; }
        public int GTPayTransactionStatusId { get; set; }

        public virtual GTPayTransactionStatus GTPayTransactionStatus { get; set; }
        public virtual Order Order { get; set; }
    }
}