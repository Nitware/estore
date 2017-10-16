using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using SmartStore.GTPay.Domain;

namespace SmartStore.GTPay.Interfaces
{
    public interface IGTPayCurrencyService
    {
        List<GTPaySupportedCurrency> GetAllCurrencies();
        void Add(GTPaySupportedCurrency gTPaySupportedCurrency);
        List<GTPaySupportedCurrency> GetSupportedCurrencies();
        GTPaySupportedCurrency GetSupportedCurrencyById(int id);
        GTPaySupportedCurrency GetSupportedCurrencyByCode(int code);
        void UpdateSupportedCurrency(GTPaySupportedCurrency gTPaySupportedCurrency);
    }





}