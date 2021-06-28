using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication1.Data;
using WebApplication1.Managers;
using WebApplication1.Models;
using WebApplication1.Services;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace WebApplication1
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
            services.AddDbContext<ApplicationDbContext>(options =>
            {
            options.UseNpgsql(Configuration.GetConnectionString("DBContext"))
                .UseSnakeCaseNamingConvention();
                   
                // to replace the default OpenIddict entities.
                options.UseOpenIddict();
            });

            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultUI()
                .AddDefaultTokenProviders();

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebApplication1", Version = "v1" });
            });
            services.AddSingleton<MovieSingleton>();

            services.AddScoped<UsuarioManager>();
            services.AddScoped<IGenericCRUD<Movie>, GenericCRUD<Movie>>();
            services.AddScoped<IGenericCRUD<Category>, GenericCRUD<Category>>();


            #region openidict core

            // Configure Identity to use the same JWT claims as OpenIddict instead
            // of the legacy WS-Federation claims it uses by default (ClaimTypes),
            // which saves you from doing the mapping in your authorization controller.
            services.Configure<IdentityOptions>(options =>
            {
                options.ClaimsIdentity.UserNameClaimType = Claims.Name;
                options.ClaimsIdentity.UserIdClaimType = Claims.Subject;
                options.ClaimsIdentity.RoleClaimType = Claims.Role;
            });

            services.AddOpenIddict()

                // Register the OpenIddict core components.
                .AddCore(options =>
                {
                    // Configure OpenIddict to use the Entity Framework Core stores and models.
                    // Note: call ReplaceDefaultEntities() to replace the default entities.
                    options.UseEntityFrameworkCore()
                           .UseDbContext<ApplicationDbContext>();
                })

                // Register the OpenIddict server components.
                .AddServer(options =>
                {
                    // Enable the token endpoint.
                    options.SetTokenEndpointUris("/connect/token");
                    //options.DisableAccessTokenEncryption();

                    // Enable the password and client credentials flow.
                    options
                        .AllowPasswordFlow()
                        .AllowClientCredentialsFlow();

                    // Register the signing and encryption credentials.
                    options.AddDevelopmentEncryptionCertificate()
                          .AddDevelopmentSigningCertificate();

                    // Register the ASP.NET Core host and configure the ASP.NET Core options.
                    options.UseAspNetCore()
                          .EnableTokenEndpointPassthrough();
                })

                // Register the OpenIddict validation components.
                .AddValidation(options =>
                {
                    // Import the configuration from the local OpenIddict server instance.
                    options.UseLocalServer();

                    // Register the ASP.NET Core host.
                    options.UseAspNetCore();
                });
            #endregion


            services.AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddCookie()
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.Audience = Configuration["TokenValidationParameters:Audience"];

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = Configuration["TokenValidationParameters:Issuer"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration["TokenValidationParameters:IssuerSigningKey"])),
                        ValidateIssuerSigningKey = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                    };
                });

            services.Configure<IdentityOptions>(options =>
            {
                // Password settings
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
            });

            services.AddRazorPages();

            // Workers

            services.AddHostedService<Worker>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebApplication1 v1"));
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
            });
        }
    }
}
