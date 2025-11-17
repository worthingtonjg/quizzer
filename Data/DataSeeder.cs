using quizzer.Data.Entities;
using quizzer.Services;

namespace quizzer.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var userService = scope.ServiceProvider.GetRequiredService<UserService>();
        var courseService = scope.ServiceProvider.GetRequiredService<CourseService>();
        var periodService = scope.ServiceProvider.GetRequiredService<ClassPeriodService>();
        var studentService = scope.ServiceProvider.GetRequiredService<StudentService>();
        var testService = scope.ServiceProvider.GetRequiredService<TestService>();
        var questionService = scope.ServiceProvider.GetRequiredService<QuestionService>();

        string seedEmail = "teacher@example.com";
        string seedName = "Sample Teacher";
        string seedPassword = "Pass123!";

        Console.WriteLine("---- QUIZZER DATA SEEDER ----");

        // -----------------------------------------------------------
        // 1️⃣ SEED TEACHER
        // -----------------------------------------------------------
        var teacher = await userService.GetByEmailAsync(seedEmail);
        if (teacher == null)
        {
            bool success = await userService.RegisterAsync(seedName, seedEmail, seedPassword);
            if (!success)
            {
                Console.WriteLine("⚠️ Could not seed teacher user.");
                return;
            }

            teacher = await userService.GetByEmailAsync(seedEmail);
            Console.WriteLine($"✅ Seeded teacher: {seedEmail}");
        }
        else
        {
            Console.WriteLine($"ℹ️ Teacher '{seedEmail}' already exists — skipping user creation.");
        }

        if (teacher == null)
        {
            Console.WriteLine("⚠️ No teacher available — aborting.");
            return;
        }

        string teacherId = teacher["TeacherId"];


        // -----------------------------------------------------------
        // 2️⃣ SEED COURSES
        // -----------------------------------------------------------
        var courses = await courseService.GetByTeacherAsync(teacherId, false);
        if (courses.Any())
        {
            Console.WriteLine("ℹ️ Courses already exist — skipping course creation.");
        }

        var usHistory = courses.FirstOrDefault(c => c.CourseName == "US History")
            ?? await courseService.CreateAsync(teacherId, "US History");

        var utahHistory = courses.FirstOrDefault(c => c.CourseName == "Utah History")
            ?? await courseService.CreateAsync(teacherId, "Utah History");

        Console.WriteLine($"✅ Courses available: {usHistory.CourseName}, {utahHistory.CourseName}");


        // -----------------------------------------------------------
        // 3️⃣ SEED CLASS PERIODS FOR BOTH COURSES
        // -----------------------------------------------------------
        await SeedPeriodsAsync(periodService, studentService, teacherId, usHistory, new[] { 1, 3, 4 });
        await SeedPeriodsAsync(periodService, studentService, teacherId, utahHistory, new[] { 2, 5, 6, 7 });


        // -----------------------------------------------------------
        // 4️⃣ SEED SAMPLE TESTS FOR EACH COURSE
        // -----------------------------------------------------------
        await SeedTestAsync(
            testService, questionService,
            usHistory,
            "Sample US History Quiz"
        );

        await SeedTestAsync(
            testService, questionService,
            utahHistory,
            "Sample Utah History Quiz"
        );

        Console.WriteLine("🌱 Data seed complete.");
    }



    // -----------------------------------------------------------
    // Helper: Seed Class Periods & Students
    // -----------------------------------------------------------
    private static async Task SeedPeriodsAsync(
        ClassPeriodService periodService,
        StudentService studentService,
        string teacherId,
        CourseEntity course,
        int[] periodNumbers)
    {
        Console.WriteLine($"Seeding periods for course: {course.CourseName}");

        var existing = await periodService.GetByCourseAsync(course["CourseId"]);
        var createdPeriods = new List<ClassPeriodEntity>();

        foreach (var p in periodNumbers)
        {
            string name = $"Period {p}";

            var existingPeriod = existing.FirstOrDefault(e => e.PeriodName == name);
            if (existingPeriod != null)
            {
                Console.WriteLine($"ℹ️ {name} already exists.");
                createdPeriods.Add(existingPeriod);
                continue;
            }

            var newPeriod = await periodService.CreateAsync(course["CourseId"], teacherId, name);
            Console.WriteLine($"✅ Created {name}");
            createdPeriods.Add(newPeriod);
        }

        // Seed 5 students per period
        foreach (var period in createdPeriods)
        {
            var students = await studentService.GetByClassPeriodAsync(period["ClassPeriodId"]);
            if (students.Any())
            {
                Console.WriteLine($"ℹ️ Students already exist for {period.PeriodName} — skipping.");
                continue;
            }

            for (int i = 0; i < 5; i++)
            {
                var student = await studentService.CreateAsync(period["ClassPeriodId"], teacherId);
                Console.WriteLine($"   → Student access code: {student["StudentAccessCode"]}");
            }

            Console.WriteLine($"✅ Seeded 5 students for {period.PeriodName}");
        }
    }



    // -----------------------------------------------------------
    // Helper: Seed Tests + Questions per Course
    // -----------------------------------------------------------
    private static async Task SeedTestAsync(
        TestService testService,
        QuestionService questionService,
        CourseEntity course,
        string testTitle)
    {
        var tests = await testService.GetByCourseAsync(course["CourseId"]);

        var existing = tests.FirstOrDefault(t => t.Title == testTitle);
        TestEntity test;

        if (existing != null)
        {
            Console.WriteLine($"ℹ️ Test '{testTitle}' already exists — skipping.");
            test = existing;
        }
        else
        {
            test = new TestEntity(
                courseId: course["CourseId"],
                title: testTitle,
                description: "A sample quiz for this subject.",
                instructions: "Answer all questions.",
                isOpen: true,
                questionCount: 3,
                totalPoints: 15
            );

            await testService.AddAsync(test);
            Console.WriteLine($"✅ Seeded test: {test.Title}");
        }

        var questions = await questionService.GetByTestAsync(test["TestId"]);
        if (questions.Any())
        {
            Console.WriteLine("ℹ️ Questions already exist — skipping.");
            return;
        }

        var qlist = new[]
        {
            new QuestionEntity(test["TestId"], 1,
                "What were two major causes of the American Revolution?",
                "Taxation without representation and British trade restrictions.",
                "Mention at least two causes.",
                5),

            new QuestionEntity(test["TestId"], 2,
                "Who wrote the Declaration of Independence?",
                "Thomas Jefferson.",
                "Correct name required.",
                5),

            new QuestionEntity(test["TestId"], 3,
                "Name one key outcome of the Treaty of Paris (1783).",
                "Britain recognized American independence.",
                "Must mention independence or borders.",
                5)
        };

        foreach (var q in qlist)
            await questionService.AddAsync(q);

        Console.WriteLine($"✅ Seeded 3 questions for {course.CourseName}");
    }
}
