using AzureFunctions.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;

namespace AzureFunctions.Services;

public class CosmosDbService
{
    private readonly CosmosClient _cosmosClient;
    private readonly Container _container;
    
    public CosmosDbService(string connectionString, string databaseName, string containerName)
    {
        _cosmosClient = new CosmosClient(connectionString);
        _container = _cosmosClient.GetContainer(databaseName, containerName);
    }
    
    public async Task AddOrderAsync(Order order)
    {
        await _container.CreateItemAsync(order, new PartitionKey(order.Id));
    }

    public async Task UpdateOrderAsync(Order order)
    {
        await _container.UpsertItemAsync(order, new PartitionKey(order.Id));
    }

    public async Task<IEnumerable<Order>> GetOrdersFromDateAsync(DateTime date)
    {
        var query = new QueryDefinition("SELECT * FROM c WHERE c._ts >= @startOfDay AND c._ts < @endOfDay")
            .WithParameter("@startOfDay", new DateTimeOffset(date).ToUnixTimeSeconds())
            .WithParameter("@endOfDay", new DateTimeOffset(date.AddDays(1)).ToUnixTimeSeconds());

        var iterator = _container.GetItemQueryIterator<Order>(query);
        var orders = new List<Order>();

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            orders.AddRange(response.ToList());
        }

        return orders;
    }
}