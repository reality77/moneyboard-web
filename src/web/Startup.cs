using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using web.Services;

namespace web
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
            services.AddTransient<ApiClient>(s => new ApiClient(s.GetService<ILogger<ApiClient>>(), s.GetService<IConfiguration>()));

            services.AddAuthentication(opt => {
                    opt.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    opt.DefaultChallengeScheme = "Keycloak";
                })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, c =>
                {
                    c.AccessDeniedPath = "/Home/Forbidden";
                    // The default setting for cookie expiration is 14 days. SlidingExpiration is set to true by default
                    c.ExpireTimeSpan = TimeSpan.FromHours(1);
                    c.SlidingExpiration = true;
                })
            .AddOpenIdConnect("OpenIdConnect", opt => 
                {
                    opt.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    opt.ClientId = Configuration.GetValue<string>("OpenIdConnect:ClientId");
                    opt.ClientSecret = Configuration.GetValue<string>("OpenIdConnect:ClientSecret");
                    opt.Authority = Configuration.GetValue<string>("OpenIdConnect:Authority");
                    opt.MetadataAddress = Configuration.GetValue<string>("OpenIdConnect:Authority");
                    opt.RequireHttpsMetadata = false;
                    opt.ResponseType = OpenIdConnectResponseType.Code;
                    opt.GetClaimsFromUserInfoEndpoint = true;
                    opt.SaveTokens = true;
                    opt.MetadataAddress = Configuration.GetValue<string>("OpenIdConnect:Authority");
                });

            services.AddAuthorization();

            var mvcBuilder = services.AddControllersWithViews();

#if DEBUG
            mvcBuilder.AddRazorRuntimeCompilation();
#endif

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseForwardedHeaders();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true; 
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
