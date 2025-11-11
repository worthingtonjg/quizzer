using Azure.Data.Tables;
using Microsoft.AspNetCore.Identity;
using quizzer.Models.YourAppNamespace.Models;

namespace quizzer.Services
{
    public class UserService
    {
        private readonly TableClient _table;
        private readonly PasswordHasher<UserEntity> _hasher = new();

        public UserService(IConfiguration config)
        {
            var connStr = config.GetConnectionString("StorageAccount")
                          ?? Environment.GetEnvironmentVariable("StorageAccount");

            if (string.IsNullOrWhiteSpace(connStr))
                throw new InvalidOperationException("StorageAccount connection string is missing.");

            var serviceClient = new TableServiceClient(connStr);
            _table = serviceClient.GetTableClient("Users");
            _table.CreateIfNotExists();
        }

        public async Task<UserEntity?> GetByEmailAsync(string email)
        {
            var results = _table.QueryAsync<UserEntity>(u => u.Email == email);
            await foreach (var user in results)
                return user;
            return null;
        }

        public async Task<bool> RegisterAsync(string name, string email, string password)
        {
            if (await GetByEmailAsync(email) != null)
                return false; // already exists

            var user = new UserEntity
            {
                PartitionKey = "Users",
                RowKey = Guid.NewGuid().ToString(),
                Name = name,
                Email = email,
                PasswordHash = _hasher.HashPassword(null!, password),
                Role = "Teacher",
                CreatedUtc = DateTime.UtcNow
            };

            await _table.AddEntityAsync(user);
            return true;
        }

        public async Task<UserEntity?> ValidateCredentialsAsync(string email, string password)
        {
            var user = await GetByEmailAsync(email);
            if (user == null) return null;

            var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, password);
            return result == PasswordVerificationResult.Success ? user : null;
        }
    }
}
