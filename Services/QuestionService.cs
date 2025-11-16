using Azure.Data.Tables;
using quizzer.Data.Entities;

namespace quizzer.Services
{
    public class QuestionService
    {
        private readonly TableClient _table;

        public QuestionService(IConfiguration config)
        {
            var connStr = config.GetConnectionString("StorageAccount")
                          ?? Environment.GetEnvironmentVariable("StorageAccount");
            if (string.IsNullOrWhiteSpace(connStr))
                throw new InvalidOperationException("StorageAccount connection string is missing.");

            var serviceClient = new TableServiceClient(connStr);
            _table = serviceClient.GetTableClient("Questions");
            _table.CreateIfNotExists();
        }

        public async Task<List<QuestionEntity>> GetByTestAsync(string testId)
        {
            var results = new List<QuestionEntity>();

            await foreach (var item in _table.QueryAsync<QuestionEntity>(q => q.PartitionKey == testId))
                results.Add(item);

            return results.OrderBy(q => q.CreatedDate).ToList();
        }

        public async Task AddAsync(QuestionEntity question) =>
            await _table.AddEntityAsync(question);

        public async Task UpdateAsync(QuestionEntity question) =>
            await _table.UpdateEntityAsync(question, question.ETag, TableUpdateMode.Replace);

        public async Task DeleteAsync(string testId, string questionId) =>
            await _table.DeleteEntityAsync(testId, questionId);
    }
}
