namespace SmartStore.GTPay.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class GTPayOnlyTransactionRefNoRequired : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.GTPayTransactionLog", "ApprovedAmount", c => c.Decimal(precision: 18, scale: 2));
            AlterColumn("dbo.GTPayTransactionLog", "AmountInUnit", c => c.Long());
            AlterColumn("dbo.GTPayTransactionStatus", "StatusName", c => c.String(nullable: false, maxLength: 100));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.GTPayTransactionStatus", "StatusName", c => c.String(nullable: false, maxLength: 4000));
            AlterColumn("dbo.GTPayTransactionLog", "AmountInUnit", c => c.Long(nullable: false));
            AlterColumn("dbo.GTPayTransactionLog", "ApprovedAmount", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
    }
}
