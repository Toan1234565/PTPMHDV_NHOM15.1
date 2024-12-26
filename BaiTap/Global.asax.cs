using BaiTap.IRepository;
using BaiTap.Models;
using BaiTap.Repositories;
using BaiTap.UnitOfWork;
using BaiTap.DependencyResolvers;
using Microsoft.Extensions.DependencyInjection;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace BaiTap
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            var services = new ServiceCollection();
            ConfigureServices(services);
            var serviceProvider = services.BuildServiceProvider();
            GlobalConfiguration.Configuration.DependencyResolver = new DefaultDependencyResolver(serviceProvider);
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<Model1>();
            services.AddScoped<IUnitOfWork, BaiTap.UnitOfWork.UnitOfWork>();
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<ProductService>();
        }
    }
}
