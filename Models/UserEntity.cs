namespace quizzer.Models
{
    using Azure;
    using Azure.Data.Tables;

    namespace YourAppNamespace.Models
    {
        public class UserEntity : ITableEntity
        {
            public string PartitionKey { get; set; } = "user";
            public string RowKey { get; set; } = Guid.NewGuid().ToString();
            public string Name { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string PasswordHash { get; set; } = string.Empty;
            public string Role { get; set; } = "Teacher";
            public ETag ETag { get; set; }
            public DateTimeOffset? Timestamp { get; set; }
            public DateTime CreatedUtc { get; internal set; }
        }
    }

}
