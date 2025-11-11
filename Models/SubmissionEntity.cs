using Azure;
using Azure.Data.Tables;
using System;

namespace quizzer.Models
{
    public class SubmissionEntity : ITableEntity
    {
        public string PartitionKey { get; set; } = string.Empty; // TestId
        public string RowKey { get; set; } = string.Empty; // AccessCode

        public string AnswersJson { get; set; } = string.Empty;
        public DateTime? SubmittedDate { get; set; } 
        public double? Score { get; set; }
        public double? MaxScore { get; set; }
        public bool Graded { get; set; } = false;
        public string Feedback { get; set; } = string.Empty;
        public DateTime? GradedDate { get; set; }
        public string PerQuestionScoresJson { get; set; } = string.Empty; // {"Q1":4,"Q2":5}
        public ETag ETag { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
    }
}
