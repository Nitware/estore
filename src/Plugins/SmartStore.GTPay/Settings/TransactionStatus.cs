using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartStore.GTPay.Settings
{
    /// <summary>
    /// Represents card payment processor transaction status
    /// </summary>
    public enum TransactionStatus
    {
        /// <summary>
        /// Pending
        /// </summary>
        Pending = 1,

        /// <summary>
        /// Successful
        /// </summary>
        Successful = 2,

        /// <summary>
        /// Failed
        /// </summary>
        Failed = 3
    }



}