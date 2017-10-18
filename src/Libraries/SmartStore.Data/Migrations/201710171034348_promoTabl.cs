namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class promoTabl1 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.PromotionProducts", "Picture_Id", "dbo.Picture");
            DropIndex("dbo.PromotionProducts", new[] { "PromotionId" });
            DropIndex("dbo.PromotionProducts", new[] { "Picture_Id" });
            DropIndex("dbo.PromotionProducts", new[] { "Promotion_Id" });
            DropColumn("dbo.PromotionProducts", "PromotionId");
            RenameColumn(table: "dbo.PromotionProducts", name: "Promotion_Id", newName: "PromotionId");
            AlterColumn("dbo.PromotionProducts", "PromotionId", c => c.Int(nullable: false));
            CreateIndex("dbo.PromotionProducts", "PromotionId");
            DropColumn("dbo.PromotionProducts", "Picture_Id");
        }
        
        public override void Down()
        {
            //AddColumn("dbo.PromotionProducts", "Picture_Id", c => c.Int());
            DropIndex("dbo.PromotionProducts", new[] { "PromotionId" });
            AlterColumn("dbo.PromotionProducts", "PromotionId", c => c.Int());
            //RenameColumn(table: "dbo.PromotionProducts", name: "PromotionId", newName: "Promotion_Id");
            AddColumn("dbo.PromotionProducts", "PromotionId", c => c.Int(nullable: false));
            //CreateIndex("dbo.PromotionProducts", "Promotion_Id");
            //CreateIndex("dbo.PromotionProducts", "Picture_Id");
            CreateIndex("dbo.PromotionProducts", "PromotionId");
            AddForeignKey("dbo.PromotionProducts", "PromotionId", "dbo.Promotions", "Id");
        }
    }
}
