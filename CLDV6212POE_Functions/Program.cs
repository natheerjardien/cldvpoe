using CLDV6212_POE_ST10435542.Models.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

var config = builder.Configuration;

builder.Services.AddSingleton<AzureFileShareService>(sp =>
{
    var connectionString = config["AzureWebJobsStorage"];
    var fileShareName = config["AzureFileShareName"];

    if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(fileShareName))
        throw new InvalidOperationException("Azure File Share configuration missing.");

    return new AzureFileShareService(connectionString, fileShareName);
});

builder.Build().Run();
