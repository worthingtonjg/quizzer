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

        public TestEntity()
        {
                
        }

        public TestEntity(string courseId, string title, string description = null, string instructions = null, int questionCount = 0, int totalPoints = 0, bool isOpen = false)
        {
            PartitionKey = courseId;
            RowKey = Guid.NewGuid().ToString();
            Title = title;

            // Data seeding values
            Description = description;
            Instructions = instructions;
            QuestionCount = questionCount;
            TotalPoints = totalPoints;

            // By default, if a test is open, it is also published (for data seeding purposes)
            IsOpen = isOpen;
            IsPublished = isOpen;
        }
    }
}
