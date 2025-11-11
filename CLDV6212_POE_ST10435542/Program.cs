using CLDV6212_POE_ST10435542.Models.Services;
using Microsoft.Extensions.Configuration;

namespace CLDV6212_POE_ST10435542
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var configuration = builder.Configuration;

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddHttpClient(); // for functions

// As demonstarted by IIEVC School of Computer Science (2025), all 4 services relating to Azure Storage must be configured in the Program file
// Further demonstarted by IIEVC School of Computer Science (2025), I registered the Blob service, as well as created a BlobService model
// I registered the services for Table, Blob, Queue, and File Share storage with the configuration settings from appsettings.json (connectionString)

            // Register table storage with configuration
            builder.Services.AddSingleton(new TableStorageService(configuration.GetConnectionString("AzureStorage")));

            // Register Blob storage with configuration
            builder.Services.AddSingleton(new BlobService(configuration.GetConnectionString("AzureStorage")));

            // Register the Queue storage service with configuration
            builder.Services.AddSingleton<QueueService>(sp =>
            {
                var connectionString = configuration.GetConnectionString("AzureStorage");
                return new QueueService(connectionString, "orders");
            });

            // Register the fileShareService with configuration
            builder.Services.AddSingleton<AzureFileShareService>(sp =>
            {
                var connectionString = configuration.GetConnectionString("AzureStorage");
                return new AzureFileShareService(connectionString, "fileshare");
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
                pattern: "{controller=Login}/{action=Index}/{id?}"); // set the default page to the Login page

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

