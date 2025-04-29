using CRUDMailSender.Consumers;
using CRUDMailSender.SMTP;
using CRUDShared.Settings;
using MassTransit;

namespace CRUDMailSender
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.Configure<RabbitSettings>(builder.Configuration.GetSection("RabbitSettings"));
            builder.Services.Configure<SMTPConfig>(builder.Configuration.GetSection("SMTPConfig"));
            builder.Services.AddSingleton<MailSender>();

            builder.Services.AddMassTransit(config =>
            {
                var host = (string)builder.Configuration.GetSection("RabbitSettings").GetValue(typeof(string), "Host")!;
                var username = (string)builder.Configuration.GetSection("RabbitSettings").GetValue(typeof(string), "Username")!;
                var password = (string)builder.Configuration.GetSection("RabbitSettings").GetValue(typeof(string), "Password")!;

                config.AddConsumer<BirthdayGreetingsConsumer>();

                config.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(host, h =>
                    {
                        h.Username(username);
                        h.Password(password);
                        h.RequestedConnectionTimeout(15000);
                    });

                    cfg.ReceiveEndpoint("birthday-greetings-queue", e =>
                    {
                        e.ConfigureConsumer<BirthdayGreetingsConsumer>(context);
                    });
                });
            });

            var app = builder.Build();

            app.Run();
        }
    }
}
