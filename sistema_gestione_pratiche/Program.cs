using sistema_gestione_pratiche.Auth;
using sistema_gestione_pratiche.Common;
using sistema_gestione_pratiche.DB.Contexts;
using sistema_gestione_pratiche.DB.Entities.AuthEntities;
using sistema_gestione_pratiche.DB.InitialScripts;
using sistema_gestione_pratiche.DB.Interceptors;
using sistema_gestione_pratiche.Hubs;
using sistema_gestione_pratiche.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System;
using System.Text;
using System.Threading.Tasks;
using static sistema_gestione_pratiche.Auth.AuthPolicyManager;

namespace sistema_gestione_pratiche
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //general configuration
            var configuration = new ConfigurationBuilder().AddJsonFile($"appsettings.json")
                .AddEnvironmentVariables().Build();

            //serilog
            Log.Logger = new LoggerConfiguration()
                 .ReadFrom.Configuration(configuration)
                 .Enrich.FromLogContext()
                 .CreateLogger();

            var builder = WebApplication.CreateBuilder(args);

            //serilog
            builder.Host.UseSerilog();

            // Add services to the container.

            builder.Services.AddControllers();

            /*
                                                User Management
             */
            //sql server
            //builder.Services.AddDbContext<AuthDBContext>(options => options.UseSqlServer(configuration.GetConnectionString("AuthDB_SQLServer"))
            //.AddInterceptors(new DBCommandInterceptor()));

            //postgres
            builder.Services.AddDbContext<AuthDBContext>(options => options.UseNpgsql(configuration.GetConnectionString("AuthDB_Postgres"))
            .AddInterceptors(new DBCommandInterceptor()));


            builder.Services.AddIdentity<User, Role>(
                options =>
                {
                    /*questo serve perché quando si criptano i nomi possono comparire dei caratteri non standard,
                     e mettendo null si ignora il controllo sui caratteri*/
                    options.User.AllowedUserNameCharacters = null;
                }
                )
                .AddEntityFrameworkStores<AuthDBContext>()
                .AddDefaultTokenProviders();
            builder.Services.AddScoped<SignInManager<User>>();
            builder.Services.AddScoped<UserManager<User>>();
            builder.Services.AddScoped<RoleManager<Role>>();

            /*
             //questo da mettere prima di .AddJwtBearer se si vuole usare uno scheme custom..
             builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = ConstantValues.Auth.JWTScheme;
                options.DefaultChallengeScheme = ConstantValues.Auth.JWTScheme;
                options.DefaultScheme = ConstantValues.Auth.JWTScheme;
            })
                .AddScheme<AuthenticationSchemeOptions, AuthSchema>("Schema",
            (options) => { })
             */

            TokenValidationParameters tokenValidationParameters = new TokenValidationParameters
            {
                ValidIssuer = builder.Configuration.GetSection(ConstantValues.Auth.AuthSettings).GetSection(ConstantValues.Auth.JWT).GetSection(ConstantValues.Auth.JWTIssuer).Value,
                ValidAudience = builder.Configuration.GetSection(ConstantValues.Auth.AuthSettings).GetSection(ConstantValues.Auth.JWT).GetSection(ConstantValues.Auth.JWTAudience).Value,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                        builder.Configuration
                        .GetSection(ConstantValues.Auth.AuthSettings)
                        .GetSection(ConstantValues.Auth.JWT)
                        .GetSection(ConstantValues.Auth.JWTSigningKey).Value)
                    ),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                LifetimeValidator = (DateTime? notBefore, DateTime? expires, SecurityToken securityToken, TokenValidationParameters validationParameters) =>
                {
                    if (validationParameters.ValidateLifetime)
                    {
                        if (expires > DateTime.UtcNow)
                            return true;
                        else
                            return false;
                    }
                    return true;
                },
                ValidateIssuerSigningKey = true,
                TokenDecryptionKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                        builder.Configuration
                        .GetSection(ConstantValues.Auth.AuthSettings)
                        .GetSection(ConstantValues.Auth.JWT)
                        .GetSection(ConstantValues.Auth.JWTEncryptionKey).Value)
                    ),
            };
            ConstantValues.Auth.TokenValidationParameters = tokenValidationParameters;

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(o =>
            {
                o.TokenValidationParameters = tokenValidationParameters;
                o.Events = new JwtBearerEvents()
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];

                        // If the request is for our hub...
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) &&
                            (path.StartsWithSegments("/api/signalRHub")))
                        {
                            // Read the token out of the query string
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            builder.Services.AddAuthorization(options =>
            {
                foreach (var property in typeof(ConstantValues.Auth.Claims.Types).GetFields())
                    options.AddPolicy(property.Name, policy => policy.Requirements.Add(new AuthPolicyRequirement(property.Name)));
            });
            builder.Services.AddSingleton<IAuthorizationHandler, AuthPolicyHandler>();


            /*
                                               Application entities
            */
            //sql server
            //builder.Services.AddDbContext<DBContext>(options => options.UseSqlServer(configuration.GetConnectionString("DbContext_SQLServer"))
            //.AddInterceptors(new DBCommandInterceptor()));

            //postgres
            builder.Services.AddDbContext<DBContext>(options => options.UseNpgsql(configuration.GetConnectionString("DbContext_Postgres"))
            .AddInterceptors(new DBCommandInterceptor()));

            //AESEncryptionHelper.DefaultKey = configuration.GetSection(ConstantValues.StringCypher.Cypher).GetSection(ConstantValues.StringCypher.Key).Value;
            //DBContextDump.InitialDump(services.BuildServiceProvider());

            builder.Services.AddScoped<AuthService>();
            builder.Services.AddScoped<SignalRService>();
            builder.Services.AddScoped<NotificationsService>();
            builder.Services.AddCors(o => o.AddPolicy("Cors", builder =>
            {
                builder.AllowAnyHeader()
                .AllowAnyMethod()
                .SetIsOriginAllowed(_ => true)
                .AllowCredentials();
            }));

            builder.Services.AddSignalR();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            //builder.Services.AddSwaggerGen();
            builder.Services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            try
            {
                //AESEncryptionHelper.DefaultKey = configuration.GetSection(ConstantValues.StringCypher.Cypher).GetSection(ConstantValues.StringCypher.Key).Value;
                DBContextDump.InitialDump(builder.Services.BuildServiceProvider());
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, string.Empty);
            }

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
#if DEBUG
                app.UseSwagger();
                app.UseSwaggerUI();
#endif
            }
            app.UseCors("Cors");
            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseMiddleware<AuthMiddleware>();

            app.MapControllers();

            app.MapHub<SignalRHub>("api/signalRHub");

            app.Run();
        }
    }
}
