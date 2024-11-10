using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using TimerFunction.Models;

namespace TimerFunction
{
    public class TimerFunction
    {
        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;

        public TimerFunction(ILoggerFactory loggerFactory, HttpClient httpClient)
        {
            _logger = loggerFactory.CreateLogger<TimerFunction>();
            _httpClient = httpClient;
        }

        [Function("Function1")]
        public async Task Run([TimerTrigger("* * * * *")] TimerInfo myTimer)  // Triggers every minute for test purposes change to "0 0 * * *" for daily trigger
        {
            _logger.LogInformation($"Daily sales: {DateTime.Now}");

            if (myTimer.ScheduleStatus is not null)
            {
                _logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");
            }

            try
            {
                await QueryOrdersAsync(DateTime.Now);

            }
            catch
            {
                _logger.LogInformation("error getting records");
            }
        }

        public async Task QueryOrdersAsync(DateTime date)
        {
            try
            {
                string dateString = date.Date.ToString("yyyy-MM-dd");
                string requestUrl = $"http://localhost:7029/api/orders?date={dateString}";

                HttpResponseMessage response = await _httpClient.GetAsync(requestUrl);
                response.EnsureSuccessStatusCode();

                string result = await response.Content.ReadAsStringAsync();
                var orders = JsonSerializer.Deserialize<List<Order>>(result, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                await GenerateDailyReport(orders, date);
            }
            catch (HttpRequestException httpEx)
            {
                Console.WriteLine($"HTTP error: {httpEx.Message}");
            }
            catch (JsonException jsonEx)
            {
                Console.WriteLine($"JSON parsing error: {jsonEx.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        internal async Task GenerateDailyReport(ICollection<Order> orders, DateTime date)
        {
            string filename = date.ToString().Split(' ')[0];
            var productSummary = orders
            .SelectMany(order => order.Products)
            .GroupBy(product => product.Name)
            .Select(g => new
            {
                ProductName = g.Key,
                TotalQuantity = g.Sum(p => p.Count),
                TotalCost = g.Sum(p => p.Count * p.Price)
            })
            .ToList();

            var total = 0m;

            var sb = new StringBuilder();
            sb.AppendLine($"Daily sales report for: {date}");
            sb.AppendLine("======================================================");
            foreach (var item in productSummary)
            {
                sb.AppendLine($"Product: {item.ProductName}, Total Quantity: {item.TotalQuantity}, Total Cost: {item.TotalCost:C}");
                total += item.TotalCost;
            }
            sb.AppendLine("======================================================");
            sb.AppendLine($"Total Amount: {total:C}");
            string filePath = $"salesReport_{filename}.txt";
            File.WriteAllText(filePath, sb.ToString());

            await UploadFileToLocalAzBlob(filePath);

        }

        private static async Task UploadFileToLocalAzBlob(string file)
        {
            string connectionString = "UseDevelopmentStorage=true";
            string containerName = "dailyreports";
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
    }
}
