namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class promoAdvert_AddedMenuColorColumn : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Promotion", "MenuColor", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Promotion", "MenuColor");
        }
    }
}
