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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using DataTables.AspNet.AspNetCore;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Http;

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

            builder.AddUserSecrets("dariaman46@");

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<JbsDbContext>(options => options.UseMySQL(Configuration.GetConnectionString("jbsDB")));
            services.AddDbContext<Life21DbContext>(options => options.UseMySQL(Configuration.GetConnectionString("life21")));
            //services.AddDbContext<Life21pDbContext>(options => options.UseMySQL(Configuration.GetConnectionString("life21p")));
            services.AddDbContext<UserDbContext>(options => options.UseMySQL(Configuration.GetConnectionString("jbsUser")));

            services.AddSingleton<ITempDataProvider, CookieTempDataProvider>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<Vereyon.Web.IFlashMessage, Vereyon.Web.FlashMessage>();

            services.AddIdentity<ApplicationUser, IdentityRole>(
                options =>
                {
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
            services.AddMvc(config =>
            {
                var policy = new AuthorizationPolicyBuilder()
                                 .RequireAuthenticatedUser()
                                 .Build();
                config.Filters.Add(new AuthorizeFilter(policy));
            });

            services.AddMvc();
            //    .AddJsonOptions(options => {
            //    // handle loops correctly
            //    options.SerializerSettings.ReferenceLoopHandling =
            //        Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            //    // use standard name conversion of properties
            //    options.SerializerSettings.ContractResolver =
            //        new CamelCasePropertyNamesContractResolver();
            //    // include $id property in the output
            //    options.SerializerSettings.PreserveReferencesHandling =
            //        PreserveReferencesHandling.Objects;
            //});

            services.AddMvcGrid();
            // Adds a default in-memory implementation of IDistributedCache.
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                // Set a short timeout for easy testing.
                options.IdleTimeout = TimeSpan.FromDays(1);
                options.CookieHttpOnly = true;
            });

            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
            services.RegisterDataTables();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {

            app.UseSession();
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseExceptionHandler("/Home/Error");

            app.UseStaticFiles();
            app.UseIdentity();
            app.UseDeveloperExceptionPage();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
