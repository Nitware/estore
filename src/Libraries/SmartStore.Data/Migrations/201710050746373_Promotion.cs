namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Promotion : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Promotion",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ProductId = c.Int(nullable: false),
                        CategoryId = c.Int(),
                        Title = c.String(),
                        Image = c.String(),
                        DiscountPercentage = c.Double(nullable: false),
                        DiscountAmount = c.Double(nullable: false),
                        DiscountConditions = c.String(),
                        Deleted = c.Boolean(nullable: false),
                        Active = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Category", t => t.CategoryId)
                .ForeignKey("dbo.Product", t => t.ProductId)
                .Index(t => t.ProductId)
                .Index(t => t.CategoryId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Promotion", "ProductId", "dbo.Product");
            DropForeignKey("dbo.Promotion", "CategoryId", "dbo.Category");
            DropIndex("dbo.Promotion", new[] { "CategoryId" });
            DropIndex("dbo.Promotion", new[] { "ProductId" });
            DropTable("dbo.Promotion");
        }
    }
}
