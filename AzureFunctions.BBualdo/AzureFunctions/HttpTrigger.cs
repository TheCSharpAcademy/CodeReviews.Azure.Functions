using System.Net;
using System.Text;
using Azure.Storage.Queues;
using AzureFunctions.Helpers;
using AzureFunctions.Models;
using AzureFunctions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AzureFunctions;

public class HttpTrigger
{
    private readonly CosmosDbService _cosmosDbService;
    private readonly QueueClient _queueClient;
    private readonly ILogger _logger;

    public HttpTrigger(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<HttpTrigger>();
        var connectionString = Environment.GetEnvironmentVariable("CosmosDbConnectionString") ?? 
                               throw new ArgumentNullException();
        var databaseName = Environment.GetEnvironmentVariable("CosmosDbDatabaseName") ??
                           throw new ArgumentNullException();
        const string containerName = "Orders";
        _cosmosDbService = new CosmosDbService(connectionString, databaseName, containerName);

        var queueConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage") ??
                                    throw new ArgumentNullException();
        _queueClient = new QueueClient(queueConnectionString, "orders-queue");
        _queueClient.CreateIfNotExists();
    }

    [Function("HttpTrigger")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "POST")] HttpRequestData req,
        FunctionContext executionContext)
    {
        _logger.LogInformation("C# HTTP Trigger function processed a request.");

        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var order = JsonConvert.DeserializeObject<Order>(requestBody);

        if (order is null)
        {
            _logger.LogError("Order processing failed. Order has insufficient information.");
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }
        
        order.Id = Guid.NewGuid().ToString();
        order.Status = StatusOptions.OrderPlaced;
        
        _logger.LogInformation("Saving order to CosmosDB.");
        await _cosmosDbService.AddOrderAsync(order);

        var queueMessage = JsonConvert.SerializeObject(order);
        _logger.LogInformation("Sending Queue Message.");
        var bytes = Encoding.UTF8.GetBytes(queueMessage);
        await _queueClient.SendMessageAsync(Convert.ToBase64String(bytes));

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "application/json; charset=utf-8");
        response.WriteString("Order received and is being processed.");

        return response;
    }
}