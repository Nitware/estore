using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using SmartStore.Core;

namespace SmartStore.GTPay.Domain
{
    public class GTPayCardType : BaseEntity
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}