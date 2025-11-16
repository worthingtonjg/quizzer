using Azure;
using Azure.Data.Tables;

namespace quizzer.Data.Entities
{
    // This holds test metadata
    [EntityKeys("CourseId", "TestId")]
    public class TestEntity : BaseTableEntity
    {
        public string TeacherId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Instructions { get; set; } = string.Empty;
        public bool IsOpen { get; set; } = false;
        public bool IsPublished { get; set; } = false;
        public int QuestionCount { get; set; } = 0;
        public int TotalPoints { get; set; } = 0;

        // Read-only properties for convenience
        public bool IsClosed => IsPublished && !IsOpen;
    }
}
