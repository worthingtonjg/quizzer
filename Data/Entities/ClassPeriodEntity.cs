namespace quizzer.Data.Entities
{
    // Represents a specific class period under a course (Period 1, Period 3, etc.)
    [EntityKeys("CourseId", "ClassPeriodId")]
    public class ClassPeriodEntity : BaseTableEntity
    {
        public string TeacherId { get; set; } = string.Empty; // duplicated for convenience and security
        public string PeriodName { get; set; } = string.Empty; // e.g., "Period 3"
    }
}
