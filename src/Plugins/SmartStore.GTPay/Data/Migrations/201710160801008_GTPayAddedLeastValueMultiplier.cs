namespace SmartStore.GTPay.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class GTPayAddedLeastValueMultiplier : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.GTPayTransactionLog", "GTPaySupportedCurrencyId", c => c.Int(nullable: false));
            AddColumn("dbo.GTPayTransactionLog", "FullVerificationHash", c => c.String(maxLength: 400));
            AddColumn("dbo.GTPayCurrency", "LeastValueUnitMultiplier", c => c.Int(nullable: false));
            CreateIndex("dbo.GTPayTransactionLog", "GTPaySupportedCurrencyId");
            AddForeignKey("dbo.GTPayTransactionLog", "GTPaySupportedCurrencyId", "dbo.GTPayCurrency", "Id", cascadeDelete: true);
            DropColumn("dbo.GTPayTransactionLog", "StatusReason");
            DropColumn("dbo.GTPayTransactionLog", "CardNo");
            DropColumn("dbo.GTPayTransactionLog", "CurrencyCode");
            DropColumn("dbo.GTPayTransactionLog", "Gateway");
        }
        
        public override void Down()
        {
            AddColumn("dbo.GTPayTransactionLog", "Gateway", c => c.String(maxLength: 10));
            AddColumn("dbo.GTPayTransactionLog", "CurrencyCode", c => c.Int());
            AddColumn("dbo.GTPayTransactionLog", "CardNo", c => c.String(maxLength: 30));
            AddColumn("dbo.GTPayTransactionLog", "StatusReason", c => c.String(maxLength: 400));
            DropForeignKey("dbo.GTPayTransactionLog", "GTPaySupportedCurrencyId", "dbo.GTPayCurrency");
            DropIndex("dbo.GTPayTransactionLog", new[] { "GTPaySupportedCurrencyId" });
            DropColumn("dbo.GTPayCurrency", "LeastValueUnitMultiplier");
            DropColumn("dbo.GTPayTransactionLog", "FullVerificationHash");
            DropColumn("dbo.GTPayTransactionLog", "GTPaySupportedCurrencyId");
        }
    }
}
