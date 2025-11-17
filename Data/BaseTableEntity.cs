using Azure;
using Azure.Data.Tables;
using System.Reflection;

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

        private EntityKeysAttribute _keys =>
            this.GetType().GetCustomAttribute<EntityKeysAttribute>();

        public string this[string logicalKeyName]
        {
            get
            {
                if (_keys == null)
                    throw new InvalidOperationException(
                        $"EntityKeysAttribute not defined on {this.GetType().Name}");

                // Match logical names to PK/RK meanings
                if (logicalKeyName == _keys.PartitionKeyMeaning)
                    return PartitionKey;

                if (logicalKeyName == _keys.RowKeyMeaning)
                    return RowKey;

                throw new ArgumentException(
                    $"Key '{logicalKeyName}' not found on entity type '{this.GetType().Name}'. " +
                    $"Expected '{_keys.PartitionKeyMeaning}' or '{_keys.RowKeyMeaning}'.");
            }
        }
    }
}
