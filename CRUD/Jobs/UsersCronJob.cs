using CRUDShared;
using CRUDShared.Contracts;
using MassTransit;
using Quartz;

namespace CRUDMailSender.Jobs
{
    public class UsersCronJob : IJob
    {
        private readonly MongoJobService _mongoJobService;
        private readonly UserRepository _users;
        private readonly IPublishEndpoint _publishEndpoint;

        public UsersCronJob(
            MongoJobService mongoJobService,
            UserRepository users,
            IPublishEndpoint publishEndpoint)
        {
            _mongoJobService = mongoJobService;
            _users = users;
            _publishEndpoint = publishEndpoint;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            Console.WriteLine("Executing Users Job...");

            var now = DateTime.Now;

            var users = (await _users.GetAllAsync())?
                .Where(x => x.BirthDate.Day == now.Day && x.BirthDate.Month == now.Month)
                .ToList();

            users?.ForEach(async x =>
                await _publishEndpoint.Publish(new BirthdayContract()
                {
                    CorrelationId = Guid.NewGuid(),
                    Email = x.Email,
                    Name = x.Name
                }));

            await _mongoJobService.InsertJobLog("Users");
        }
    }

}
