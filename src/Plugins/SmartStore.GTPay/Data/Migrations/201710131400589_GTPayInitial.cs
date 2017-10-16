namespace SmartStore.GTPay.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class GTPayInitial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.GTPayTransactionLog",
                c => new
                    {
                        TransactionRefNo = c.String(nullable: false, maxLength: 40),
                        GTPayTransactionStatusId = c.Int(nullable: false),
                        ApprovedAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        AmountInUnit = c.Long(nullable: false),
                        OrderId = c.Int(),
                        ResponseCode = c.String(maxLength: 5),
                        ResponseDescription = c.String(maxLength: 4000),
                        StatusReason = c.String(maxLength: 400),
                        DatePaid = c.DateTime(),
                        CardNo = c.String(maxLength: 30),
                        MerchantReference = c.String(maxLength: 150),
                        IsAmountMismatch = c.Boolean(),
                        TransactionDate = c.DateTime(),
                        CurrencyCode = c.Int(),
                        Gateway = c.String(maxLength: 10),
                        VerificationHash = c.String(maxLength: 400),
                        Id = c.Int(nullable: false, identity: true),
                    })
                .PrimaryKey(t => t.TransactionRefNo)
                .ForeignKey("dbo.GTPayTransactionStatus", t => t.GTPayTransactionStatusId, cascadeDelete: true)
                .Index(t => t.GTPayTransactionStatusId);
            
            CreateTable(
                "dbo.GTPayTransactionStatus",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        StatusName = c.String(nullable: false, maxLength: 4000),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.GTPayCurrency",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Code = c.Int(nullable: false),
                        Alias = c.String(),
                        Name = c.String(nullable: false, maxLength: 20),
                        Gateway = c.String(nullable: false, maxLength: 10),
                        IsSupported = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Code, unique: true);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.GTPayTransactionLog", "GTPayTransactionStatusId", "dbo.GTPayTransactionStatus");
            DropIndex("dbo.GTPayCurrency", new[] { "Code" });
            DropIndex("dbo.GTPayTransactionLog", new[] { "GTPayTransactionStatusId" });
            DropTable("dbo.GTPayCurrency");
            DropTable("dbo.GTPayTransactionStatus");
            DropTable("dbo.GTPayTransactionLog");
        }
    }
}
