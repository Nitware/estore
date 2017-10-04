
using Autofac;
using Autofac.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Fakes;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Infrastructure.DependencyManagement;
using SmartStore.GTPay.Data;
using SmartStore.GTPay.Models;
using SmartStore.GTPay.Services;
using System.Web;

namespace SmartStore.GTPay
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, bool isActiveModule)
        {
            //builder.RegisterType<ShippingByWeightService>().As<IShippingByWeightService>().InstancePerRequest();
            //builder.Register(HttpContextBaseFactory).As<HttpContextBase>();

            builder.RegisterType<GatewayLuncher>().As<IGatewayLuncher>().InstancePerRequest();
            //builder.RegisterType<CardIconManager>().As<ICardIconManager>().InstancePerRequest();
            

            // data layer
            // register named context
            builder.Register<IDbContext>(c => new GTPayObjectContext(DataSettings.Current.DataConnectionString))
                .Named<IDbContext>(GTPayObjectContext.ALIASKEY)
                .InstancePerRequest();
            
            builder.Register<GTPayObjectContext>(c => new GTPayObjectContext(DataSettings.Current.DataConnectionString))
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