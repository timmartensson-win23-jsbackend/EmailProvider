using Azure.Communication.Email;
using Azure.Messaging.ServiceBus;
using EmailProvider.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace EmailProvider.Functions;

public class EmailSender(ILogger<EmailSender> logger, EmailClient emailClient, EmailService emailService)
{
    private readonly ILogger<EmailSender> _logger = logger;
    private readonly EmailClient _emailClient = emailClient;
    private readonly EmailService _emailService = emailService;

    [Function(nameof(EmailSender))]
    public async Task Run([ServiceBusTrigger("email_request", Connection = "ServiceBusConnection")] ServiceBusReceivedMessage message, ServiceBusMessageActions messageActions)
    {
        try
        {
            var emailRequest = _emailService.UnPackEmailRequest(message);
            if (emailRequest != null && !string.IsNullOrEmpty(emailRequest.Recipient))
            {
                if (_emailService.SendEmail(emailRequest))
                {
                    await messageActions.CompleteMessageAsync(message);
                }

            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : EmailSender.Run() :: {ex.Message}");
        }
    }


}
