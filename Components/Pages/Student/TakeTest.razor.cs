using Microsoft.AspNetCore.Components;
using quizzer.Models;
using quizzer.Services;
using System.Text.Json;

namespace quizzer.Pages.Student
{
    public partial class TakeTest : ComponentBase
    {
        [Parameter] public string AccessCodeId { get; set; } = string.Empty;

        [Inject] protected AccessCodeService AccessCodeService { get; set; } = default!;
        [Inject] protected TestService TestService { get; set; } = default!;
        [Inject] protected QuestionService QuestionService { get; set; } = default!;
        [Inject] protected SubmissionService SubmissionService { get; set; } = default!;
        [Inject] protected NavigationManager Nav { get; set; } = default!;

        protected bool IsLoading { get; set; } = true;
        protected string? ErrorMessage { get; set; }

        protected TestEntity? Test { get; set; }
        protected List<QuestionEntity> Questions { get; set; } = new();
        protected Dictionary<string, string> Responses { get; set; } = new();
        protected bool IsSubmitted { get; set; } = false;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                var accessCode = await AccessCodeService.GetByIdAsync(AccessCodeId);
                if (accessCode == null)
                {
                    ErrorMessage = "Invalid or expired access link.";
                    return;
                }

                Test = await TestService.GetByIdAsync(accessCode.TeacherId, accessCode.PartitionKey);
                if (Test == null)
                {
                    ErrorMessage = "This test no longer exists.";
                    return;
                }

                if (!Test.IsPublished)
                {
                    ErrorMessage = "This test has not been published yet.";
                    return;
                }

                if (!Test.IsOpen)
                {
                    ErrorMessage = "This test is currently closed. Please check with your teacher for availability.";
                    return;
                }

                Questions = await QuestionService.GetByTestAsync(Test.RowKey);
                Responses = Questions.ToDictionary(q => q.RowKey, q => string.Empty);

                // 🟩 Load existing submission
                var existingSubmission = await SubmissionService.GetByIdAsync(Test.RowKey, AccessCodeId);
                if (existingSubmission != null)
                {
                    IsSubmitted = existingSubmission.SubmittedDate != null;  // 🟩 key change

                    if (!string.IsNullOrEmpty(existingSubmission.AnswersJson))
                    {
                        var savedAnswers = JsonSerializer.Deserialize<Dictionary<string, string>>(existingSubmission.AnswersJson);
                        if (savedAnswers != null)
                        {
                            foreach (var kvp in savedAnswers)
                                if (Responses.ContainsKey(kvp.Key))
                                    Responses[kvp.Key] = kvp.Value;
                        }
                    }
                }
            }
            finally
            {
                IsLoading = false;
            }
        }


        protected async Task SaveAnswerAsync(string questionId, string answer)
        {
            // if the test is submitted, skip saving
            if (Test == null || IsSubmitted)
                return;

            try
            {
                await SubmissionService.UpsertAnswerAsync(Test.RowKey, AccessCodeId, questionId, answer);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Auto-save failed for {questionId}: {ex.Message}");
            }
        }


        protected async Task SubmitAsync()
        {
            if (Test == null) return;

            await SubmissionService.MarkSubmittedAsync(Test.RowKey, AccessCodeId);
            IsSubmitted = true;
        }


        protected void GoHome()
        {
            Nav.NavigateTo("/");
        }
    }
}
