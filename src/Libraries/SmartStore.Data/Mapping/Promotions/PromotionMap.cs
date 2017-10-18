using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Promotions;

namespace SmartStore.Data.Mapping.Promotions
{
    public partial class PromotionMap : EntityTypeConfiguration<Promotion>
    {
        public PromotionMap()
        {
            this.ToTable("Promotion");
            this.HasKey(a => a.Id);

            //this.HasRequired(a => a.Product).WithMany().HasForeignKey(x => x.ProductId).WillCascadeOnDelete(false);
			this.HasRequired(a => a.Picture).WithMany().HasForeignKey(x => x.PictureId).WillCascadeOnDelete(false);

		}
    }
}