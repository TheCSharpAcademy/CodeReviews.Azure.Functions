using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SharedClassLibrary.Data;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((context,services) =>
    {
        var configuration = context.Configuration;
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseCosmos(
                configuration["CosmoLink:AccountEndpoint"],
                configuration["CosmoLink:AccountKey"],
                configuration["CosmoLink:DatabaseName"]
            );
        });
    
    })
    .Build();

host.Run();
