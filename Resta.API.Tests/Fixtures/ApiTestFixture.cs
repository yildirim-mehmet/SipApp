using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Resta.API.Data;
using System.Linq;

namespace Resta.API.Tests.Fixtures;

public class ApiTestFixture : WebApplicationFactory<Program>
{
    public HttpClient Client { get; }

    public ApiTestFixture()
    {
        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Mevcut DbContext'i kaldır
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<RestaContext>));
                    if (descriptor != null)
                        services.Remove(descriptor);

                    // InMemory DB (HER TESTTE TEMİZ)
                    services.AddDbContext<RestaContext>(options =>
                    {
                        options.UseInMemoryDatabase("TestDb_" + Guid.NewGuid());
                    });
                });
            });

        Client = factory.CreateClient();
    }


    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Mevcut DbContext'i kaldır
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<RestaContext>));

            if (descriptor != null)
                services.Remove(descriptor);

            // InMemory DB ekle
            services.AddDbContext<RestaContext>(options =>
            {
                options.UseInMemoryDatabase("Resta_Test_Db");
            });
        });
    }
}
