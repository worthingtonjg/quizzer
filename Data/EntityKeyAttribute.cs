namespace quizzer.Data
{
    /// <summary>
    /// Documents the meaning of PartitionKey and RowKey for a table entity.
    /// This is metadata only and does not impact serialization or storage.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class EntityKeysAttribute : Attribute
    {
        /// <summary>
        /// Describes what the PartitionKey represents for this entity.
        /// </summary>
        public string PartitionKeyMeaning { get; }

        /// <summary>
        /// Describes what the RowKey represents for this entity.
        /// </summary>
        public string RowKeyMeaning { get; }

        public EntityKeysAttribute(string partitionKeyMeaning, string rowKeyMeaning)
        {
            PartitionKeyMeaning = partitionKeyMeaning;
            RowKeyMeaning = rowKeyMeaning;
        }
    }
}
