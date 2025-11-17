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

        public QuestionEntity()
        {
                
        }

        public QuestionEntity(string testId, int questionNumber, string prompt, string expectedAnswer, string rubric, double maxPoints)
        {
            PartitionKey = testId;
            RowKey = Guid.NewGuid().ToString();
            QuestionNumber = questionNumber;
            Prompt = prompt;
            ExpectedAnswer = expectedAnswer;
            Rubric = rubric;
            MaxPoints = maxPoints;
        }
    }
}
