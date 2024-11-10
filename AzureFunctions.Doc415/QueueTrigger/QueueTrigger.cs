using System;
using Azure.Storage.Queues.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using QueueTrigger.SignalHub;

namespace QueueTrigger
{
    internal class QueueTrigger
    {
        private readonly ILogger<QueueTrigger> _logger;
        private readonly IHubContext<NotificationHub> _hubContext;
        public QueueTrigger(ILogger<QueueTrigger> logger, IHubContext<NotificationHub> hubContext)
        {
            _logger = logger;
            _hubContext = hubContext;

        }

        [Function(nameof(QueueTrigger))]
        public async Task Run([QueueTrigger("orders-queue", Connection = "DevelopmentString")] QueueMessage message)
        {
            _logger.LogInformation($"C# Queue trigger function processed: {message.MessageText}");
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", message.MessageText);

            // buraya e mail eklenecek message da ki order id
        }
    }
}
