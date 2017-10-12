using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using SmartStore.GTPay.Domain;
using System.Data.Entity.ModelConfiguration;

namespace SmartStore.GTPay.Data
{
    public class GTPayTransactionLogRecordMap : EntityTypeConfiguration<GTPayTransactionLog>
    {
        public GTPayTransactionLogRecordMap()
        {
            this.ToTable("GTPayTransactionLog");
            this.HasKey(x => x.TransactionRefNo).Property(x => x.TransactionRefNo).HasMaxLength(40);
            this.Property(x => x.AmountInUnit).IsRequired();
            this.Property(x => x.GTPayTransactionStatusId).IsRequired();
            this.Property(x => x.ResponseCode).IsOptional().HasMaxLength(5);
            this.Property(x => x.ResponseDescription).IsOptional().HasMaxLength(4000);
            this.Property(x => x.StatusReason).IsOptional().HasMaxLength(400);
            this.Property(x => x.DatePaid).IsOptional();
            this.Property(x => x.CardNo).IsOptional().HasMaxLength(30);
            this.Property(x => x.WebPayRefNo).IsOptional().HasMaxLength(30);
            this.Property(x => x.PayRefNo).IsOptional().HasMaxLength(30);
            this.Property(x => x.IsAmountMismatch).IsOptional();
            this.Property(x => x.OrderId).IsOptional();
            this.Property(x => x.TransactionDate).IsOptional();
            this.Property(x => x.CurrencyCode).IsOptional();
            this.Property(x => x.Gateway).IsOptional().HasMaxLength(10);

            this.HasRequired(nc => nc.GTPayTransactionStatus)
                .WithMany()
                .HasForeignKey(nc => nc.GTPayTransactionStatusId);

            this.HasRequired(nc => nc.Order)
               .WithMany()
               .HasForeignKey(nc => nc.OrderId);


            //this.HasRequired(nc => nc.GTPayTransactionStatus)
            //    .WithMany(n => n.GTPayTransactionLogs)
            //    .HasForeignKey(nc => nc.GTPayTransactionStatusId);

            //this.HasRequired(nc => nc.Order)
            //   .WithMany(n => n.orders)
            //   .HasForeignKey(nc => nc.GTPayTransactionStatusId);


        }

    }


}