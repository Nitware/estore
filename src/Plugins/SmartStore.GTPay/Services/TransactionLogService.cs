using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using SmartStore.GTPay.Interfaces;
using SmartStore.GTPay.Domain;
using SmartStore.Core.Data;

namespace SmartStore.GTPay.Services
{
    public class TransactionLogService : ITransactionLogService
    {
        private readonly IRepository<GTPayTransactionLog> _gTPayTransactionLogRepository;

        public TransactionLogService(IRepository<GTPayTransactionLog> gTPayTransactionLogRepository)
        {
            _gTPayTransactionLogRepository = gTPayTransactionLogRepository;
        }
        
        public virtual GTPayTransactionLog GetByTransactionReference(string transactionReference)
        {
            if (!transactionReference.HasValue())
                return null;

            return _gTPayTransactionLogRepository.GetById(transactionReference);
        }

        public virtual bool TransactionReferenceExist(string transactionReference)
        {
            bool isExist = false;

            if (!transactionReference.HasValue())
                throw new ArgumentNullException("transactionReference");

            GTPayTransactionLog transactionLog = GetByTransactionReference(transactionReference);
            if (transactionLog != null && transactionLog.TransactionRefNo.HasValue())
            {
                isExist = true;
            }
            
            return isExist;
        }

        public virtual void Save(GTPayTransactionLog gTPayTransactionLog)
        {
            if (gTPayTransactionLog == null)
                throw new ArgumentNullException("gTPayTransactionLog");

            _gTPayTransactionLogRepository.Insert(gTPayTransactionLog);
        }

        public virtual void Update(GTPayTransactionLog gTPayTransactionLog)
        {
            if (gTPayTransactionLog == null)
                throw new ArgumentNullException("gTPayTransactionLog");

            _gTPayTransactionLogRepository.Update(gTPayTransactionLog);
        }




    }
}