using Azure;
using Azure.Data.Tables;
using quizzer.Data.Entities;

namespace quizzer.Services
{
    public class ClassPeriodService
    {
        private readonly TableClient _table;

        public ClassPeriodService(IConfiguration config)
        {
            var connStr = config.GetConnectionString("StorageAccount")
                          ?? Environment.GetEnvironmentVariable("StorageAccount");

            _table = new TableClient(connStr, "ClassPeriods");
            _table.CreateIfNotExists();
        }

        /// <summary>
        /// Creates a new ClassPeriodEntity under a Course.
        /// </summary>
        public async Task<ClassPeriodEntity> CreateAsync(string courseId, string teacherId, string periodName)
        {
            var entity = new ClassPeriodEntity
            {
                PartitionKey = courseId,
                RowKey = Guid.NewGuid().ToString(),  // ClassPeriodId
                TeacherId = teacherId,
                PeriodName = periodName
            };

            await _table.AddEntityAsync(entity);
            return entity;
        }

        /// <summary>
        /// Gets a ClassPeriod by CourseId + ClassPeriodId.
        /// </summary>
        public async Task<ClassPeriodEntity> GetAsync(string courseId, string classPeriodId)
        {
            try
            {
                var response = await _table.GetEntityAsync<ClassPeriodEntity>(courseId, classPeriodId);
                return response.Value;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null;
            }
        }

        /// <summary>
        /// Lists all ClassPeriods for a given Course.
        /// </summary>
        public async Task<List<ClassPeriodEntity>> GetByCourseAsync(string courseId)
        {
            var results = new List<ClassPeriodEntity>();

            var query = _table.QueryAsync<ClassPeriodEntity>(c => c.PartitionKey == courseId);

            await foreach (var entity in query)
                results.Add(entity);

            return results.OrderBy(c => c.PeriodName).ToList();
        }

        /// <summary>
        /// Lists all ClassPeriods for a teacher (across all courses).
        /// </summary>
        public async Task<List<ClassPeriodEntity>> GetByTeacherAsync(string teacherId)
        {
            var results = new List<ClassPeriodEntity>();

            string filter = TableClient.CreateQueryFilter<ClassPeriodEntity>(
                c => c.TeacherId == teacherId
            );

            await foreach (var entity in _table.QueryAsync<ClassPeriodEntity>(filter))
                results.Add(entity);

            return results.OrderBy(c => c.PeriodName).ToList();
        }

        /// <summary>
        /// Update a class period.
        /// </summary>
        public async Task UpdateAsync(ClassPeriodEntity period)
        {
            await _table.UpdateEntityAsync(period, period.ETag, TableUpdateMode.Replace);
        }

        /// <summary>
        /// Upsert (insert or replace) a ClassPeriodEntity.
        /// </summary>
        public async Task UpsertAsync(ClassPeriodEntity period)
        {
            await _table.UpsertEntityAsync(period, TableUpdateMode.Replace);
        }

        /// <summary>
        /// Deletes a class period.
        /// </summary>
        public async Task DeleteAsync(string courseId, string classPeriodId)
        {
            await _table.DeleteEntityAsync(courseId, classPeriodId);
        }
    }
}
