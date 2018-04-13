using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Builder
{
    public static class RoutingAndMvc
    {
        public static IRouteBuilder UseCustomRoutes(this IRouteBuilder routes)
        {
            routes.AddCloudscribeFileManagerRoutes();
            routes.MapRoute(
                name: "errorhandler",
                template: "oops/error/{statusCode?}",
                defaults: new { controller = "Oops", action = "Error" }
                );



            routes.MapRoute(
                name: "def",
                template: "{controller}/{action}"
                , defaults: new { controller = "Home", action = "Index" }
                );
            
            return routes;
        }

        public static IServiceCollection SetupMvc(
            this IServiceCollection services,
            bool sslIsAvailable
            )
        {
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto;
            });

            services.Configure<MvcOptions>(options =>
            {
                if (sslIsAvailable)
                {
                    options.Filters.Add(new RequireHttpsAttribute());
                }

            });

            services.AddRouting(options =>
            {
                options.LowercaseUrls = true;
            });

            services.AddMvc()
                .AddRazorOptions(options =>
                {
                    options.AddCloudscribeCommonEmbeddedViews();
                    options.AddCloudscribeNavigationBootstrap3Views();
                    options.AddCloudscribeCoreBootstrap3Views();
                    options.AddCloudscribeFileManagerBootstrap3Views();

                    options.ViewLocationExpanders.Add(new cloudscribe.Core.Web.Components.SiteViewLocationExpander());
                });

            return services;
        }

    }
}
