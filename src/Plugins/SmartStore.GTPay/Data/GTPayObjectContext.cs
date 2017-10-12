using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using SmartStore.Data;
using SmartStore.Data.Setup;
using System.Data.Entity;
using SmartStore.GTPay.Data.Migrations;

namespace SmartStore.GTPay.Data
{
    public class GTPayObjectContext : ObjectContextBase
    {
        public const string ALIASKEY = "sm_object_context_gtpay";

        static GTPayObjectContext()
        {
            var initializer = new MigrateDatabaseInitializer<GTPayObjectContext, Configuration>
            {
                TablesToCheck = new[] { "GTPaySupportedCurrency", "GTPayTransactionLog", "GTPayTransactionStatus" }
                //TablesToCheck = new[] { "GTPay" }
            };
            Database.SetInitializer(initializer);
        }

        /// <summary>
        /// For tooling support, e.g. EF Migrations
        /// </summary>
        public GTPayObjectContext()
            : base()
        {
        }

        public GTPayObjectContext(string nameOrConnectionString)
            : base(nameOrConnectionString, ALIASKEY)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new GTPayTransactionLogRecordMap());
            //modelBuilder.Configurations.Add(new GTPaySupportedCurrencyRecordMap());
            //modelBuilder.Configurations.Add(new GTPayTransactionStatusRecordMap());


            //disable EdmMetadata generation
            //modelBuilder.Conventions.Remove<IncludeMetadataConvention>();
            base.OnModelCreating(modelBuilder);
        }



    }
}