using CRUDMailSender.SMTP;
using CRUDShared.Contracts;
using MassTransit;

namespace CRUDMailSender.Consumers
{
    public class BirthdayGreetingsConsumer : IConsumer<BirthdayContract>
    {
        private readonly ILogger<BirthdayGreetingsConsumer> _logger;
        private readonly MailSender _mailSender;

        public BirthdayGreetingsConsumer(
            ILogger<BirthdayGreetingsConsumer> logger,
            MailSender mailSender)
        {
            _logger = logger;
            _mailSender = mailSender;
        }

        public async Task Consume(ConsumeContext<BirthdayContract> context)
        {
            _logger.LogInformation("Sending email to {Email}", context.Message.Email);

            await _mailSender.SendEmailAsync(context.Message.Email, "Happy birthday!", $"Happy birthday {context.Message.Name}!");
        }
    }
}
