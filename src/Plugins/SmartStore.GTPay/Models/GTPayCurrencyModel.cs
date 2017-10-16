using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Framework;

namespace SmartStore.GTPay.Models
{
    public class GTPayCurrencyModel : ModelBase
    {
        public int Id { get; set; }

        [SmartResourceDisplayName("Plugins.Payments.GTPay.Fields.CurrencyCode")]
        public int Code { get; set; }

        [SmartResourceDisplayName("Plugins.Payments.GTPay.Fields.CurrencyAlias")]
        public string Alias { get; set; }

        [SmartResourceDisplayName("Plugins.Payments.GTPay.Fields.CurrencyName")]
        public string Name { get; set; }

        [SmartResourceDisplayName("Plugins.Payments.GTPay.Fields.CurrencyGateway")]
        public string Gateway { get; set; }

        [SmartResourceDisplayName("Plugins.Payments.GTPay.Fields.IsSupported")]
        public bool IsSupported { get; set; }

        [SmartResourceDisplayName("Plugins.Payments.GTPay.Fields.Multiplicity")]
        public int Multiplicity { get; set; }


    }
}