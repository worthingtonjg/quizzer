using Azure;
using Azure.Data.Tables;
using quizzer.Data.Entities;
using System.Text.Json;

namespace quizzer.Services
{
    public class SubmissionService
    {
        private readonly TableClient _table;

        public SubmissionService(IConfiguration config)
        {
            var connStr = config.GetConnectionString("StorageAccount")
                          ?? Environment.GetEnvironmentVariable("StorageAccount");

            _table = new TableClient(connStr, "Submissions");
            _table.CreateIfNotExists();
        }

        public async Task<SubmissionEntity> GetByIdAsync(string testId, string accessCodeId)
        {
            try
            {
                var response = await _table.GetEntityAsync<SubmissionEntity>(testId, accessCodeId);
                return response.Value;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null;
            }
        }

        public async Task UpsertAnswerAsync(string testId, string accessCodeId, string questionId, string answer)
        {
            // Get existing submission
            var submission = await GetByIdAsync(testId, accessCodeId)
                             ?? new SubmissionEntity
                             {
                                 PartitionKey = testId,
                                 RowKey = accessCodeId,
                                 AnswersJson = "{}",
                             };

            // Deserialize current answers
            var answers = string.IsNullOrWhiteSpace(submission.AnswersJson)
                ? new Dictionary<string, string>()
                : JsonSerializer.Deserialize<Dictionary<string, string>>(submission.AnswersJson) ?? new();

            // Update this question
            answers[questionId] = answer;

            // Re-serialize
            submission.AnswersJson = JsonSerializer.Serialize(answers);

            await _table.UpsertEntityAsync(submission, TableUpdateMode.Replace);
        }

        public async Task MarkSubmittedAsync(string testId, string accessCodeId)
        {
            var submission = await GetByIdAsync(testId, accessCodeId);
            if (submission == null) return;

            if (submission.SubmittedDate == null)
            {
                submission.SubmittedDate = DateTime.UtcNow;
                await _table.UpdateEntityAsync(submission, ETag.All, TableUpdateMode.Replace);
            }
        }

        public async Task<List<SubmissionEntity>> GetByTestAsync(string testId)
        {
            var results = new List<SubmissionEntity>();

            try
            {
                // Query all submissions where PartitionKey == testId
                var query = _table.QueryAsync<SubmissionEntity>(s => s.PartitionKey == testId);

                await foreach (var entity in query)
                    results.Add(entity);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading submissions for test {testId}: {ex.Message}");
            }

            return results;
        }

        public async Task SaveAllAnswersAsync(string testId, string accessCodeId, string answersJson)
        {
            var submission = await GetByIdAsync(testId, accessCodeId) ?? new SubmissionEntity
            {
                PartitionKey = testId,
                RowKey = accessCodeId
            };

            submission.AnswersJson = answersJson;
            await _table.UpsertEntityAsync(submission);
        }

        public async Task<List<SubmissionEntity>> GetUngradedAsync()
        {
            var results = new List<SubmissionEntity>();

            var cutoff = DateTime.Today.AddDays(-1);

            string filter = TableClient.CreateQueryFilter<SubmissionEntity>(
                s => s.SubmittedDate >= cutoff
            );

            await foreach (var s in _table.QueryAsync<SubmissionEntity>(filter))
            {
                if (s.SubmittedDate != null && s.GradedDate == null)
                    results.Add(s);
            }

            // Only grade 50 tests at a time, ordered by submission date
            return results
                .OrderBy(s => s.SubmittedDate)  
                .Take(50)
                .ToList();
        }

        public async Task SaveSubmissionAsync(SubmissionEntity submission)
        {
            await _table.UpsertEntityAsync(submission, TableUpdateMode.Replace);
        }
    }
}

