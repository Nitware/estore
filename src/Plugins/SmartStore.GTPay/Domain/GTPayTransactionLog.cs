using System;
using System.Runtime.Serialization;

namespace SmartStore.GTPay.Domain
{
    public class GTPayTransactionLog
    {
        public string TransactionRefNo { get; set; }
        public int GTPayTransactionStatusId { get; set; }
        public long AmountInUnit { get; set; }
        public string ResponseCode { get; set; }
        public string ResponseDescription { get; set; }
        public string StatusReason { get; set; }
        public DateTime DatePaid { get; set; }
        public string CardNo { get; set; }
        public string WebPayRefNo { get; set; }
        public string PayRefNo { get; set; }
        public bool IsAmountMismatch { get; set; }

        //[DataMember]
        public virtual GTPayTransactionStatus GTPayTransactionStatus { get; set; }


    }
}