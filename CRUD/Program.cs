using CRUDMailSender.Jobs;
using CRUDShared;
using CRUDShared.Entities;
using CRUDShared.Settings;
using MassTransit;
using Quartz;

namespace CRUD
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDbSettings"));
            builder.Services.Configure<RabbitSettings>(builder.Configuration.GetSection("RabbitSettings"));
            builder.Services.AddOptions<MongoDbSettings>();
            builder.Services.AddSingleton<UserRepository>();
			
			builder.Services.AddCors(options =>
			{
				options.AddPolicy("AllowAll", policy =>
				{
					policy.AllowAnyOrigin()
						  .AllowAnyHeader()
						  .AllowAnyMethod();
				});
			});

            builder.Services.AddMassTransit(config =>
            {
                var host = (string)builder.Configuration.GetSection("RabbitSettings").GetValue(typeof(string), "Host")!;
                var username = (string)builder.Configuration.GetSection("RabbitSettings").GetValue(typeof(string), "Username")!;
                var password = (string)builder.Configuration.GetSection("RabbitSettings").GetValue(typeof(string), "Password")!;

                config.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(host, h =>
                    {
                        h.Username(username);
                        h.Password(password);
                        h.RequestedConnectionTimeout(15000);
                    });
                });
            });

            builder.Services.AddSingleton<MongoJobService>();

            builder.Services.AddQuartz(q =>
            {
                var jobKey = new JobKey("MongoJob");

                q.AddJob<UsersCronJob>(opts => opts.WithIdentity(jobKey));
                q.AddTrigger(opts => opts
                    .ForJob(jobKey)
                    .WithIdentity("JobTrigger")
                    .WithCronSchedule((string)builder.Configuration.GetSection("CronJobs").GetValue(typeof(string), "UsersJob")!));
            });

            builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

            var app = builder.Build();
			
			app.UseCors("AllowAll");

            // Minimal API Endpoints
            app.MapGet("/users", async (UserRepository repository) => Results.Ok(await repository.GetAllAsync()));
            app.MapGet("/users/{id}", async (int id, UserRepository repository) =>
                await repository.GetByIdAsync(id) is User user ? Results.Ok(user) : Results.NotFound());
            app.MapPost("/users", async (User user, UserRepository repository) =>
            {
                await repository.CreateAsync(user);
                return Results.Created($"/users/{user.Id}", user);
            });
            app.MapPut("/users/{id}", async (int id, User user, UserRepository repository) =>
            {
                await repository.UpdateAsync(id, user);
                return Results.NoContent();
            });
            app.MapDelete("/users/{id}", async (int id, UserRepository repository) =>
            {
                await repository.DeleteAsync(id);
                return Results.NoContent();
            });

            app.Run();
        }
    }
}
