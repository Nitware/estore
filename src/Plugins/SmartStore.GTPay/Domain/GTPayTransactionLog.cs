using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using SmartStore.Core.Domain.Orders;
using System.Runtime.Serialization;

namespace SmartStore.GTPay.Domain
{
    public class GTPayTransactionLog
    {
        [DataMember]
        public string TransactionRefNo { get; set; }
        [DataMember]
        public decimal ApprovedAmount { get; set; }
        [DataMember]
        public long AmountInUnit { get; set; }
        [DataMember]
        public string ResponseCode { get; set; }
        [DataMember]
        public string ResponseDescription { get; set; }
        [DataMember]
        public string StatusReason { get; set; }
        [DataMember]
        public DateTime DatePaid { get; set; }
        [DataMember]
        public string CardNo { get; set; }
        [DataMember]
        public string WebPayRefNo { get; set; }
        [DataMember]
        public string PayRefNo { get; set; }
        [DataMember]
        public bool IsAmountMismatch { get; set; }
        [DataMember]
        public DateTime TransactionDate { get; set; }
        [DataMember]
        public int CurrencyCode { get; set; }
        [DataMember]
        public string Gateway { get; set; }
        [DataMember]
        public int OrderId { get; set; }
        [DataMember]
        public int GTPayTransactionStatusId { get; set; }

        
        public virtual GTPayTransactionStatus GTPayTransactionStatus { get; set; }
        
        public virtual Order Order { get; set; }
    }
}