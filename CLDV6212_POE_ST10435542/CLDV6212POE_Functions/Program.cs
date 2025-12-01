using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Files.Shares;
using Azure.Storage.Queues;
using CLDV6212_POE_ST10435542.Models.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http.Features;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication(builder =>
    {
        builder.Services.Configure<FormOptions>(options =>
        {
            options.MultipartBodyLengthLimit = 50 * 1024 * 1024; // 50MB
        });
    })
    .ConfigureServices(services =>
    {
        string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException("AzureWebJobsStorage connection string not set.");

        services.AddSingleton(x => new BlobServiceClient(connectionString));
        services.AddSingleton(x => new QueueServiceClient(connectionString));
        services.AddSingleton(x => new TableServiceClient(connectionString));
        services.AddSingleton(x => new ShareClient(connectionString, "contracts"));
    })
    .Build();

host.Run();