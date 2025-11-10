using quizzer.Models;

namespace quizzer.Services
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();

            var userService = scope.ServiceProvider.GetRequiredService<UserService>();
            var testService = scope.ServiceProvider.GetRequiredService<TestService>();
            var questionService = scope.ServiceProvider.GetRequiredService<QuestionService>();
            var accessCodeService = scope.ServiceProvider.GetRequiredService<AccessCodeService>();

            string seedEmail = "teacher@example.com";
            string seedPassword = "Pass123!";

            // 1️⃣ Seed Teacher
            var teacher = await userService.GetByEmailAsync(seedEmail);
            if (teacher == null)
            {
                bool success = await userService.RegisterAsync(seedEmail, seedPassword);
                if (success)
                {
                    teacher = await userService.GetByEmailAsync(seedEmail);
                    Console.WriteLine($"✅ Seeded teacher user: {seedEmail} / {seedPassword}");
                }
                else
                {
                    Console.WriteLine($"⚠️ Could not seed user (already exists?)");
                    return;
                }
            }
            else
            {
                Console.WriteLine($"ℹ️ User '{seedEmail}' already exists — skipping seed.");
            }

            if (teacher == null)
            {
                Console.WriteLine("⚠️ No teacher available — aborting seeding.");
                return;
            }

            // 2️⃣ Check if sample test already exists
            var existingTests = await testService.GetByTeacherAsync(teacher.RowKey);
            if (existingTests.Any())
            {
                Console.WriteLine("ℹ️ Test data already exists — skipping seed.");
                return;
            }

            // 3️⃣ Create sample test
            var test = new TestEntity
            {
                PartitionKey = teacher.RowKey,
                Title = "Sample History Quiz",
                Description = "A short quiz on early U.S. history.",
                Instructions = "Answer each question completely. Partial credit may be awarded.",
                IsOpen = true,
                QuestionCount = 3,
                TotalPoints = 15
            };

            await testService.AddAsync(test);
            Console.WriteLine($"✅ Seeded test: {test.Title}");

            // 4️⃣ Add sample questions
            var questions = new[]
            {
                new QuestionEntity
                {
                    PartitionKey = test.RowKey,
                    QuestionNumber = 1,
                    Prompt = "What were two major causes of the American Revolution?",
                    ExpectedAnswer = "Taxation without representation and British control over trade.",
                    Rubric = "Mention at least two causes for full credit.",
                    MaxPoints = 5
                },
                new QuestionEntity
                {
                    PartitionKey = test.RowKey,
                    QuestionNumber = 2,
                    Prompt = "Who wrote the Declaration of Independence?",
                    ExpectedAnswer = "Thomas Jefferson.",
                    Rubric = "Name must be correct for full credit.",
                    MaxPoints = 5
                },
                new QuestionEntity
                {
                    PartitionKey = test.RowKey,
                    QuestionNumber = 3,
                    Prompt = "Name one key outcome of the Treaty of Paris (1783).",
                    ExpectedAnswer = "Recognition of American independence by Britain.",
                    Rubric = "Must mention independence or new borders.",
                    MaxPoints = 5
                }
            };

            foreach (var q in questions)
                await questionService.AddAsync(q);

            Console.WriteLine("✅ Seeded 3 questions");

            for (int i = 0; i < 5; i++)
            {
                var code = new AccessCodeEntity
                {
                    PartitionKey = test.RowKey,
                    RowKey = AccessCodeGenerator.Generate(),
                    TeacherId = teacher.RowKey
                };
                await accessCodeService.AddAsync(code);
            }

            Console.WriteLine("✅ Seeded 5 access codes");
        }
    }
}
