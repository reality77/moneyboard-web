using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Localization;
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
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<ApiClient>(s => new ApiClient(s.GetService<ILogger<ApiClient>>(), s.GetService<IConfiguration>(), s.GetService<IHttpContextAccessor>()));

            services.AddAuthentication(opt => {
                    opt.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    opt.DefaultChallengeScheme = "OpenIdConnect";
                })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, c =>
                {
                    c.AccessDeniedPath = "/Home/Forbidden";
                    // L'expiration doit être inférieure à l'expiration de l'access token OpenIdConnect
                    c.ExpireTimeSpan = TimeSpan.FromMinutes(Configuration.GetValue<int>("CookieExpiration"));
                    c.SlidingExpiration = true;
                })
            .AddOpenIdConnect("OpenIdConnect", opt => 
                {
                    var authority = Configuration.GetValue<string>("OpenIdConnect:Authority");
                    var configPublicAuthorities = Configuration.GetSection("OpenIdConnect:PublicAuthorityAliases");
                    Dictionary<string, string> dicPublicAuthorities = null;

                    if(configPublicAuthorities != null)
                    {
                        dicPublicAuthorities = configPublicAuthorities.GetChildren().ToDictionary(
                            keySelector: c => c.GetValue<string>("Host"),
                            elementSelector: c => c.GetValue<string>("Authority")
                        );
                    }

                    opt.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    opt.ClientId = Configuration.GetValue<string>("OpenIdConnect:ClientId");
                    opt.ClientSecret = Configuration.GetValue<string>("OpenIdConnect:ClientSecret");
                    opt.Authority = authority;
                    opt.RequireHttpsMetadata = false;
                    opt.ResponseType = OpenIdConnectResponseType.Code;
                    opt.GetClaimsFromUserInfoEndpoint = true;
                    opt.SaveTokens = true;

                    if(dicPublicAuthorities != null)
                    {
                        var validIssuers = dicPublicAuthorities.Select(x => x.Value).Distinct().ToList();
                        
                        if(!validIssuers.Contains(authority))
                            validIssuers.Add(authority);

                        opt.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                        {
                            ValidIssuers = validIssuers,
                        };
                    }

                    // --- Fix pour pb de cookie non créé par Chrome (https://stackoverflow.com/a/60668367)
                    opt.NonceCookie.SameSite = SameSiteMode.Unspecified;
                    opt.CorrelationCookie.SameSite = SameSiteMode.Unspecified;
                    // --- fin fix

                    // --- Fix pour pb certificat invalide
                    var allowInvalidSSLCert = Configuration.GetValue<bool>("OpenIdConnect:AllowInvalidAuthoritySSLCertificate");
                    
                    if(allowInvalidSSLCert)
                    {
                        var httpHandler = new System.Net.Http.HttpClientHandler();
                        httpHandler.ServerCertificateCustomValidationCallback = System.Net.Http.HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
                        opt.BackchannelHttpHandler = httpHandler;
                    }
                    // --- fin fix

                    // --- gestion des alias publics d'Authority en fonction de l'appelant
                    // NE FONCTIONNE PAS BIEN : Retourne un 401 après la redirection pour le login
                    if(dicPublicAuthorities.Count() > 0)
                    {
                        opt.Events = new OpenIdConnectEvents
                        {
                            OnTokenValidated = context => 
                            {
                                if(dicPublicAuthorities.ContainsKey(context.Request.Host.Value))
                                {
                                    var issuer = dicPublicAuthorities[context.Request.Host.Value];
                                    //_logger.LogInformation($"OnRedirectToIdentityProvider : Issuer replaced. Before = {redirectContext.ProtocolMessage.IssuerAddress}, After = {issuer}");                                    

                                    context.ProtocolMessage.IssuerAddress = context.ProtocolMessage.IssuerAddress.Replace(issuer, authority);
                                }
                                return Task.FromResult(0);
                            },
                            OnRedirectToIdentityProvider = redirectContext =>
                            {
                                //_logger.LogInformation($"OnRedirectToIdentityProvider : Host detected = {redirectContext.Request.Host.Value}");
                                if(dicPublicAuthorities.ContainsKey(redirectContext.Request.Host.Value))
                                {
                                    var issuer = dicPublicAuthorities[redirectContext.Request.Host.Value];
                                    //_logger.LogInformation($"OnRedirectToIdentityProvider : Issuer replaced. Before = {redirectContext.ProtocolMessage.IssuerAddress}, After = {issuer}");                                    

                                    redirectContext.ProtocolMessage.IssuerAddress = redirectContext.ProtocolMessage.IssuerAddress.Replace(authority, issuer);
                                }
                                return Task.FromResult(0);
                            },
                            OnAuthenticationFailed = context => 
                            {
                                return Task.FromResult(0);
                            },
                            OnTokenResponseReceived = context => 
                            {
                                return Task.FromResult(0);
                            },
                            OnAuthorizationCodeReceived = context => 
                            {
                                return Task.FromResult(0);
                            },
                            OnTicketReceived = context => 
                            {
                                return Task.FromResult(0);
                            },
                        };
                    }

                });

            services.AddAuthorization();

            services.Configure<RequestLocalizationOptions>(options =>
            {
                options.DefaultRequestCulture = new RequestCulture("fr-FR");
            });

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
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true; 
            
            //app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRequestLocalization();

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
