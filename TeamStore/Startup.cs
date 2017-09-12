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
using TeamStore.DataAccess;
using Microsoft.EntityFrameworkCore;
using TeamStore.Interfaces;
using TeamStore.Services;
using System.IO;
using Microsoft.AspNetCore.DataProtection;

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

            var connectionString = "Data Source=" + fileName;

            // Set up the DbContext for data access
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlite(connectionString);
            });

            // We use Session and In-memory cache for token storage
            // This will not scale across applications and users need ro re-authenticate on restart
            services.AddMemoryCache();
            services.AddSession();

            // Set up services
            services.AddScoped<IEventService, EventService>(); // needs to be before auth setup
            services.AddScoped<IProjectsService, ProjectsService>();
            services.AddScoped<IGraphService, GraphService>();

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

            // Looks up the key in the Keys folder. Will fail if it can't find it.
            //services.AddDataProtection()
            //    .DisableAutomaticKeyGeneration()
            //    .SetDefaultKeyLifetime(new TimeSpan(1230000, 12, 12))
            //    .SetApplicationName("TeamStore-UnitTests")
            //    .PersistKeysToFileSystem(new DirectoryInfo(Environment.CurrentDirectory + "\\Keys"));
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
