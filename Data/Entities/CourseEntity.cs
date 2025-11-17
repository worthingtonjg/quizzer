namespace quizzer.Data.Entities
{
    // Represents a subject the teacher teaches (US History, Utah History, etc.)
    [EntityKeys("TeacherId", "CourseId")]
    public class CourseEntity : BaseTableEntity
    {
        public string CourseName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        public CourseEntity()
        {
            
        }

        public CourseEntity(string teacherId, string name, bool isActive = true)
        {
            PartitionKey = teacherId;
            RowKey = Guid.NewGuid().ToString();
            CourseName = name;
            IsActive = IsActive;
        }
    }
}
