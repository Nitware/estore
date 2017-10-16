namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class promotion3 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Promotion", "TitleFontType", c => c.String());
            AddColumn("dbo.Promotion", "TitleFontSize", c => c.String());
            AddColumn("dbo.Promotion", "TitleFontColor", c => c.String());
            AddColumn("dbo.Promotion", "SubTitle", c => c.String());
            AddColumn("dbo.Promotion", "SubTitleFontType", c => c.String());
            AddColumn("dbo.Promotion", "SubTitleFontSize", c => c.String());
            AddColumn("dbo.Promotion", "SubTitleFontColor", c => c.String());
            AddColumn("dbo.Promotion", "DiscountText", c => c.String());
            AddColumn("dbo.Promotion", "DiscountTextFontType", c => c.String());
            AddColumn("dbo.Promotion", "DiscountTextFontSize", c => c.String());
            AddColumn("dbo.Promotion", "DiscountTextFontColor", c => c.String());
            AddColumn("dbo.Promotion", "Description", c => c.String());
            AddColumn("dbo.Promotion", "TextFrameType", c => c.String());
            AddColumn("dbo.Promotion", "TextFrameBackground", c => c.String());
            AddColumn("dbo.Promotion", "TextFrameHeight", c => c.String());
            AddColumn("dbo.Promotion", "TextFrameWidth", c => c.String());
            AddColumn("dbo.Promotion", "CreationDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.Promotion", "CreatedBy", c => c.String());
            AddColumn("dbo.Promotion", "ExpiryDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.Promotion", "NoOfColumn", c => c.Int(nullable: false));
            DropColumn("dbo.Promotion", "DiscountConditions");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Promotion", "DiscountConditions", c => c.String());
            DropColumn("dbo.Promotion", "NoOfColumn");
            DropColumn("dbo.Promotion", "ExpiryDate");
            DropColumn("dbo.Promotion", "CreatedBy");
            DropColumn("dbo.Promotion", "CreationDate");
            DropColumn("dbo.Promotion", "TextFrameWidth");
            DropColumn("dbo.Promotion", "TextFrameHeight");
            DropColumn("dbo.Promotion", "TextFrameBackground");
            DropColumn("dbo.Promotion", "TextFrameType");
            DropColumn("dbo.Promotion", "Description");
            DropColumn("dbo.Promotion", "DiscountTextFontColor");
            DropColumn("dbo.Promotion", "DiscountTextFontSize");
            DropColumn("dbo.Promotion", "DiscountTextFontType");
            DropColumn("dbo.Promotion", "DiscountText");
            DropColumn("dbo.Promotion", "SubTitleFontColor");
            DropColumn("dbo.Promotion", "SubTitleFontSize");
            DropColumn("dbo.Promotion", "SubTitleFontType");
            DropColumn("dbo.Promotion", "SubTitle");
            DropColumn("dbo.Promotion", "TitleFontColor");
            DropColumn("dbo.Promotion", "TitleFontSize");
            DropColumn("dbo.Promotion", "TitleFontType");
        }
    }
}
