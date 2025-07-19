// Konum: FlightBooker.Identity.API.Tests/IdentityApiFactory.cs
using FlightBooker.Identity.API;
using FlightBooker.Identity.API.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Testcontainers.MsSql;

namespace FlightBooker.Identity.API.Tests;

// Bu sınıf, testler boyunca SQL Server konteynerini ve API'yi yönetir.
public class IdentityApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    // SQL Server için bir Testcontainer nesnesi tanımlıyoruz.
    private readonly MsSqlContainer _dbContainer = new MsSqlBuilder().Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Orijinal ApplicationDbContext kaydını kaldırıyoruz.
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();

            // Test konteynerine bağlanacak yeni bir DbContext kaydı ekliyoruz.
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                // Bağlantı cümlesini çalışan geçici konteynerden alıyoruz.
                options.UseSqlServer(_dbContainer.GetConnectionString());
            });
        });
    }

    // Bu metot, testler başlamadan önce bir kez çalışır.
    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync(); // SQL Server konteynerini başlat.

        // Konteyner başladıktan sonra, veritabanı şemasını oluşturmak için
        // EF Core migration'larını otomatik olarak uygula.
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    // Bu metot, tüm testler bittikten sonra bir kez çalışır.
    public new async Task DisposeAsync()
    {
        await _dbContainer.StopAsync(); // SQL Server konteynerini durdur ve sil.
    }
}