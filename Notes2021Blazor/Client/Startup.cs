using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components.Builder;
using Microsoft.AspNetCore.Identity;
using System;
using Blazored.Modal;

namespace Notes2021Blazor.Client
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddBlazoredModal();
            services.AddBlazoredLocalStorage();
            services.AddAuthorizationCore();
            services.AddScoped<AuthenticationStateProvider, ApiAuthenticationStateProvider>();
            services.AddScoped<IAuthService, AuthService>();
        }

        public void Configure(IComponentsApplicationBuilder app)
        {
            app.AddComponent<App>("app");

        }
    }
}