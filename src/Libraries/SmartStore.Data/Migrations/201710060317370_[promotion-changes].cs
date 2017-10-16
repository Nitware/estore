namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class promotionchanges : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Promotion", "PictureId", c => c.Int(nullable: false));
            AddColumn("dbo.Promotion", "PictureUrl", c => c.String());
            AddColumn("dbo.Promotion", "RedirectUrl", c => c.String());
            AddColumn("dbo.Promotion", "Published", c => c.Boolean(nullable: false));
            AddColumn("dbo.Promotion", "DisplayOrder", c => c.Int(nullable: false));
            CreateIndex("dbo.Promotion", "PictureId");
            AddForeignKey("dbo.Promotion", "PictureId", "dbo.Picture", "Id");
            DropColumn("dbo.Promotion", "Image");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Promotion", "Image", c => c.String());
            DropForeignKey("dbo.Promotion", "PictureId", "dbo.Picture");
            DropIndex("dbo.Promotion", new[] { "PictureId" });
            DropColumn("dbo.Promotion", "DisplayOrder");
            DropColumn("dbo.Promotion", "Published");
            DropColumn("dbo.Promotion", "RedirectUrl");
            DropColumn("dbo.Promotion", "PictureUrl");
            DropColumn("dbo.Promotion", "PictureId");
        }
    }
}
