
using Resta.API.Data;
using Microsoft.EntityFrameworkCore;
using Resta.API.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<RestaContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ✅ SignalR
builder.Services.AddSignalR();

// ✅ CORS (MVC origin'ini izinli yap)
builder.Services.AddCors(options =>
{
    options.AddPolicy("mvc", policy =>
    {
        policy.WithOrigins("https://localhost:7231") // MVC'nin portu
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ✅ CORS mutlaka MapHub’dan önce
app.UseCors("mvc");

app.UseAuthorization();

app.MapControllers();

// ✅ Hub endpoint
app.MapHub<SiparisHub>("/hubs/siparis");

app.Run();



//using Resta.API.Data;
//using Microsoft.EntityFrameworkCore;
//using Resta.API.Hubs;



//namespace Resta.API
//{
//    public class Program
//    {
//        public static void Main(string[] args)
//        {
//            var builder = WebApplication.CreateBuilder(args);

//            // Add services to the container.

//            builder.Services.AddControllers();
//            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//            builder.Services.AddEndpointsApiExplorer();
//            builder.Services.AddSwaggerGen();

//            // DbContext
//            builder.Services.AddDbContext<RestaContext>(options =>
//                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//            // Add SignalR
//            builder.Services.AddSignalR();



//            var app = builder.Build();

//            // Add SignalR            
//            app.MapHub<SiparisHub>("/hubs/siparis");


//            // Add SignalR bit

//            // Configure the HTTP request pipeline.
//            if (app.Environment.IsDevelopment())
//            {
//                app.UseSwagger();
//                app.UseSwaggerUI();
//            }

//            app.UseHttpsRedirection();

//            app.UseAuthorization();


//            app.MapControllers();

//            app.Run();
//        }
//    }
//}
