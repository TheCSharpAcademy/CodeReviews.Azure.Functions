using System.Net.Mail;
using AzureFunctions.Helpers;
using FluentEmail.Core;
using FluentEmail.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AzureFunctions.Services;

public class EmailService
{
    private readonly EmailOptions _options;
    private readonly ILogger _logger;

    public EmailService(IOptions<EmailOptions> options, ILoggerFactory loggerFactory)
    {
        _options = options.Value;
        _logger = loggerFactory.CreateLogger<EmailService>();
    }

    public async Task SendEmailAsync(string toEmail, string subject, string message)
    {
        var sender = new SmtpSender(() =>
        {
            return new SmtpClient(_options.SmtpServer, _options.Port)
            {
                EnableSsl = _options.EnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network
            };
        });

        Email.DefaultSender = sender;

        var email = await Email
            .From(_options.Sender, _options.SenderName)
            .To(toEmail)
            .Subject(subject)
            .UsingTemplate(message, new { }, true)
            .SendAsync();
        
        if (!email.Successful) 
            _logger.LogError("Sending Email failed.");
    }
}