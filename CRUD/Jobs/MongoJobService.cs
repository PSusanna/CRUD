using CRUDShared.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace CRUDMailSender.Jobs
{
    public class MongoJobService
    {
        private readonly IMongoCollection<BsonDocument> _jobCollection;
        private readonly MongoDbSettings _settings;

        public MongoJobService(IOptions<MongoDbSettings> options)
        {
            _settings = options.Value;

            var client = new MongoClient(_settings.ConnectionString);
            var database = client.GetDatabase(_settings.DatabaseName);
            _jobCollection = database.GetCollection<BsonDocument>("Jobs");
        }

        public async Task InsertJobLog(string jobName)
        {
            var jobLog = new BsonDocument
            {
                { "JobName", jobName },
                { "ExecutedAt", DateTime.UtcNow }
            };

            await _jobCollection.InsertOneAsync(jobLog);
            Console.WriteLine($"Logged job execution to MongoDB: {jobName}");
        }
    }
}
