namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class promoAdvert : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Promotion",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(),
                        TitleFontType = c.String(),
                        TitleFontSize = c.Int(nullable: false),
                        TitleFontColor = c.String(),
                        SubTitle = c.String(),
                        SubTitleFontType = c.String(),
                        SubTitleFontSize = c.Int(nullable: false),
                        SubTitleFontColor = c.String(),
                        DiscountText = c.String(),
                        DiscountTextFontType = c.String(),
                        DiscountTextFontSize = c.Int(nullable: false),
                        DiscountTextFontColor = c.String(),
                        Description = c.String(),
                        TextFrameType = c.String(),
                        TextFrameBackground = c.String(),
                        TextFrameHeight = c.Int(nullable: false),
                        TextFrameWidth = c.Int(nullable: false),
                        CreationDate = c.DateTime(nullable: false),
                        CreatedBy = c.String(),
                        ExpiryDate = c.DateTime(),
                        NoOfColumn = c.Int(nullable: false),
                        DiscountPercentage = c.Int(nullable: false),
                        DiscountAmount = c.Double(nullable: false),
                        PictureId = c.Int(nullable: false),
                        PictureUrl = c.String(),
                        RedirectUrl = c.String(),
                        DisplayOrder = c.Int(nullable: false),
                        Published = c.Boolean(nullable: false),
                        Deleted = c.Boolean(nullable: false),
                        Active = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Picture", t => t.PictureId)
                .Index(t => t.PictureId);
            
            CreateTable(
                "dbo.PromotionProducts",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ProductId = c.Int(nullable: false),
                        CategoryId = c.Int(),
                        PromotionId = c.Int(nullable: false),
                        Deleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Category", t => t.CategoryId)
                .ForeignKey("dbo.Product", t => t.ProductId)
                .ForeignKey("dbo.Promotion", t => t.PromotionId)
                .Index(t => t.ProductId)
                .Index(t => t.CategoryId)
                .Index(t => t.PromotionId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PromotionProducts", "PromotionId", "dbo.Promotion");
            DropForeignKey("dbo.PromotionProducts", "ProductId", "dbo.Product");
            DropForeignKey("dbo.PromotionProducts", "CategoryId", "dbo.Category");
            DropForeignKey("dbo.Promotion", "PictureId", "dbo.Picture");
            DropIndex("dbo.PromotionProducts", new[] { "PromotionId" });
            DropIndex("dbo.PromotionProducts", new[] { "CategoryId" });
            DropIndex("dbo.PromotionProducts", new[] { "ProductId" });
            DropIndex("dbo.Promotion", new[] { "PictureId" });
            DropTable("dbo.PromotionProducts");
            DropTable("dbo.Promotion");
        }
    }
}
