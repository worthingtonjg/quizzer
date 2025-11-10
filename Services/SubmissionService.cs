using Azure.Data.Tables;
using quizzer.Models;

namespace quizzer.Services
{
    public class SubmissionService
    {
        private readonly TableClient _table;

        public SubmissionService(IConfiguration config)
        {
            var connStr = config.GetConnectionString("StorageAccount")
                          ?? Environment.GetEnvironmentVariable("StorageAccount");
            if (string.IsNullOrWhiteSpace(connStr))
                throw new InvalidOperationException("StorageAccount connection string is missing.");

            var serviceClient = new TableServiceClient(connStr);
            _table = serviceClient.GetTableClient("Submissions");
            _table.CreateIfNotExists();
        }

        public async Task<SubmissionEntity?> GetAsync(string testId, string accessCode)
        {
            try
            {
                var response = await _table.GetEntityAsync<SubmissionEntity>(testId, accessCode);
                return response.Value;
            }
            catch (Azure.RequestFailedException)
            {
                return null;
            }
        }

        public async Task<IEnumerable<SubmissionEntity>> GetByTestAsync(string testId)
        {
            var results = new List<SubmissionEntity>();
            await foreach (var item in _table.QueryAsync<SubmissionEntity>(s => s.PartitionKey == testId))
                results.Add(item);
            return results;
        }

        public async Task AddAsync(SubmissionEntity entity) =>
            await _table.AddEntityAsync(entity);

        public async Task UpdateAsync(SubmissionEntity entity) =>
            await _table.UpdateEntityAsync(entity, entity.ETag, TableUpdateMode.Replace);

        public async Task DeleteAsync(string testId, string accessCode) =>
            await _table.DeleteEntityAsync(testId, accessCode);
    }
}
