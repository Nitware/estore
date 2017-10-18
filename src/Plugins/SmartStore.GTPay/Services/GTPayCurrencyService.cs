using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using SmartStore.GTPay.Interfaces;
using SmartStore.Core.Data;
using SmartStore.GTPay.Domain;

namespace SmartStore.GTPay.Services
{
    public class GTPayCurrencyService : IGTPayCurrencyService
    {
        private readonly IRepository<GTPaySupportedCurrency> _supportedCurrencyRepository;

        public GTPayCurrencyService(IRepository<GTPaySupportedCurrency> supportedCurrencyRepository)
        {
            _supportedCurrencyRepository = supportedCurrencyRepository;
        }

        public virtual List<GTPaySupportedCurrency> GetAllCurrencies()
        {
            return _supportedCurrencyRepository.Get(c => c.Id > 0).ToList();
        }
        public virtual List<GTPaySupportedCurrency> GetSupportedCurrencies()
        {
           return _supportedCurrencyRepository.Get(c => c.IsSupported == true).ToList();
        }
        public virtual GTPaySupportedCurrency GetSupportedCurrencyByCode(int code)
        {
            return _supportedCurrencyRepository.Get(c => c.Code == code).FirstOrDefault();
        }
        public virtual GTPaySupportedCurrency GetSupportedCurrencyById(int id)
        {
            return _supportedCurrencyRepository.Get(c => c.Id == id).FirstOrDefault();
        }
        public virtual void Add(GTPaySupportedCurrency gTPaySupportedCurrency)
        {
            if (gTPaySupportedCurrency == null)
                throw new ArgumentNullException("gTPaySupportedCurrency");

            _supportedCurrencyRepository.Insert(gTPaySupportedCurrency);
        }

        public void AddRange(List<GTPaySupportedCurrency> gtpaySupportedCurrencies)
        {
            if (gtpaySupportedCurrencies == null || gtpaySupportedCurrencies.Count <= 0)
                throw new ArgumentNullException("gtpaySupportedCurrencies");

            _supportedCurrencyRepository.InsertRange(gtpaySupportedCurrencies);
        }

        public virtual void UpdateSupportedCurrency(GTPaySupportedCurrency gTPaySupportedCurrency)
        {
            if (gTPaySupportedCurrency == null)
                throw new ArgumentNullException("gTPaySupportedCurrency");

            _supportedCurrencyRepository.Update(gTPaySupportedCurrency);
        }





    }
}