namespace quizzer.Data.Entities
{
    // This holds student submissions
    [EntityKeys("StudentAccessCode", "TestId")]
    public class SubmissionEntity : BaseTableEntity
    {
        public string AnswersJson { get; set; } = string.Empty;
        public DateTime? SubmittedDate { get; set; } 
        public double? Score { get; set; }
        public string Feedback { get; set; } = string.Empty;
        public DateTime? GradedDate { get; set; }
        public string PerQuestionScoresJson { get; set; } = string.Empty; // {"Q1":4,"Q2":5}

        // Read-only property for convenience
        public bool Graded => GradedDate != null;
    }
}
