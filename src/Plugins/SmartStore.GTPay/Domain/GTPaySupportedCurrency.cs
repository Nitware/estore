using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using SmartStore.Core;

namespace SmartStore.GTPay.Domain
{
    public class GTPaySupportedCurrency : BaseEntity
    {
        public int Code { get; set; }
        public string Alias { get; set; }
        public string Name { get; set; }
        public string Gateway { get; set; }
        public bool IsSupported { get; set; }








    }
}