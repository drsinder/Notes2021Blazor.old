using Hangfire;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Notes2021.Services;
using Notes2021Blazor.Shared;
using System;

namespace Notes2021
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
            services.AddDbContext<NotesDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));

            services.AddHangfire(x => x.UseSqlServerStorage(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddRoleManager<RoleManager<IdentityRole>>()
                .AddDefaultTokenProviders()
                .AddEntityFrameworkStores<NotesDbContext>();


            services.Configure<IdentityOptions>(options =>
            {
                // Default Lockout settings.
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // Default Password settings.
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 1;

            });

            services.AddDistributedMemoryCache();

            services.AddSession(options =>
            {
                // Set a short timeout for easy testing.
                //options.IdleTimeout = TimeSpan.FromSeconds(10);
                //options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            Globals.StartupDateTime = DateTime.Now.ToUniversalTime();

            Globals.ProductionUrl = Configuration["ProductionUrl"];

            Globals.PathBase = Configuration["PathBase"];

            //Globals.InstKey = Configuration["InstKey"];

            Globals.TimeZoneDefaultID = int.Parse(Configuration["DefaultTZ"]);
            Globals.SendGridApiKey = Configuration["SendGridApiKey"];
            Globals.SendGridEmail = Configuration["SendGridEmail"];
            Globals.SendGridName = Configuration["SendGridName"];

            Globals.PusherAppId = Configuration["Pusher:AppId"];
            Globals.PusherKey = Configuration["Pusher:AppKey"];
            Globals.PusherSecret = Configuration["Pusher:AppSecret"];
            Globals.PusherCluster = Configuration["Pusher:Cluster"];

            Globals.ChatKitAppLoc = Configuration["ChatKit:InstanceLocator"];
            Globals.ChatKitKey = Configuration["ChatKit:AppKey"];

            Globals.DBConnectString = Configuration.GetConnectionString("DefaultConnection");

            Globals.PrimeAdminName = "Dale Sinder";
            Globals.PrimeAdminEmail = "sinder@illinois.edu";

            try
            {
                Globals.PrimeAdminName = Configuration["PrimeAdminName"];
                Globals.PrimeAdminEmail = Configuration["PrimeAdminEmail"];
            }
            catch
            {
                //ignore
            }

            services.AddSingleton<IEmailSender, EmailSender>();


            services.AddControllersWithViews();
            services.AddRazorPages();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UsePathBase(Configuration["PathBase"]);

            UpdateDatabase(app);

            if (true || env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseSession();


            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseHangfireServer();
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[] { new Notes2021AuthorizationFilter() }
            });



            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }

        private static void UpdateDatabase(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                using (var context = serviceScope.ServiceProvider.GetService<NotesDbContext>())
                {
                    context.Database.Migrate();
                }
            }
        }

    }

    public class Notes2021AuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();

            return httpContext.User.IsInRole("Admin");
        }
    }

}
