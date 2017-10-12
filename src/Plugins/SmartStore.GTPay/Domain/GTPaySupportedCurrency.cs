using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using SmartStore.Core;
using System.Runtime.Serialization;

namespace SmartStore.GTPay.Domain
{
    public class GTPaySupportedCurrency : BaseEntity
    {
        [DataMember]
        public int Code { get; set; }
        [DataMember]
        public string Alias { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Gateway { get; set; }
        [DataMember]
        public bool IsSupported { get; set; }








    }
}