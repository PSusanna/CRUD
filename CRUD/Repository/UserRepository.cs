using CRUDShared.Entities;
using CRUDShared.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace CRUDShared
{
    public class UserRepository
    {
        private readonly IMongoCollection<User> _users;

        public UserRepository(IOptions<MongoDbSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            var database = client.GetDatabase(settings.Value.DatabaseName);
            _users = database.GetCollection<User>(settings.Value.UsersCollectionName);
        }

        public async Task<List<User>> GetAllAsync() => await _users.Find(user => true).ToListAsync();
        public async Task<User> GetByIdAsync(int id) => await _users.Find(user => user.Id == id).FirstOrDefaultAsync();
        public async Task CreateAsync(User user) => await _users.InsertOneAsync(user);
        public async Task UpdateAsync(int id, User user) => await _users.ReplaceOneAsync(u => u.Id == id, user);
        public async Task DeleteAsync(int id) => await _users.DeleteOneAsync(u => u.Id == id);
    }
}
