namespace quizzer.Data.Entities
{
        // This holds teacher login info
        [EntityKeys("Role", "TeacherId")]
        public class UserEntity : BaseTableEntity
        {
            public string Name { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string PasswordHash { get; set; } = string.Empty;
        }
}
