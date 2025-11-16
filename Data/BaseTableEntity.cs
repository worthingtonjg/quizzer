using Azure;
using Azure.Data.Tables;

namespace quizzer.Data
{
    public class BaseTableEntity : ITableEntity
    {
        public string PartitionKey { get; set; } = "";
        public string RowKey { get; set; } = "";
        public ETag ETag { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;
    }
}
