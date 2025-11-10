namespace quizzer.Services
{
    public static class AccessCodeGenerator
    {
        /// <summary>
        /// Generates a unique access code for a student/test combination.
        /// Currently uses a GUID (32 hex characters, no dashes).
        /// </summary>
        public static string Generate()
        {
            return Guid.NewGuid().ToString("N"); // Example: "2f8b3d2e9c424e81bb08cf4f81a73a6b"
        }
    }
}
