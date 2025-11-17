namespace quizzer.Data.Entities
{
    // Represents a student identity in a specific class period (anonymous access code)
    [EntityKeys("ClassPeriodId", "StudentAccessCode")]
    public class StudentEntity : BaseTableEntity
    {
        public string TeacherId { get; set; } = string.Empty; // duplicated for convenience
        public string PeriodId { get; set; } = string.Empty; // e.g., "Period 3"
        public string CourseId { get; set; } = string.Empty; // e.g., "Biology 101"
    }
}
