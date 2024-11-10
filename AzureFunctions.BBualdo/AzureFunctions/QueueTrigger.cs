using System.Text;
using AzureFunctions.Helpers;
using AzureFunctions.Models;
using AzureFunctions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AzureFunctions;

public class QueueTrigger
{
    private readonly ILogger<QueueTrigger> _logger;
    private readonly CosmosDbService _cosmosDbService;
    private readonly BlobService _blobService;

    public QueueTrigger(ILogger<QueueTrigger> logger)
    {
        _logger = logger;
        var connectionString = Environment.GetEnvironmentVariable("CosmosDbConnectionString") ?? 
                               throw new ArgumentNullException();
        var databaseName = Environment.GetEnvironmentVariable("CosmosDbDatabaseName") ??
                           throw new ArgumentNullException();
        const string containerName = "Orders";
        _cosmosDbService = new CosmosDbService(connectionString, databaseName, containerName);

        var blobConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage") ??
                                   throw new ArgumentNullException();
        _blobService = new BlobService(blobConnectionString);
    }

    [Function(nameof(QueueTrigger))]
    public async Task Run([QueueTrigger("orders-queue", Connection = "AzureWebJobsStorage")] string message)
    {
        _logger.LogInformation("C# Queue Trigger function processed a queue message.");
        var order = JsonConvert.DeserializeObject<Order>(message);

        if (order is not null)
        {
            _logger.LogInformation($"OrderID: {order.Id}, Status: {order.Status}");
            order.Status = StatusOptions.PaymentComplete;

            var fileName = $"{order.Id}-confirmation.txt";
            var messageBuilder = new StringBuilder();
            messageBuilder.AppendLine("Dear Customer,");
            messageBuilder.AppendLine("We receiver your order and payment. We will deliver it as soon as possible!\n\n");
            messageBuilder.AppendLine($"Product: {order.Quantity}x {order.ProductName}");
            messageBuilder.AppendLine($"Price: {order.Price}$"); ;

            _logger.LogInformation("Sending order notification to Blob Storage");
            await _blobService.UploadAsync("order-confirm", fileName, messageBuilder.ToString());

            _logger.LogInformation("Updating order status in CosmosDB.");
            await _cosmosDbService.UpdateOrderAsync(order);
            
            _logger.LogInformation($"OrderID: {order.Id}, Status: {order.Status}");
        }
        else
        {
            _logger.LogError("Failed to process the queue message.");
        }
    }
}