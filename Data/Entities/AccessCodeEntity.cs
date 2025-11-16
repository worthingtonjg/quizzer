namespace quizzer.Data.Entities
{
    // This holds access code details
    public class AccessCodeEntity : BaseTableEntity
    {
        public string TeacherId { get; set; } = string.Empty;
        public bool Used { get; set; } = false;
    }
}
