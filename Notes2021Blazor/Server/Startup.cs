using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Notes2021Blazor.Shared;
using System;
using System.Linq;
using System.Text;

namespace Notes2021Blazor.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }


        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddResponseCompression(opts =>
            {
                opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                    new[] { "application/octet-stream" });
            });


            services.AddDbContext<NotesDbContext>(
                options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddDefaultIdentity<IdentityUser>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<NotesDbContext>();


            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = Configuration["JwtIssuer"],
                    ValidAudience = Configuration["JwtAudience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JwtSecurityKey"]))
                };
            });

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


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseResponseCompression();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBlazorDebugging();
            }

            app.UseStaticFiles();
            app.UseClientSideBlazorFiles<Client.Program>();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
                endpoints.MapFallbackToClientSideBlazor<Client.Program>("index.html");
            });
        }
    }
}
