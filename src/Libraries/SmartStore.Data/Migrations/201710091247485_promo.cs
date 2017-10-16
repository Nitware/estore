namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class promo : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Promotion", "TextFrameBackground", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Promotion", "TextFrameBackground", c => c.Int(nullable: false));
        }
    }
}
