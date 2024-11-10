using Azure.Storage.Blobs;
using AzureFunctions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AzureFunctions;

public class BlobTrigger
{
    private readonly ILogger<BlobTrigger> _logger;
    private readonly EmailService _emailService;

    public BlobTrigger(ILogger<BlobTrigger> logger, EmailService emailService)
    {
        _logger = logger;
        _emailService = emailService;
    }

    [Function(nameof(BlobTrigger))]
    public async Task Run([BlobTrigger("order-confirm/{name}", Connection = "AzureWebJobsStorage")] BlobClient blobClient, string name)
    {
        _logger.LogInformation($"C# Blob Trigger function processed blob\n Name: {name}");

        var blobContent = await blobClient.DownloadContentAsync();
        var blobText = blobContent.Value.Content.ToString();

        var customerEmail = "customer@example.com";
        var subject = "Your order has been placed.";
        var message = blobText;

        await _emailService.SendEmailAsync(customerEmail, subject, message);
        
        _logger.LogInformation("Email sent to customer");
    }
}