using CLDV6212_POE_ST10435542.Models.Services;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using CLDV6212_POE_ST10435542.Models.Services;
using CLDV6212_POE_ST10435542.Controllers;
using Microsoft.Extensions.Configuration;
using CLDV6212_POE_ST10435542.Models.Data;

namespace CLDV6212_POE_ST10435542
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var configuration = builder.Configuration;

            var connectionString = builder.Configuration.GetConnectionString("AzureSqlConnection");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddAuthentication("LoginCookie")
                .AddCookie("LoginCookie", options =>
                {
                    options.Cookie.Name = "LoginCookie";
                    options.LoginPath = "/Account/Login";
                    options.AccessDeniedPath = "/Home/AccessDenied";
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                });

            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });


            builder.Services.AddControllersWithViews();

            builder.Services.AddHttpClient(); // for functions

// As demonstarted by IIEVC School of Computer Science (2025), all 4 services relating to Azure Storage must be configured in the Program file
// Further demonstarted by IIEVC School of Computer Science (2025), I registered the Blob service, as well as created a BlobService model
// I registered the services for Table, Blob, Queue, and File Share storage with the configuration settings from appsettings.json (connectionString)

            builder.Services.AddSingleton(new TableStorageService(configuration.GetConnectionString("AzureStorage")));

            builder.Services.AddSingleton(new BlobService(configuration.GetConnectionString("AzureStorage")));

            builder.Services.AddSingleton<QueueService>(sp =>
            {
                var connectionString = configuration.GetConnectionString("AzureStorage");
                return new QueueService(connectionString, "orders");
            });

            builder.Services.AddSingleton<AzureFileShareService>(sp =>
            {
                var connectionString = configuration.GetConnectionString("AzureStorage");
                return new AzureFileShareService(connectionString, "fileshare");
            });

            builder.WebHost.ConfigureKestrel(serverOptions =>
            {
                serverOptions.Limits.MaxRequestBodySize = 50 * 1024 * 1024; // 50 MB
            });

            builder.Services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 50 * 1024 * 1024; // 50 MB
            });

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Account}/{action=Login}/{id?}");

            app.Run();
        }
    }
}

/* References:

IIEVC School of Computer Science, 2025. CLDV6212 Building a Modern Web App with Azure Table Storage & ASP.NET Core MVC - Part 1. [video online] 
Available at: <https://youtu.be/Txp7VYUMBGQ?si=5sD7TV0vS90-pPHY>
[Accessed 15 August 2025].


IIEVC School of Computer Science, 2025. CLDV6212 ASP.NET MVC & Azure Series - Part 2: Adding Image Uploads with Blob Storage!. [video online] 
Available at: <https://youtu.be/CuszKqZvRuM?si=RZaHcDniR_ZWB-59>
[Accessed 16 August 2025].

*/

