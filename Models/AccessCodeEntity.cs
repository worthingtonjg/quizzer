using Azure;
using Azure.Data.Tables;
using System;

namespace quizzer.Models
{
    public class AccessCodeEntity : ITableEntity
    {
        public string PartitionKey { get; set; } = string.Empty; // TestId
        public string RowKey { get; set; } = string.Empty; // AccessCode

        public string TeacherId { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public bool Used { get; set; } = false;
        public DateTime? SubmittedDate { get; set; }

        public ETag ETag { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
    }
}
