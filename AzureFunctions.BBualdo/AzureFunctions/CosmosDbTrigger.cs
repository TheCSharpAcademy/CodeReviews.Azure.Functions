using AzureFunctions.Helpers;
using AzureFunctions.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AzureFunctions;

public class CosmosDbTrigger
{
    private readonly ILogger _logger;

    public CosmosDbTrigger(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<CosmosDbTrigger>();
    }

    [Function("CosmosDbTrigger")]
    public void Run([CosmosDBTrigger(
            databaseName: "AzureFunctionsDb",
            containerName: "Orders",
            Connection = "CosmosDbConnectionString",
            LeaseContainerName = "leases")]
        IReadOnlyList<Order> input, FunctionContext context)
    {
        if (input is { Count: > 0 })
        {
            foreach (var order in input)
            {
                _logger.LogInformation($"Processing order {order.Id} with status {order.Status}");
                if (order.Status == StatusOptions.OrderShipped)
                {
                    _logger.LogInformation($"Updating inventory for order {order.Id}...");
                    // Updating inventory
                }
            }
        }

        
    }
}