using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Web;

using SmartStore.GTPay.Domain;
using System.Data.Entity.Infrastructure.Annotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartStore.GTPay.Data
{
    public class GTPayCardTypeRecordMap : EntityTypeConfiguration<GTPayCardType>
    {
        public GTPayCardTypeRecordMap()
        {
            this.ToTable("GTPayCardType");
            this.HasKey(x => x.Id);
            this.Property(x => x.Code).IsRequired().HasMaxLength(4).HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_Code") { IsUnique = true }));
            this.Property(x => x.Name).IsRequired().HasMaxLength(10);
            this.Property(x => x.Description).IsOptional().HasMaxLength(400);
                     
            

        }

    }
}