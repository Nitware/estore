using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Promotions;

namespace SmartStore.Data.Mapping.Promotions
{
    public partial class PromotionProductsMap : EntityTypeConfiguration<PromotionProducts>
    {
        public PromotionProductsMap()
        {
            this.ToTable("PromotionProducts");
            this.HasKey(a => a.Id);

            this.HasRequired(a => a.Product).WithMany().HasForeignKey(x => x.ProductId).WillCascadeOnDelete(false);
			this.HasRequired(a => a.Promotion).WithMany(d=>d.PromotionProducts).HasForeignKey(x => x.PromotionId).WillCascadeOnDelete(false);

		}
    }
}