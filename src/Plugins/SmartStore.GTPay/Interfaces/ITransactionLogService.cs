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
        GTPayTransactionLog GetByTransactionReference(string transactionReference);
        bool TransactionReferenceExist(string transactionReference);
        void Save(GTPayTransactionLog gTPayTransactionLog);
        void Update(GTPayTransactionLog gTPayTransactionLog);
    }


}