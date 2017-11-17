using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using System.IO;
using Microsoft.AspNetCore.DataProtection;
using TeamStore.Keeper.Interfaces;
using TeamStore.Keeper.Services;
using TeamStore.Keeper.DataAccess;
using Microsoft.AspNetCore.Http;

namespace TeamStore
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var fileName = Configuration["DataAccess:SQLiteDbFileName"];
            var eventsFileName = Configuration["DataAccess:SQLiteEventsDbFileName"];
            var connectionString = "Data Source=" + fileName;
            var connectionStringEvents = "Data Source=" + eventsFileName;

            // Set up the DbContext for data access
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlite(connectionString, b => b.MigrationsAssembly("TeamStore.Keeper"));
            });
            services.AddDbContext<EventDbContext>(options =>
            {
                options.UseSqlite(connectionStringEvents, b => b.MigrationsAssembly("TeamStore.Keeper"));
            });

            // We use Session and In-memory cache for token storage
            // This will not scale across applications and users need ro re-authenticate on restart
            services.AddMemoryCache();
            services.AddSession();

            // Set up services
            services.AddScoped<IEventService, EventService>(); // needs to be before auth setup
            services.AddScoped<IProjectsService, ProjectsService>(); 
            services.AddScoped<IGraphService, GraphService>(); 
            services.AddScoped<IAssetService, AssetService>();
            services.AddScoped<IPermissionService, PermissionService>(); 
            services.AddScoped<IEncryptionService, EncryptionService>(); 
            services.AddScoped<IApplicationIdentityService, ApplicationIdentityService>(); 
            services.AddScoped<IAccessTokenRetriever, UserAccessTokenRetriever>();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();


            // Sets up Azure Ad Open Id Connect auth
            services.AddAuthentication(sharedOptions =>
            {
                sharedOptions.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                sharedOptions.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddAzureAd(options =>
            {
                Configuration.Bind("AzureAd", options);
            })
            .AddCookie();

            // Set up MVC
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
