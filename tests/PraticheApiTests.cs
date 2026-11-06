using Castle.Core.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Newtonsoft.Json;
using sistema_gestione_pratiche;
using sistema_gestione_pratiche.Common;
using sistema_gestione_pratiche.DB.Contexts;
using sistema_gestione_pratiche.DB.Entities.AuthEntities;
using sistema_gestione_pratiche.DB.Interceptors;
using sistema_gestione_pratiche.Models;
using sistema_gestione_pratiche.Models.AuthModels;
using sistema_gestione_pratiche.Services;
using System.Configuration;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using tests.DB.InitialScripts;

namespace tests
{
    public class PraticheApiTests : IClassFixture<ApiFactory>
    {
        private readonly HttpClient httpClient;

        public PraticheApiTests(ApiFactory factory)
        {
            this.httpClient = factory.CreateClient();
        }

        public async Task<string> getToken()
        {
            using var httpClient = new HttpClient();
            //httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);
            var response = await this.httpClient.PostAsJsonAsync(
                $"/api/Auth/Login",
                new LoginModel()
                {
                    Username = "admin",
                    Password = "Password01!"
                }
                );
            string stringResponse = await response.Content.ReadAsStringAsync();
            var jsonResponse = await response.Content.ReadFromJsonAsync<UsersAndRolesModel>();
            var definition = new { Token = "" };
            var anonObject = JsonConvert.DeserializeAnonymousType(stringResponse, definition);
            return anonObject != null ? anonObject.Token : "";
        }

        [Fact]
        public async Task Login()
        {
            using var httpClient = new HttpClient();

            var response = await this.httpClient.PostAsJsonAsync(
                $"/api/Auth/Login",
                new LoginModel()
                {
                    Username = "admin",
                    Password = "Password01!"
                }
                );
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetPratiche()
        {
            string token = await this.getToken();
            this.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await this.httpClient.GetFromJsonAsync<List<PraticaModel>>(
                $"/api/Pratiche/GetPratiche"
                );
            Assert.NotNull(response);
        }


        [Fact]
        public async Task InsertPratica()
        {
            string token = await this.getToken();
            this.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await this.httpClient.PostAsJsonAsync<object>(
                $"/api/Pratiche/InsertPratica",
                new PraticaModel()
                {
                    ContentBytes = System.Text.Encoding.UTF8.GetBytes("ciaone"),
                    CreatorCF = "CF",
                    CreatorDateOfBirth = DateTime.UtcNow,
                    CreatorLastName = "last",
                    CreatorName = "name",
                    FileName = "filename",
                }
                );
            Assert.NotNull(response);
        }
    }

    public class ApiFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            string connectionString = "Server=localhost;Port=5431;Database=AuthDB_tests;UserId=postgres;Password=postgres;";

            builder.UseEnvironment("Testing");

            builder.ConfigureAppConfiguration((ctx, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:AuthDB_Postgres"] = connectionString,
                    ["ConnectionStrings:DbContext_Postgres"] = connectionString,

                    ["Serilog:WriteTo:0:Name"] = "Console",
                    ["Serilog:WriteTo:1:Name"] = "",
                    ["Serilog:WriteTo:2:Name"] = ""
                });
            });

            builder.ConfigureServices(services =>
            {
                var authDBdescriptor = services
                .SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AuthDBContext>));

                if (authDBdescriptor != null)
                    services.Remove(authDBdescriptor);

                services.AddDbContext<AuthDBContext>(options => options.UseNpgsql(connectionString)
                .AddInterceptors(new DBCommandInterceptor()));


                var dbDescriptor = services
                .SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<DBContext>));
                if (dbDescriptor != null)
                    services.Remove(dbDescriptor);

                services.AddDbContext<DBContext>(options => options.UseNpgsql(connectionString)
                  .AddInterceptors(new DBCommandInterceptor()));
                //services.AddScoped<AuthService>();

                var serviceProvider = services.BuildServiceProvider();
                using var scope = serviceProvider.CreateScope();
                DBContextDump.InitialDump(scope.ServiceProvider);
            });
        }
    }
}