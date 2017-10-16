namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class promotionset : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Promotion", "TitleFontSize", c => c.Int(nullable: false));
            AlterColumn("dbo.Promotion", "SubTitleFontSize", c => c.Int(nullable: false));
            AlterColumn("dbo.Promotion", "DiscountTextFontSize", c => c.Int(nullable: false));
            AlterColumn("dbo.Promotion", "TextFrameBackground", c => c.Int(nullable: false));
            AlterColumn("dbo.Promotion", "TextFrameHeight", c => c.Int(nullable: false));
            AlterColumn("dbo.Promotion", "TextFrameWidth", c => c.Int(nullable: false));
            AlterColumn("dbo.Promotion", "DiscountPercentage", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Promotion", "DiscountPercentage", c => c.Double(nullable: false));
            AlterColumn("dbo.Promotion", "TextFrameWidth", c => c.String());
            AlterColumn("dbo.Promotion", "TextFrameHeight", c => c.String());
            AlterColumn("dbo.Promotion", "TextFrameBackground", c => c.String());
            AlterColumn("dbo.Promotion", "DiscountTextFontSize", c => c.String());
            AlterColumn("dbo.Promotion", "SubTitleFontSize", c => c.String());
            AlterColumn("dbo.Promotion", "TitleFontSize", c => c.String());
        }
    }
}
