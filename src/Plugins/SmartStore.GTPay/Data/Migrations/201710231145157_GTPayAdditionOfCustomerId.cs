namespace SmartStore.GTPay.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class GTPayAdditionOfCustomerId : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.GTPayTransactionLog", "CustomerId", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.GTPayTransactionLog", "CustomerId");
        }
    }
}
