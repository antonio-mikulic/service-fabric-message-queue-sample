using System;
using System.Linq;
using System.Text;
using AutoMapper;
using FabricMQ.Broker.Controllers;
using FabricMQ.Broker.Database;
using FabricMQ.Broker.Identity.Managers;
using FabricMQ.Broker.Identity.Models;
using FabricMQ.Broker.Identity.Seeds;
using FabricMQ.Broker.Identity.Stores;
using FabricMQ.Broker.Interfaces.Controllers;
using FabricMQ.Broker.SignalR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Cors.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Swagger;

namespace FabricMQ.Broker
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private const string DefaultCorsPolicyName = "localhost";
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAutoMapper();

            // Configure ASP.NET Core fields
            services.AddIdentity<User, Role>()
                .AddUserManager<BrokerUserManager>()
                .AddRoleManager<BrokerRoleManager>()
                .AddDefaultTokenProviders();

            // Configure auth from settings
            services.AddAuthentication(opt =>
                {
                    opt.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(opt =>
                {
                    opt.Audience = Configuration["Tokens:Site"];
                    opt.ClaimsIssuer = Configuration["Tokens:Site"];
                    opt.TokenValidationParameters = new TokenValidationParameters
                    {
                        RequireSignedTokens = true,
                        ValidateIssuer = true,
                        ValidIssuer = Configuration["Tokens:Site"],
                        ValidateAudience = true,
                        ValidAudience = Configuration["Tokens:Site"],
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Tokens:Key"])),
                        ValidateLifetime = true
                    };
                    opt.SaveToken = true;
                });

            services.Configure<IdentityOptions>(options =>
            {
                // Default Password settings.
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 1;
            });

            // Register needed services
            services.AddSingleton<IReliableTableTimeoutConfiguration, ReliableTableTimeoutConfiguration>();
            services.AddSingleton<IDatabaseContext, BrokerDatabaseContext>();
            services.AddTransient<IUserStore<User>, UserStore>();
            services.AddTransient<IUserRoleStore<User>, UserStore>();
            services.AddTransient<IUserClaimStore<User>, UserStore>();
            services.AddTransient<IRoleStore<Role>, RoleStore>();

            services.AddScoped<IBrokerService, DummyBrokerService>();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddScoped<SeedAuth>();
            services.AddScoped<SeedDefaultMessageTypes>();
            services.AddSingleton<NotificationsHub>();

            services.AddSignalR();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddMvc().AddControllersAsServices();

            services.AddMvc(options =>
            {
                options.Filters.Add(new CorsAuthorizationFilterFactory(DefaultCorsPolicyName));
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            //Swagger - Enable this line and the related lines in Configure method to enable swagger UI
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Info { Title = "FabricMQ.Broker API", Version = "v1" });
                options.DocInclusionPredicate((docName, description) => true);
            });

            //Configure CORS for angular2 UI
            services.AddCors(options =>
            {
                options.AddPolicy(DefaultCorsPolicyName, builder =>
                {
                    //App:CorsOrigins in appsettings.json can contain more than one address with splitted by comma.
                    builder.WithOrigins(
                            // App:CorsOrigins in appsettings.json can contain more than one address separated by comma.
                            Configuration["App:CorsOrigins"]
                                .Split(",", StringSplitOptions.RemoveEmptyEntries)
                                .ToArray()
                        )
                        .SetIsOriginAllowedToAllowWildcardSubdomains()
                        .SetIsOriginAllowed((host) => true)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors(DefaultCorsPolicyName);

            app.UseAuthentication();

            app.UseSwagger();

            app.UseSignalR(route =>
            {
                route.MapHub<NotificationsHub>("/notifications");
            });


            app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "FabricMQ.Broker API V1"); });

            app.UseMvc();
        }
    }
}
