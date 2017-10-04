
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

        static HttpContextBase HttpContextBaseFactory(IComponentContext ctx)
        {
            if (IsRequestValid())
            {
                return new HttpContextWrapper(HttpContext.Current);
            }

            // TODO: determine store url

            // register FakeHttpContext when HttpContext is not available
            return new FakeHttpContext("~/");
        }
        static bool IsRequestValid()
        {
            if (HttpContext.Current == null)
                return false;

            try
            {
                // The "Request" property throws at application startup on IIS integrated pipeline mode
                var req = HttpContext.Current.Request;
            }
            catch (System.Exception)
            {
                return false;
            }

            return true;
        }



    }
}