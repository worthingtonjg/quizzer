namespace quizzer.Data.Entities
{
    // Represents a student identity in a specific class period (anonymous access code)
    [EntityKeys("ClassPeriodId", "StudentAccessCode")]
    public class StudentEntity : BaseTableEntity
    {
        public string TeacherId { get; set; } = string.Empty; // duplicated for convenience
    }
}
