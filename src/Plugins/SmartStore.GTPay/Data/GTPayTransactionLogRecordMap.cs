using SmartStore.GTPay.Domain;
using System.Data.Entity.ModelConfiguration;

namespace SmartStore.GTPay.Data
{
    public class GTPayTransactionLogRecordMap : EntityTypeConfiguration<GTPayTransactionLog>
    {
        public GTPayTransactionLogRecordMap()
        {
            this.ToTable("GTPayTransactionLog");
            this.HasKey(x => x.TransactionRefNo);
            this.Property(x => x.AmountInUnit).IsRequired();
            this.Property(x => x.GTPayTransactionStatusId).IsRequired();
            this.Property(x => x.ResponseCode).IsOptional().HasMaxLength(5);
            this.Property(x => x.ResponseDescription).IsOptional().HasMaxLength(4000);
            this.Property(x => x.StatusReason).IsOptional().HasMaxLength(300);
            this.Property(x => x.DatePaid).IsOptional();
            this.Property(x => x.CardNo).IsOptional().HasMaxLength(30);
            this.Property(x => x.WebPayRefNo).IsOptional().HasMaxLength(30);
            this.Property(x => x.PayRefNo).IsOptional().HasMaxLength(30);
            this.Property(x => x.IsAmountMismatch).IsOptional();

            //this.HasRequired(nc => nc.GTPayTransactionStatus)
            //    .WithMany(n => n.GTPayTransactionLogs)
            //    .HasForeignKey(nc => nc.GTPayTransactionStatusId);



        }

    }
}