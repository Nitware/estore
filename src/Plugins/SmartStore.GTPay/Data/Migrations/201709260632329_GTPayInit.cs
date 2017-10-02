namespace SmartStore.GTPay.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class GTPayInit : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.GTPayTransactionLog",
                c => new
                {
                    TransactionRefNo = c.String(nullable: false, maxLength: 128),
                    GTPayTransactionStatusId = c.Int(nullable: false),
                    AmountInUnit = c.Long(nullable: false),
                    ResponseCode = c.String(maxLength: 5),
                    ResponseDescription = c.String(maxLength: 4000),
                    StatusReason = c.String(maxLength: 300),
                    DatePaid = c.DateTime(),
                    CardNo = c.String(maxLength: 30),
                    WebPayRefNo = c.String(maxLength: 30),
                    PayRefNo = c.String(maxLength: 30),
                    IsAmountMismatch = c.Boolean(),
                })
                .PrimaryKey(t => t.TransactionRefNo)
                .ForeignKey("dbo.GTPayTransactionStatus", t => t.GTPayTransactionStatusId, cascadeDelete: true)
                .Index(t => t.GTPayTransactionStatusId);

            CreateTable(
                "dbo.GTPayTransactionStatus",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    StatusDescription = c.String(),
                })
                .PrimaryKey(t => t.Id);

        }
        
        public override void Down()
        {
            DropForeignKey("dbo.GTPayTransactionLog", "GTPayTransactionStatusId", "dbo.GTPayTransactionStatus");
            DropIndex("dbo.GTPayTransactionLog", new[] { "GTPayTransactionStatusId" });
            DropTable("dbo.GTPayTransactionStatus");
            DropTable("dbo.GTPayTransactionLog");
        }
    }
}
