using Microsoft.AspNetCore.Components;
using quizzer.Services;
using System.Text.Json;

namespace quizzer.Pages.Teacher
{
    public class SubmissionsSection : ComponentBase
    {
        [Parameter] public string TestId { get; set; } = string.Empty;

        [Inject] protected SubmissionService SubmissionService { get; set; } = default!;
        [Inject] protected AccessCodeService AccessCodeService { get; set; } = default!;
        [Inject] protected QuestionService QuestionService { get; set; } = default!;
        [Inject] protected NavigationManager Nav { get; set; } = default!;

        protected bool IsLoading { get; set; } = true;
        protected List<SubmissionView> Submissions { get; set; } = new();

        protected override async Task OnInitializedAsync()
        {
            IsLoading = true;
            Submissions = await LoadSubmissionsAsync(TestId);
            IsLoading = false;
        }

        private async Task<List<SubmissionView>> LoadSubmissionsAsync(string testId)
        {
            var subs = new List<SubmissionView>();

            var submissionEntities = await SubmissionService.GetByTestAsync(testId);
            var accessCodes = await AccessCodeService.GetByTestAsync(testId);
            var questions = await QuestionService.GetByTestAsync(testId);
            int totalQuestions = questions.Count;

            foreach (var code in accessCodes)
            {
                var submission = submissionEntities.FirstOrDefault(s => s.RowKey == code.RowKey);

                int answeredCount = 0;
                if (submission != null && !string.IsNullOrEmpty(submission.AnswersJson))
                {
                    try
                    {
                        var answers = JsonSerializer.Deserialize<Dictionary<string, string>>(submission.AnswersJson);
                        answeredCount = answers?.Count(a => !string.IsNullOrWhiteSpace(a.Value)) ?? 0;
                    }
                    catch
                    {
                        // ignore malformed JSON
                    }
                }

                subs.Add(new SubmissionView
                {
                    AccessCode = code.RowKey,
                    Started = submission != null,
                    Submitted = submission?.SubmittedDate != null,
                    StartedDate = submission?.Timestamp?.UtcDateTime,
                    SubmittedDate = submission?.SubmittedDate,
                    Score = submission?.Score,
                    MaxScore = submission?.MaxScore,
                    QuestionsAnswered = answeredCount,
                    TotalQuestions = totalQuestions
                });
            }

            return subs.OrderByDescending(s => s.SubmittedDate ?? s.StartedDate).ToList();
        }

        protected void ViewSubmission(string accessCodeId)
        {
            Nav.NavigateTo($"/student/{accessCodeId}");
        }

        public class SubmissionView
        {
            public string AccessCode { get; set; } = string.Empty;
            public bool Started { get; set; }
            public bool Submitted { get; set; }
            public DateTime? StartedDate { get; set; }
            public DateTime? SubmittedDate { get; set; }
            public double? Score { get; set; }
            public double? MaxScore { get; set; }
            public int QuestionsAnswered { get; set; }
            public int TotalQuestions { get; set; }
        }
    }
}
