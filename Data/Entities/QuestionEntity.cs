namespace quizzer.Data.Entities
{
    // This holds question details
    [EntityKeys("TestId", "QuestionId")]
    public class QuestionEntity : BaseTableEntity
    {
        public int QuestionNumber { get; set; }
        public string Prompt { get; set; } = string.Empty;
        public string ExpectedAnswer { get; set; } = string.Empty;
        public string Rubric { get; set; } = string.Empty;
        public double MaxPoints { get; set; }
    }
}
