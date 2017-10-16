using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using SmartStore.GTPay.Domain;

namespace SmartStore.GTPay.Interfaces
{
    public interface ITransactionStatusService
    {
        void Add(GTPayTransactionStatus gTPayTransactionStatus);
        void AddRange(List<GTPayTransactionStatus> gTPayTransactionStatusList);
        void UpdateGTPayTransactionStatus(GTPayTransactionStatus gTPayTransactionStatus);
        //void DeleteAllGTPayTransactionStatus();
    }



}