using Resta.MVC.Services;

namespace Resta.MVC
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();


            //eklenen api çekimi json
            var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"];

            if (string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                throw new Exception("ApiSettings:BaseUrl tanýmlý deðil!");
            }

            builder.Services.AddHttpClient<ApiClient>(client =>
            {
                client.BaseAddress = new Uri(apiBaseUrl);
            });



            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapControllerRoute(
                name: "masa",
                pattern: "Masa/{id:int}",
                defaults: new { controller = "Masa", action = "Index" }
            );


            app.Run();
        }
    }
}
