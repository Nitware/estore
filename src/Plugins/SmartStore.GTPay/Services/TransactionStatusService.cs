using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using SmartStore.GTPay.Interfaces;
using SmartStore.Core.Data;
using SmartStore.GTPay.Domain;

namespace SmartStore.GTPay.Services
{
    public class TransactionStatusService : ITransactionStatusService
    {
        private readonly IRepository<GTPayTransactionStatus> _transactionStatusRepository;

        public TransactionStatusService(IRepository<GTPayTransactionStatus> transactionStatusRepository)
        {
            _transactionStatusRepository = transactionStatusRepository;
        }

        public void Add(GTPayTransactionStatus gTPayTransactionStatus)
        {
            if (gTPayTransactionStatus == null)
                throw new ArgumentNullException("gTPayTransactionStatus");

            _transactionStatusRepository.Insert(gTPayTransactionStatus);
           
        }
        public void AddRange(List<GTPayTransactionStatus> gTPayTransactionStatusList)
        {
            if (gTPayTransactionStatusList == null || gTPayTransactionStatusList.Count <= 0)
                throw new ArgumentNullException("gTPayTransactionStatusList");

            _transactionStatusRepository.InsertRange(gTPayTransactionStatusList);

        }
        public virtual void UpdateGTPayTransactionStatus(GTPayTransactionStatus gTPayTransactionStatus)
        {
            if (gTPayTransactionStatus == null)
                throw new ArgumentNullException("gTPayTransactionStatus");

            _transactionStatusRepository.Update(gTPayTransactionStatus);
        }
      




    }


}