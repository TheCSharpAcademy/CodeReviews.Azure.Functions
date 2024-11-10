using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace QueueFunction
{
    public class QueueFunction
    {
        private readonly ILogger<QueueFunction> _logger;
        public QueueFunction(ILogger<QueueFunction> logger)
        {
            _logger = logger;
        }

        [Function(nameof(QueueFunction))]
        public void Run([QueueTrigger("myqueue-orders", Connection = "AzureWebJobsStorage")] string message)
        {
            try
            {
                _logger.LogInformation($"C# Queue trigger function processed: {message}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing queue message: {ex.Message}, StackTrace: {ex.StackTrace}");
                throw;
            }
        }
    }
}
