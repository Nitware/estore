
using Autofac;
using Autofac.Core;
using Autofac.Integration.Mvc;
using SmartStore.Core.Data;
using SmartStore.Core.Fakes;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Infrastructure.DependencyManagement;
using SmartStore.GTPay.Data;
using SmartStore.GTPay.Filters;
using SmartStore.GTPay.Models;
using SmartStore.GTPay.Services;
using SmartStore.Web.Controllers;
using System.Web;

namespace SmartStore.GTPay
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        //private IGatewayLuncher _gatewayLuncher;

        //public DependencyRegistrar(IGatewayLuncher gatewayLuncher)
        //{
        //    _gatewayLuncher = gatewayLuncher;
        //}

        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, bool isActiveModule)
        {
            builder.RegisterType<GatewayLuncher>().As<IGatewayLuncher>().InstancePerRequest();
            //builder.RegisterType<CardIconManager>().As<ICardIconManager>().InstancePerRequest();
            


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
                 .AsActionFilterFor<OrderController>(x => x.Details(GatewayLuncher.OrderId))
                 .InstancePerRequest();
            
            builder.RegisterType<CheckoutCompletedWidgetZoneFilter>()
                   .AsActionFilterFor<CheckoutController>(x => x.Completed())
                   .InstancePerRequest();






            //// override required repository with our custom context
            //builder.RegisterType<EfRepository<ShippingByWeightRecord>>()
            //    .As<IRepository<ShippingByWeightRecord>>()
            //    .WithParameter(ResolvedParameter.ForNamed<IDbContext>(GTPayObjectContext.ALIASKEY))
            //    .InstancePerRequest();
        }

        public int Order
        {
            get { return 1; }
        }

       



    }
}