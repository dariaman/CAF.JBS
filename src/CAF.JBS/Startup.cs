using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
//using MySQL.Data.EntityFrameworkCore.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
//using Microsoft.ApplicationInsights.AspNetCore;
using CAF.JBS.Data;
using CAF.JBS.Models;
using CAF.JBS.Services;
using NonFactors.Mvc.Grid;
using System.Reflection;
using MySQL.Data.Entity.Extensions;

namespace CAF.JBS
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
                builder.AddUserSecrets("dariaman46@");
                //builder.AddApplicationInsightsSettings(developerMode: true);
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            //services.AddApplicationInsightsTelemetry(Configuration);

            services.AddDbContext<JbsDbContext>(options => options.UseMySQL(Configuration.GetConnectionString("jbsDB")));
            services.AddDbContext<Life21DbContext>(options => options.UseMySQL(Configuration.GetConnectionString("life21")));
            services.AddDbContext<UserDbContext>(options => options.UseMySQL(Configuration.GetConnectionString("jbsUser")));

            services.AddIdentity<ApplicationUser, IdentityRole>(
                options => {
                    // configure identity options
                    options.Password.RequireDigit = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    //options.Password.RequiredLength = 7;
                    options.Cookies.ApplicationCookie.AutomaticAuthenticate = true;
                    options.Cookies.ApplicationCookie.AutomaticChallenge = true;
                    options.Cookies.ApplicationCookie.LoginPath = "/Account/Login";

                    // User settings
                    options.User.RequireUniqueEmail = true;
                })
                .AddEntityFrameworkStores<UserDbContext>()
                .AddDefaultTokenProviders();
            
            services.AddMvcCore().AddViewLocalization();
            //services.AddMvc();
            services.AddMvcGrid();

            // Adds a default in-memory implementation of IDistributedCache.
            services.AddDistributedMemoryCache();

            services.AddSession(options =>
            {
                // Set a short timeout for easy testing.
                options.IdleTimeout = TimeSpan.FromSeconds(10);
                options.CookieHttpOnly = true;
            });

            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseSession();
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseIdentity();
            app.UseDeveloperExceptionPage();

            app.UseMvc(routes =>
            {
                //routes.MapRoute(name: "SampleSolution",
                //    template: "{controller=Settings}/{action=ViewApplicationSolution}/{ bugID ?}/{ view ?}");
                //routes.MapRoute(name: "Sample",
                //    template: "{controller=Settings}/{action=ViewApplication}/{applicationID?}/{ applicationVersionID ?}/{ view ?}");

                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
