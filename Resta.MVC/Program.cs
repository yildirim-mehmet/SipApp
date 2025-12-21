using Resta.MVC.Services;
using Resta.MVC.Hubs;

namespace Resta.MVC;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // MVC
        builder.Services.AddControllersWithViews();
        builder.Services.AddSignalR();


        // API base url (appsettings.json -> ApiSettings:BaseUrl)
        var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"];
        if (string.IsNullOrWhiteSpace(apiBaseUrl))
            throw new Exception("ApiSettings:BaseUrl tanımlı değil! appsettings.json kontrol et.");

        builder.Services.AddHttpClient<ApiClient>(client =>
        {
            // Örnek: https://localhost:7286/api/
            client.BaseAddress = new Uri(apiBaseUrl);
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();
        app.UseAuthorization();

        // Daha spesifik route önce (Masa/{id})
        app.MapControllerRoute(
            name: "masa",
            pattern: "Masa/{id:int}",
            defaults: new { controller = "Masa", action = "Index" }
        );

        // Default route
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}"
        );

        app.MapHub<SiparisHub>("/hubs/siparis");


        app.Run();
    }
}
