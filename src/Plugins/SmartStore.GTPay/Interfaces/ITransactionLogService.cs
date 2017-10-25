using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using SmartStore.GTPay.Domain;

namespace SmartStore.GTPay.Interfaces
{
    public interface ITransactionLogService
    {
        List<GTPayTransactionLog> GetLatest500Transactions();
        bool TransactionReferenceExist(string transactionReference);
        void Save(GTPayTransactionLog gTPayTransactionLog);
        void Update(GTPayTransactionLog gTPayTransactionLog);

        GTPayTransactionLog GetBy(string transactionReference);
        List<GTPayTransactionLog> GetBy(string transactionReference, DateTime transactionDate);
        List<GTPayTransactionLog> GetBy(DateTime transactionDate);
    }


}