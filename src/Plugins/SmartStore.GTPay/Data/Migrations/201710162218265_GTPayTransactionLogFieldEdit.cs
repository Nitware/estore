namespace SmartStore.GTPay.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class GTPayTransactionLogFieldEdit : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.GTPayTransactionLog", "GTPaySupportedCurrencyId", "dbo.GTPayCurrency");
            DropIndex("dbo.GTPayTransactionLog", new[] { "GTPaySupportedCurrencyId" });
            AddColumn("dbo.GTPayTransactionLog", "CurrencyAlias", c => c.String(maxLength: 5));
            AddColumn("dbo.GTPayTransactionLog", "Gateway", c => c.String(maxLength: 10));
            DropColumn("dbo.GTPayTransactionLog", "GTPaySupportedCurrencyId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.GTPayTransactionLog", "GTPaySupportedCurrencyId", c => c.Int(nullable: false));
            DropColumn("dbo.GTPayTransactionLog", "Gateway");
            DropColumn("dbo.GTPayTransactionLog", "CurrencyAlias");
            CreateIndex("dbo.GTPayTransactionLog", "GTPaySupportedCurrencyId");
            AddForeignKey("dbo.GTPayTransactionLog", "GTPaySupportedCurrencyId", "dbo.GTPayCurrency", "Id", cascadeDelete: true);
        }
    }
}
