using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using SmartStore.Core;

namespace SmartStore.GTPay.Domain
{
    public class GTPayTransactionStatus : BaseEntity
    {
        //private ICollection<GTPayTransactionLog> _gTPayTransactionLogs;

        public string StatusName { get; set; }

        //public virtual ICollection<GTPayTransactionLog> GTPayTransactionLogs
        //{
        //    get { return _gTPayTransactionLogs ?? (_gTPayTransactionLogs = new HashSet<GTPayTransactionLog>()); }
        //    protected set { _gTPayTransactionLogs = value; }
        //}
    }



}