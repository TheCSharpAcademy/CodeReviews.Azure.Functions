using System.IO;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Reflection.PortableExecutable;
using System.Reflection.Metadata;

namespace BlobFunction;

public class Function1
{
    [Function(nameof(BlobFunction))]
    public static void Run(
    [BlobTrigger("orders/{name}")] string myTriggerItem,
    [BlobInput("orders/{name}")] string myBlob,
    FunctionContext context)
    {
        var logger = context.GetLogger("BlobFunction");
        logger.LogInformation("Triggered Item = {myTriggerItem}", myTriggerItem);
        logger.LogInformation("Input Item = {myBlob}", myBlob);

        SendMail(myBlob);        

    }

    private static void SendMail(string invoice)
    {
        string customerEmail="";

        var lines = invoice.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        customerEmail = lines[1].Substring(10);
       
        string smtpHost = "127.0.0.1";
        int smtpPort = 25; 

        string fromEmail = "coffeeShop@azuredevelopment.com";
        string toEmail = customerEmail;
        string subject = "Your invoice";
        string body = invoice;

       
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
