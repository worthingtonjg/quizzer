using Azure.Data.Tables;
using quizzer.Components.Pages;
using quizzer.Models;
using System.CodeDom.Compiler;
using static System.Net.Mime.MediaTypeNames;

namespace quizzer.Services
{
    public class AccessCodeService
    {
        private readonly TableClient _table;

        public AccessCodeService(IConfiguration config)
        {
            var connStr = config.GetConnectionString("StorageAccount")
                          ?? Environment.GetEnvironmentVariable("StorageAccount");
            if (string.IsNullOrWhiteSpace(connStr))
                throw new InvalidOperationException("StorageAccount connection string is missing.");

            var serviceClient = new TableServiceClient(connStr);
            _table = serviceClient.GetTableClient("AccessCodes");
            _table.CreateIfNotExists();
        }

        public async Task<AccessCodeEntity?> GetAsync(string testId, string accessCode)
        {
            try
            {
                var response = await _table.GetEntityAsync<AccessCodeEntity>(testId, accessCode);
                return response.Value;
            }
            catch (Azure.RequestFailedException)
            {
                return null;
            }
        }

        public async Task<List<AccessCodeEntity>> GetByTestAsync(string testId)
        {
            var results = new List<AccessCodeEntity>();
            
            await foreach (var item in _table.QueryAsync<AccessCodeEntity>(a => a.PartitionKey == testId))
                results.Add(item);
            
            return results;
        }

        public async Task AddAsync(AccessCodeEntity entity) =>
            await _table.AddEntityAsync(entity);

        public async Task UpdateAsync(AccessCodeEntity entity) =>
            await _table.UpdateEntityAsync(entity, entity.ETag, TableUpdateMode.Replace);

        public async Task DeleteAsync(string testId, string code) =>
            await _table.DeleteEntityAsync(testId, code);

        public async Task GenerateAsync(string teacherId, string testId, int generateCount)
        {
            var generated = 0;
            while (generated < generateCount)
            {
                var code = new AccessCodeEntity
                {
                    PartitionKey = testId,
                    RowKey = AccessCodeGenerator.Generate(),
                    TeacherId = teacherId
                };
                await AddAsync(code);
                ++generated;
            }
        }
    }
}
