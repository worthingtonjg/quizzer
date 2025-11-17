using Azure;
using Azure.Data.Tables;
using quizzer.Data.Entities;

namespace quizzer.Services
{
    public class StudentService
    {
        private readonly TableClient _table;

        public StudentService(IConfiguration config)
        {
            var connStr = config.GetConnectionString("StorageAccount")
                          ?? Environment.GetEnvironmentVariable("StorageAccount");

            _table = new TableClient(connStr, "Students");
            _table.CreateIfNotExists();
        }

        /// <summary>
        /// Creates a new student identity for a class period.
        /// StudentAccessCode = random unique code.
        /// </summary>
        public async Task<StudentEntity> CreateAsync(string classPeriodId, string teacherId)
        {
            var accessCode = AccessCodeGenerator.Generate();

            var entity = new StudentEntity
            {
                PartitionKey = classPeriodId,
                RowKey = accessCode,
                TeacherId = teacherId
            };

            await _table.AddEntityAsync(entity);
            return entity;
        }

        /// <summary>
        /// Lists all students in a class period.
        /// </summary>
        public async Task<List<StudentEntity>> GetByClassPeriodAsync(string classPeriodId)
        {
            var results = new List<StudentEntity>();

            var query = _table.QueryAsync<StudentEntity>(s => s.PartitionKey == classPeriodId);

            await foreach (var entity in query)
                results.Add(entity);

            return results.OrderBy(s => s.RowKey).ToList(); // order by access code
        }

        /// <summary>
        /// Deletes all students for a given class period.
        /// </summary>
        public async Task DeleteAllByClassPeriodAsync(string classPeriodId)
        {
            // Get all students for this period
            var students = await GetByClassPeriodAsync(classPeriodId);

            foreach (var student in students)
            {
                await _table.DeleteEntityAsync(student.PartitionKey, student.RowKey);
            }
        }


        /// <summary>
        /// Gets a student by ClassPeriod + AccessCode.
        /// </summary>
        public async Task<StudentEntity> GetAsync(string classPeriodId, string accessCode)
        {
            try
            {
                var response = await _table.GetEntityAsync<StudentEntity>(classPeriodId, accessCode);
                return response.Value;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null;
            }
        }

        /// <summary>
        /// Lists all students for a teacher across all courses and periods.
        /// </summary>
        public async Task<List<StudentEntity>> GetByTeacherAsync(string teacherId)
        {
            var results = new List<StudentEntity>();

            string filter = TableClient.CreateQueryFilter<StudentEntity>(
                s => s.TeacherId == teacherId
            );

            await foreach (var entity in _table.QueryAsync<StudentEntity>(filter))
                results.Add(entity);

            return results.ToList();
        }

        /// <summary>
        /// Updates a student entity.
        /// </summary>
        public async Task UpdateAsync(StudentEntity student)
        {
            await _table.UpdateEntityAsync(student, student.ETag, TableUpdateMode.Replace);
        }

        /// <summary>
        /// Upsert (insert or replace) a student entity.
        /// </summary>
        public async Task UpsertAsync(StudentEntity student)
        {
            await _table.UpsertEntityAsync(student, TableUpdateMode.Replace);
        }

        /// <summary>
        /// Deletes a student identity.
        /// </summary>
        public async Task DeleteAsync(string classPeriodId, string accessCode)
        {
            await _table.DeleteEntityAsync(classPeriodId, accessCode);
        }

        /// <summary>
        /// Creates a short, readable access code.
        /// </summary>
        private static string GenerateAccessCode()
        {
            return Guid.NewGuid().ToString("N").Substring(0, 8).ToUpperInvariant();
        }
    }
}
