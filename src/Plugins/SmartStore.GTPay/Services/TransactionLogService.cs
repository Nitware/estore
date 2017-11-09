using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using SmartStore.Core.Data;
using System.Linq.Expressions;
using SmartStore.GTPay.Interfaces;
using SmartStore.GTPay.Domain;
using System.Data.Entity;

namespace SmartStore.GTPay.Services
{
    public class TransactionLogService : ITransactionLogService
    {
        private readonly IRepository<GTPayTransactionLog> _gTPayTransactionLogRepository;

        public TransactionLogService(IRepository<GTPayTransactionLog> gTPayTransactionLogRepository)
        {
            _gTPayTransactionLogRepository = gTPayTransactionLogRepository;
        }

        public virtual List<GTPayTransactionLog> GetLatest500Transactions()
        {
            const int FIVE_HUNDRED = 500;
            Expression<Func<GTPayTransactionLog, bool>> selector = t => t.Id > 0;
            Func<IQueryable<GTPayTransactionLog>, IOrderedQueryable<GTPayTransactionLog>> orderBy = t => t.OrderByDescending(x => x.Id);

            return _gTPayTransactionLogRepository.Get(selector, orderBy).Take(FIVE_HUNDRED).ToList();
        }

        public virtual GTPayTransactionLog GetBy(string transactionReference)
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

            GTPayTransactionLog transactionLog = GetBy(transactionReference);
            if (transactionLog != null && transactionLog.TransactionRefNo.HasValue())
            {
                isExist = true;
            }
            
            return isExist;
        }

        public virtual void Save(GTPayTransactionLog gtpayTransactionLog)
        {
            if (gtpayTransactionLog == null)
                throw new ArgumentNullException("gtpayTransactionLog");

            _gTPayTransactionLogRepository.Insert(gtpayTransactionLog);
        }

        public virtual void Update(GTPayTransactionLog gTPayTransactionLog)
        {
            if (gTPayTransactionLog == null)
                throw new ArgumentNullException("gTPayTransactionLog");

            _gTPayTransactionLogRepository.Update(gTPayTransactionLog);
        }



        public virtual List<GTPayTransactionLog> GetBy(string transactionReference, DateTime transactionDate)
        {
            if (!transactionReference.HasValue())
                return null;

            Expression<Func<GTPayTransactionLog, bool>> selector = t => t.TransactionRefNo == transactionReference && DbFunctions.TruncateTime(t.TransactionDate) == transactionDate.Date;
            Func<IQueryable<GTPayTransactionLog>, IOrderedQueryable<GTPayTransactionLog>> orderBy = t => t.OrderByDescending(x => x.Id);

            return _gTPayTransactionLogRepository.Get(selector, orderBy).ToList();
        }

        public virtual List<GTPayTransactionLog> GetBy(DateTime transactionDate)
        {
            if (transactionDate == DateTime.MinValue)
                return null;

            Expression<Func<GTPayTransactionLog, bool>> selector = t => DbFunctions.TruncateTime(t.TransactionDate) == transactionDate.Date;
            Func<IQueryable<GTPayTransactionLog>, IOrderedQueryable<GTPayTransactionLog>> orderBy = t => t.OrderByDescending(x => x.Id);

            return _gTPayTransactionLogRepository.Get(selector, orderBy).ToList();
        }




    }
}