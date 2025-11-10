using Azure;
using Azure.Data.Tables;

namespace quizzer.Models
{
    public class QuestionEntity : ITableEntity
    {
        public string PartitionKey { get; set; } = string.Empty; // TestId
        public string RowKey { get; set; } = Guid.NewGuid().ToString(); // QuestionId

        public int QuestionNumber { get; set; }
        public string Prompt { get; set; } = string.Empty;
        public string ExpectedAnswer { get; set; } = string.Empty;
        public string Rubric { get; set; } = string.Empty;
        public double MaxPoints { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public ETag ETag { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
    }
}
