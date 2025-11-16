namespace quizzer.Data.Entities
{
    // Represents a subject the teacher teaches (US History, Utah History, etc.)
    [EntityKeys("TeacherId", "CourseId")]
    public class CourseEntity : BaseTableEntity
    {
        public string CourseName { get; set; } = string.Empty;
        public string CourseDescription { get; set; } = string.Empty;
    }
}
