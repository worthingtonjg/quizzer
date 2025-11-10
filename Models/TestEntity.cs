using Azure;
using Azure.Data.Tables;

namespace quizzer.Models
{
    public class TestEntity : ITableEntity
    {
        public string PartitionKey { get; set; } = string.Empty; // TeacherId
        public string RowKey { get; set; } = Guid.NewGuid().ToString(); // TestId

        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Instructions { get; set; } = string.Empty;
        public bool IsOpen { get; set; } = false;
        public bool IsPublished { get; set; } = false;
        public int QuestionCount { get; set; } = 0;
        public int TotalPoints { get; set; } = 0;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;

        public ETag ETag { get; set; }
        public DateTimeOffset? Timestamp { get; set; }

        public bool IsDraft => !IsPublished;
        public bool IsPublishedOnly => IsPublished && !IsOpen && !IsClosed;
        public bool IsClosed => IsPublished && !IsOpen;
    }
}
