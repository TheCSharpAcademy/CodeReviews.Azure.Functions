using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedClassLibrary.Data;
using SharedClassLibrary.Models;
using System.Net;
using System.Net.Mail;

namespace InventoryUpdate
{
    public class InventoryFunction
    {
        private readonly ILogger _logger;
        private readonly AppDbContext _context;
        public InventoryFunction(ILoggerFactory loggerFactory, AppDbContext context)
        {
            _logger = loggerFactory.CreateLogger<InventoryFunction>();
            _context = context;
        }

        [Function("InventoryFunction")]
        public async Task Run([CosmosDBTrigger(
            databaseName: "OrdersDemo",
            containerName: "Orders",
            Connection = "CosmoLink",
            LeaseContainerName = "leases",
            CreateLeaseContainerIfNotExists = true)] IReadOnlyList<MyDocument> input)
        {
            if (input != null && input.Count > 0)
            {
                _logger.LogInformation("Documents modified: " + input.Count);
                _logger.LogInformation("First document Id: " + input[0].id);

                var order = await _context.Orders.FindAsync(input[0].id.Split("|")[1]);

                if (order.Status == order.PreviousStatus)
                {
                    foreach (var product in order.Products)
                    {
                        var productToUpdate = await _context.Products.FirstOrDefaultAsync(o => o.Id == product.Id);
                        productToUpdate.InStock = productToUpdate.InStock - product.Count;
                        _context.Update(productToUpdate);
                        await _context.SaveChangesAsync();
                    }
                }
                else
                {
                    order.PreviousStatus = order.Status;
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                    SendMail(order.CustomerEmail, order.Status);
                }
            }
        }

        private static void SendMail(string customerEmail, Status status)
        {
            string smtpHost = "127.0.0.1";
            int smtpPort = 25;
            string fromEmail = "coffeeShop@azuredevelopment.com";
            string toEmail = customerEmail;
            string subject = string.Empty;
            string body = string.Empty;

            switch (status)
            {
                case Status.PaymentComplete:
                    {
                        subject = "Payment Confirmation for Your Recent Order";
                        body = "We are pleased to inform you that we have received your payment for the recent order.";
                        break;
                    }
                case Status.OrderShipped:
                    {
                        subject = "Your Order Has Been Shipped!";
                        body = "Good news! Your order has been shipped and is on its way to you.\r\n\r\nShipping Details:\r\nTracking Number: [Tracking Number]\r\nCarrier: [Shipping Company]\r\nEstimated Delivery Date: [Estimated Delivery Date]\r\nYou can track your order using the link below:\r\n[Tracking Link]\r\n\r\nThank you for shopping with us! If you have any questions, feel free to reach out.";
                        break;
                    }
                case Status.ShipmentFulfilled:
                    {
                        subject = "Your Order Has Been Delivered!";
                        body = "We are happy to inform you that your order has been successfully delivered!\r\n\r\nWe hope everything arrived in perfect condition and that you are satisfied with your purchase.";
                        break;
                    }
            }


            string username = "coffeeShop@azuredevelopment.com";
            string password = "yourpassword";


            MailMessage mail = new MailMessage();
            mail.From = new MailAddress(fromEmail);
            mail.To.Add(toEmail);
            mail.Subject = subject;
            mail.Body = body;


            SmtpClient smtpClient = new SmtpClient(smtpHost, smtpPort);
            smtpClient.Credentials = new NetworkCredential(username, password);

            try
            {
                smtpClient.Send(mail);
                Console.WriteLine("Email sent successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
            }
        }
    }

    public class MyDocument
    {
        public string id { get; set; }
        public string Text { get; set; }
        public int Number { get; set; }
        public bool Boolean { get; set; }
    }
}
