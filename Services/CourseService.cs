using Azure;
using Azure.Data.Tables;
using quizzer.Data.Entities;

namespace quizzer.Services
{
    public class CourseService
    {
        private readonly TableClient _table;

        public CourseService(IConfiguration config)
        {
            var connStr = config.GetConnectionString("StorageAccount")
                          ?? Environment.GetEnvironmentVariable("StorageAccount");

            _table = new TableClient(connStr, "Courses");
            _table.CreateIfNotExists();
        }

        /// <summary>
        /// Creates a new course for a teacher.
        /// </summary>
        public async Task<CourseEntity> CreateAsync(string teacherId, string courseName)
        {
            var entity = new CourseEntity
            {
                PartitionKey = teacherId,            // TeacherId
                RowKey = Guid.NewGuid().ToString(), // CourseId
                CourseName = courseName,
            };

            await _table.AddEntityAsync(entity);
            return entity;
        }

        /// <summary>
        /// Gets a single CourseEntity by TeacherId + CourseId.
        /// </summary>
        public async Task<CourseEntity> GetAsync(string teacherId, string courseId)
        {
            try
            {
                var response = await _table.GetEntityAsync<CourseEntity>(teacherId, courseId);
                return response.Value;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null;
            }
        }

        /// <summary>
        /// Lists all courses taught by a specific teacher.
        /// </summary>
        public async Task<List<CourseEntity>> GetByTeacherAsync(string teacherId, bool showActive = true)
        {
            var results = new List<CourseEntity>();

            AsyncPageable<CourseEntity> query;

            if (showActive)
            {
                query = _table.QueryAsync<CourseEntity>(c => c.PartitionKey == teacherId && c.IsActive == true);
            }
            else
            {
                query = _table.QueryAsync<CourseEntity>(c => c.PartitionKey == teacherId);
            }
                
            await foreach (var entity in query)
                    results.Add(entity);

            return results.OrderBy(c => c.CourseName).ToList();
        }

        /// <summary>
        /// Updates an existing course.
        /// </summary>
        public async Task UpdateAsync(CourseEntity course)
        {
            await _table.UpdateEntityAsync(course, course.ETag, TableUpdateMode.Replace);
        }

        /// <summary>
        /// Upsert (insert or replace) a CourseEntity.
        /// </summary>
        public async Task UpsertAsync(CourseEntity course)
        {
            await _table.UpsertEntityAsync(course, TableUpdateMode.Replace);
        }

        /// <summary>
        /// Deletes a course.
        /// </summary>
        public async Task DeleteAsync(string teacherId, string courseId)
        {
            await _table.DeleteEntityAsync(teacherId, courseId);
        }
    }
}
