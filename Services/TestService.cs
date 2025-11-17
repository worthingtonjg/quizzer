using Azure;
using Azure.Data.Tables;
using quizzer.Data.Entities;

namespace quizzer.Services
{
    public class TestService
    {
        private readonly TableClient _table;

        public TestService(IConfiguration config)
        {
            var connStr = config.GetConnectionString("StorageAccount")
                          ?? Environment.GetEnvironmentVariable("StorageAccount");

            if (string.IsNullOrWhiteSpace(connStr))
                throw new InvalidOperationException("StorageAccount connection string is missing.");

            var serviceClient = new TableServiceClient(connStr);
            _table = serviceClient.GetTableClient("Tests");
            _table.CreateIfNotExists();
        }

        // 🔹 Add a new test
        public async Task AddAsync(TestEntity test)
        {
            test.LastModifiedDate = DateTime.UtcNow;
            await _table.AddEntityAsync(test);
        }

        // 🔹 Update an existing test
        public async Task UpdateAsync(TestEntity test)
        {
            test.LastModifiedDate = DateTime.UtcNow;
            await _table.UpsertEntityAsync(test);
        }

        // 🔹 Delete a test
        public async Task DeleteAsync(string teacherId, string testId)
        {
            await _table.DeleteEntityAsync(teacherId, testId);
        }

        // 🔹 Get all tests for a teacher
        public async Task<List<TestEntity>> GetByTeacherAsync(string teacherId)
        {
            var tests = new List<TestEntity>();
            var query = _table.QueryAsync<TestEntity>(t => t.PartitionKey == teacherId);

            await foreach (var test in query)
                tests.Add(test);

            return tests.OrderByDescending(t => t.LastModifiedDate).ToList();
        }

        // 🔹 Get a test by teacher and test ID (key-based lookup)
        public async Task<TestEntity> GetByIdAsync(string teacherId, string testId)
        {
            try
            {
                var response = await _table.GetEntityAsync<TestEntity>(teacherId, testId);
                return response.Value;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null;
            }
        }

        public async Task<TestEntity> GetByTestIdAsync(string testId)
        {
            await foreach (var entity in _table.QueryAsync<TestEntity>(t => t.RowKey == testId))
                return entity; // should only be one

            return null;
        }

        public async Task<List<TestEntity>> GetByCourseAsync(string courseId)
        {
            var results = new List<TestEntity>();

            try
            {
                // Query all tests belonging to this course
                var query = _table.QueryAsync<TestEntity>(t => t.PartitionKey == courseId);

                await foreach (var entity in query)
                    results.Add(entity);

                // Optional: sort by title or creation date
                return results.OrderBy(t => t.Title).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading tests for course '{courseId}': {ex.Message}");
                return new List<TestEntity>();
            }
        }

    }
}
