using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Autofac;
using Autofac.Integration.Mvc;
using SmartStore.Core.Data;
using SmartStore.GTPay.Data;
using SmartStore.GTPay.Filters;
using SmartStore.GTPay.Interfaces;
using SmartStore.GTPay.Services;
using SmartStore.Web.Controllers;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Infrastructure.DependencyManagement;
using SmartStore.GTPay.Domain;
using SmartStore.Data;
using Autofac.Core;

namespace SmartStore.GTPay
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, bool isActiveModule)
        {
            builder.RegisterType<GatewayLuncher>().As<IGatewayLuncher>().InstancePerRequest();
            builder.RegisterType<GTPayCurrencyService>().As<IGTPayCurrencyService>().InstancePerRequest();
            builder.RegisterType<TransactionStatusService>().As<ITransactionStatusService>().InstancePerRequest();
            builder.RegisterType<TransactionLogService>().As<ITransactionLogService>().InstancePerRequest();

            // data layer
            // register named context
            builder.Register<IDbContext>(c => new GTPayObjectContext(DataSettings.Current.DataConnectionString))
                .Named<IDbContext>(GTPayObjectContext.ALIASKEY)
                .InstancePerRequest();

            builder.Register<GTPayObjectContext>(c => new GTPayObjectContext(DataSettings.Current.DataConnectionString))
                .InstancePerRequest();

            builder.RegisterType<CheckoutConfirmWidgetZoneFilter>()
                    .AsActionFilterFor<CheckoutController>(x => x.Confirm())
                    .InstancePerRequest();

            builder.RegisterType<OrderDetailsWidgetZoneFilter>()
                 .AsActionFilterFor<OrderController>(x => x.Details(0))
                 .InstancePerRequest();

            builder.RegisterType<CheckoutCompletedWidgetZoneFilter>()
                   .AsActionFilterFor<CheckoutController>(x => x.Completed())
                   .InstancePerRequest();

            // override required repository with our custom context
            builder.RegisterType<EfRepository<GTPayTransactionLog>>()
                .As<IRepository<GTPayTransactionLog>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>(GTPayObjectContext.ALIASKEY))
                .InstancePerRequest();

            builder.RegisterType<EfRepository<GTPayTransactionStatus>>()
               .As<IRepository<GTPayTransactionStatus>>()
               .WithParameter(ResolvedParameter.ForNamed<IDbContext>(GTPayObjectContext.ALIASKEY))
               .InstancePerRequest();

            builder.RegisterType<EfRepository<GTPaySupportedCurrency>>()
               .As<IRepository<GTPaySupportedCurrency>>()
               .WithParameter(ResolvedParameter.ForNamed<IDbContext>(GTPayObjectContext.ALIASKEY))
               .InstancePerRequest();
        }

        public int Order
        {
            get { return 1; }
        }





    }
}