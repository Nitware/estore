using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using SmartStore.GTPay.Domain;
using SmartStore.Web.Framework;

namespace SmartStore.GTPay.Models
{
    public class TransactionRequest
    {
        [SmartResourceDisplayName("Plugins.Payments.GTPay.Fields.ReferenceNo")]
        public string ReferenceNo { get; set; }

        [SmartResourceDisplayName("Plugins.Payments.GTPay.Fields.Date")]
        public DateTime Date { get; set; }
       
    }



}