using Microsoft.SemanticKernel;
using Quartz;
using quizzer.Services;
using System.Text.Json;

[DisallowConcurrentExecution]
public class GradeSubmissionsJob : IJob
{
    private readonly SubmissionService _submissionService;
    private readonly QuestionService _questionService;
    private readonly TestService _testService;
    private readonly Kernel _kernel;

    public GradeSubmissionsJob(
        SubmissionService submissionService,
        QuestionService questionService,
        TestService testService,
        IConfiguration config)
    {
        _submissionService = submissionService;
        _questionService = questionService;
        _testService = testService;

        var builder = Kernel.CreateBuilder();
        builder.AddAzureOpenAIChatCompletion(
            deploymentName: config.GetConnectionString("OpenAIDeploymentName") ?? Environment.GetEnvironmentVariable("OpenAIDeploymentName"),
            endpoint: config.GetConnectionString("OpenAIEndpoint") ?? Environment.GetEnvironmentVariable("OpenAIEndpoint"),
            apiKey: config.GetConnectionString("OpenAIApiKey") ?? Environment.GetEnvironmentVariable("OpenAIApiKey")
        );
        _kernel = builder.Build();
    }

    public async Task Execute(IJobExecutionContext context)
    {
        Console.WriteLine("GradeSubmissionsJob starting…");

        var ungraded = await _submissionService.GetUngradedAsync();
        Console.WriteLine($"Found {ungraded.Count} ungraded submissions.");

        foreach (var submission in ungraded)
        {
            Console.WriteLine($"Grading {submission.PartitionKey}/{submission.RowKey}…");

            try
            {
                // Load student answers
                var answers = JsonSerializer.Deserialize<Dictionary<string, string>>(submission.AnswersJson)
                             ?? new Dictionary<string, string>();

                // Load test + questions
                var test = await _testService.GetByTestIdAsync(submission.PartitionKey);
                var questions = await _questionService.GetByTestAsync(submission.PartitionKey);

                double totalScore = 0;
                var perQuestionScores = new Dictionary<string, double>();
                var perQuestionFeedback = new Dictionary<string, string>();

                foreach (var q in questions.OrderBy(q => q.QuestionNumber))
                {
                    var studentAnswer = answers.ContainsKey(q.RowKey)
                        ? answers[q.RowKey]
                        : string.Empty;

                    #region prompt
                    var prompt = $@"
You are an impartial grader for a teacher. Grade the student's answer using the test context below.

=== Test Context ===
Title: {test.Title}
Description: {test.Description}
Instructions: {test.Instructions}

=== Question ===
QuestionNumber: {q.QuestionNumber}
Prompt: {q.Prompt}

=== Expected Answer ===
{q.ExpectedAnswer}

=== Grading Rubric ===
{q.Rubric}

=== Maximum Points ===
{q.MaxPoints}

=== Student Answer ===
{studentAnswer}

=== Task ===
Evaluate the student's answer strictly according to the rubric.
The explanation should be worded as if you are speaking directly to the student.
Return ONLY the following JSON with no commentary:

{{
  ""score"": ""string"",
  ""explanation"": ""string"",
}}";
                    #endregion

                    var result = await _kernel.InvokePromptAsync(prompt);
                    var json = result.ToString();

                    var grade = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

                    double score = Convert.ToDouble(grade["score"]);
                    string explanation = grade["explanation"].ToString() ?? "";

                    // Track totals
                    totalScore += score;
                    perQuestionScores[q.RowKey] = score;
                    perQuestionFeedback[q.RowKey] = explanation;
                }

                // Save results
                submission.Score = totalScore;
                submission.GradedDate = DateTime.UtcNow;
                submission.PerQuestionScoresJson = JsonSerializer.Serialize(perQuestionScores);
                submission.Feedback = JsonSerializer.Serialize(perQuestionFeedback);
                
                await _submissionService.SaveSubmissionAsync(submission);
                
                Console.WriteLine($"Finished grading {submission.RowKey}. Score={submission.Score}/{test.TotalPoints}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error grading submission {submission.RowKey}: {ex.Message}");
            }
        }
    }
}
