// Default URL for triggering event grid function in the local environment.
// http://localhost:7074/runtime/webhooks/EventGrid?functionName={functionname}

using System.Text;
using Azure.Messaging.EventGrid;
using AzureFunctions.Models;
using AzureFunctions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AzureFunctions;

public class EventGridTrigger
{
    private readonly ILogger<EventGridTrigger> _logger;
    private readonly EmailService _emailService;
    
    public EventGridTrigger(ILogger<EventGridTrigger> logger, EmailService emailService)
    {
        _logger = logger;
        _emailService = emailService;
    }

    [Function("EventGridTrigger")]
    public async Task Run([EventGridTrigger] EventGridEvent eventGridEvent)
    {
        _logger.LogInformation("Event Grid trigger function processed an event.");
        
        var order = eventGridEvent.Data.ToObjectFromJson<Order>();
        
        _logger.LogInformation($"Order {order.Id} status changed to {order.Status}");

        var customerEmail = "customer@example.com";
        var subject = $"Order changes status to: {order.Status}";
        var messageBuilder = new StringBuilder();
        messageBuilder.AppendLine("Dear Customer,");
        messageBuilder.AppendLine($"Your order changed status for {order.Status}.");
        
        await _emailService.SendEmailAsync(customerEmail, subject, messageBuilder.ToString());
        _logger.LogInformation("Notified customer about changing status.");
    }
}