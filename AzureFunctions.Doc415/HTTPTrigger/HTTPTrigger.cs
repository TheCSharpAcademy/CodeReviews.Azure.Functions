using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using HttpTrigger.Data;
using HttpTrigger.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace HTTPTrigger;
internal class HTTPTrigger
{
    private readonly ILogger<HTTPTrigger> _logger;
    private AppDbContext _context;

    public HTTPTrigger(ILogger<HTTPTrigger> logger, AppDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    [Function("orders")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", "put", "delete","get", Route = "orders/{id?}")]
         HttpRequest req,
         string id
       )
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        string dateQuery = req.Query["date"];
        DateTime? date = null;
        if (!string.IsNullOrEmpty(dateQuery) && DateTime.TryParse(dateQuery, out var parsedDate))
        {
            date = parsedDate;
        }


        if (req.Method == HttpMethods.Get)
        {
            return await GetOrders(date);
        }
        else if (req.Method == HttpMethods.Post)
        {
            return await CreateOrder(req);
        }
        else if (req.Method == HttpMethods.Put)
        {
            return await UpdateOrder(req, id);
        }
        else if (req.Method == HttpMethods.Delete)
        {
            return await DeleteOrder(id);
        }

        return new BadRequestResult();

    }

    private async Task<IActionResult> GetOrders(DateTime? date)
    {
        if (date is null)
        {
            var orders = await _context.Orders.ToListAsync();
            return new OkObjectResult(orders);
        }
        else
        {
            DateTime previousDayStart = date.Value.AddDays(-1).Date;
            DateTime previousDayEnd = previousDayStart.AddDays(1).AddSeconds(-1);
            var orders = await _context.Orders
                .Where(x => x.CreateDate >= previousDayStart && x.CreateDate <= previousDayEnd)
                .ToListAsync();
            return new OkObjectResult(orders);
        }
    }

    private async Task<IActionResult> GetOrder(string id)
    {
        var order = await _context.Orders.FindAsync(id);
        return new OkObjectResult(order);
    }



    private async Task<IActionResult> CreateOrder(HttpRequest req)
    {
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        Order order = JsonConvert.DeserializeObject<Order>(requestBody);
        order.Id = Guid.NewGuid().ToString();

        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();

        await QueueMessage($"New order placed by-{order.CustomerEmail}");
        SendInvoice(order);

        return new CreatedResult($"/orders/{order.Id}", order);
    }

    private void SendInvoice(Order order)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Invoice for Order {order.Id}");
        sb.AppendLine($"Customer: {order.CustomerEmail}");
        sb.AppendLine($"Order Date: {DateTime.Now}");
        sb.AppendLine("==================================");
        sb.AppendLine("Items:");
        foreach (var item in order.Products)
        {
            var pricePerProduct = item.Price * item.Count;
            sb.AppendLine($"{item.Name} - Quantity: {item.Count} - Price: {item.Price:C} - Total: {pricePerProduct:C}");
        }
        sb.AppendLine("==================================");
        sb.AppendLine($"Total Amount: {order.TotalFee:C}");
        sb.AppendLine("Thank you for your order!");
        string filePath = $"invoice_{order.Id}.txt";
        File.WriteAllText(filePath, sb.ToString());
        UploadFileToLocalAzBlob(filePath);
    }

    private static async Task UploadFileToLocalAzBlob(string file)
    {
        string connectionString = "UseDevelopmentStorage=true";
        string containerName = "orders";
        string blobName = Path.GetFileName(file);
        try
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync();
            BlobClient blobClient = containerClient.GetBlobClient(blobName);
            using (FileStream uploadFileStream = File.OpenRead(file))
            {
                await blobClient.UploadAsync(uploadFileStream, true);
                Console.WriteLine($"Uploaded {blobName} to blob storage.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private async Task QueueMessage(string message)
    {
        QueueClient queue = new QueueClient("UseDevelopmentStorage=true", "myqueue-orders");
        try
        {
            await queue.CreateIfNotExistsAsync();
            await queue.SendMessageAsync(Convert.ToBase64String(Encoding.UTF8.GetBytes(message)));
        }
        catch (Exception ex)
        {
            _logger.LogInformation(ex.Message);
        }
    }

    private async Task<IActionResult> UpdateOrder(HttpRequest req, string id)
    {
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var orderToUpdate = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id);
        if (orderToUpdate is null)
        {
            return new NotFoundResult();
        }

        try
        {
            var order = JsonSerializer.Deserialize<Order>(requestBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            if (order.Id == id)
            {
                orderToUpdate.Status = order.Status;
                _context.Update(orderToUpdate);
                await _context.SaveChangesAsync();
            }

            return new OkObjectResult(orderToUpdate);
        }
        catch
        {
            return new BadRequestResult();
        }
    }

    private async Task<IActionResult> DeleteOrder(string id)
    {
        var orderId = "Order|" + id;
        var order = await _context.Orders.FindAsync(orderId);
        if (order is null)
        {
            return new NotFoundResult();
        }

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();
        return new NoContentResult();
    }
}
