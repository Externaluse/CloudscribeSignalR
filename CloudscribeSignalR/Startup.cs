using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using cloudscribe.Core.Models;
using CloudscribeSignalR.Controllers;
using Microsoft.AspNetCore.SignalR;


namespace CloudscribeSignalR
{
    public class Startup
    {
        public Startup(
            IConfiguration configuration, 
            IHostingEnvironment env,
            ILogger<Startup> logger
            )
        {
            Configuration = configuration;
            Environment = env;
            _log = logger;

            SslIsAvailable = Configuration.GetValue<bool>("AppSettings:UseSsl");
        }

        private IConfiguration Configuration { get; set; }
        private IHostingEnvironment Environment { get; set; }
        private bool SslIsAvailable { get; set; }
        private ILogger _log;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //// **** VERY IMPORTANT *****
            // This is a custom extension method in Config/DataProtection.cs
            // These settings require your review to correctly configur data protection for your environment
            services.SetupDataProtection(Configuration, Environment);
            
            services.AddAuthorization(options =>
            {
                //https://docs.asp.net/en/latest/security/authorization/policies.html
                //** IMPORTANT ***
                //This is a custom extension method in Config/Authorization.cs
                //That is where you can review or customize or add additional authorization policies
                options.SetupAuthorizationPolicies();

            });

            //// **** IMPORTANT *****
            // This is a custom extension method in Config/CloudscribeFeatures.cs
            services.SetupDataStorage(Configuration);
            
            //*** Important ***
            // This is a custom extension method in Config/CloudscribeFeatures.cs
            services.SetupCloudscribeFeatures(Configuration);

            //*** Important ***
            // This is a custom extension method in Config/Localization.cs
            services.SetupLocalization();

            //*** Important ***
            // This is a custom extension method in Config/RoutingAndMvc.cs
            services.SetupMvc(SslIsAvailable);

            services.AddSignalR();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app, 
            IHostingEnvironment env,
            IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory,
            IOptions<cloudscribe.Core.Models.MultiTenantOptions> multiTenantOptionsAccessor,
            IOptions<RequestLocalizationOptions> localizationOptionsAccessor
            )
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/oops/error");
            }

            app.UseForwardedHeaders();
            app.UseStaticFiles();

            //app.UseSession();

            app.UseRequestLocalization(localizationOptionsAccessor.Value);

            var multiTenantOptions = multiTenantOptionsAccessor.Value;

            app.UseCloudscribeCore(
                    loggerFactory,
                    multiTenantOptions,
                    SslIsAvailable);

            // Add SignalR Endpoints. Be sure to add them after app.UseCloudscribeCore if you require Authorization information; the order of middleware is important
            app.UseSignalR(routes =>
            {
                routes.MapHub<SignalRHeartbeat>("/heartbeat");
                routes.MapHub<SignalRHub>("/signalr");
            });
            // Start a heartbeat timer to the clients
            TimerCallback signalRHeartBeat = async (x) => { await serviceProvider.GetService<IHubContext<SignalRHeartbeat>>().Clients.All.SendAsync("Heartbeat", DateTime.Now); };
            var timer = new Timer(signalRHeartBeat).Change(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(30));

            app.UseMvc(routes =>
            {
                routes.UseCustomRoutes();
            });
   
        }

        
        
    }
}