
using SmartStore.GTPay.Domain;
using System.Data.Entity.ModelConfiguration;

namespace SmartStore.GTPay.Data
{
    public class GTPayTransactionStatusRecordMap : EntityTypeConfiguration<GTPayTransactionStatus>
    {
        public GTPayTransactionStatusRecordMap()
        {
            this.ToTable("GTPayTransactionStatus");
            this.HasKey(x => x.Id);
            this.Property(x => x.StatusName).IsRequired().HasMaxLength(100);

        }


    }


}