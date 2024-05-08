using Azure;
using Azure.Communication.Email;
using Azure.Messaging.ServiceBus;
using EmailProvider.Functions;
using EmailProvider.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
namespace EmailProvider.Services;

public class EmailService(EmailClient emailClient, ILogger<EmailService> logger)
{
    private readonly ILogger<EmailService> _logger = logger;
    private readonly EmailClient _emailClient = emailClient;


    public EmailRequest UnPackEmailRequest(ServiceBusReceivedMessage message)
    {
        try
        {
            var emailRequest = JsonConvert.DeserializeObject<EmailRequest>(message.Body.ToString());
            if (emailRequest != null)
            {
                return emailRequest;
            }

        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : EmailSender.UnPackEmailRequest() :: {ex.Message}");
        }
        return null!;
    }

    public bool SendEmail(EmailRequest emailRequest)
    {
        try
        {
            var result = _emailClient.Send(
                WaitUntil.Completed,

                senderAddress: Environment.GetEnvironmentVariable("SenderAddress"),
                recipientAddress: emailRequest.Recipient,
                subject: emailRequest.Subject,
                htmlContent: emailRequest.HtmlContent,
                plainTextContent: emailRequest.PlainTextContent

                );

            if (result.HasCompleted)
            {
                return true;
            }

        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : EmailSender.SendEmailAsync() :: {ex.Message}");
        }
        return false;

    }
}
