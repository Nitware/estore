namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class promotionchangesproductid1 : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.Promotion", new[] { "PictureId" });
            AlterColumn("dbo.Promotion", "PictureId", c => c.Int(nullable: false));
            CreateIndex("dbo.Promotion", "PictureId");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Promotion", new[] { "PictureId" });
            AlterColumn("dbo.Promotion", "PictureId", c => c.Int());
            CreateIndex("dbo.Promotion", "PictureId");
        }
    }
}
