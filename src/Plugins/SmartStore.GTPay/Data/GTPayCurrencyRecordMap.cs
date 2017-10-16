using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Web;

using SmartStore.GTPay.Domain;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartStore.GTPay.Data
{
    public class GTPayCurrencyRecordMap : EntityTypeConfiguration<GTPaySupportedCurrency>
    {
        public GTPayCurrencyRecordMap()
        {
            this.ToTable("GTPayCurrency");
            this.HasKey(x => x.Id);
            this.Property(x => x.Code).IsRequired().HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_Code") { IsUnique = true }));
            this.Property(x => x.Name).IsRequired().HasMaxLength(20);
            this.Property(x => x.Gateway).IsRequired().HasMaxLength(10);
            this.Property(x => x.IsSupported).IsRequired();
            this.Property(x => x.LeastValueUnitMultiplier).IsRequired();

        }

    }


}