using Microsoft.AspNetCore.Components;
using quizzer.Data.Entities;
using quizzer.Services;
using System.Text.Json;

namespace quizzer.Components.Pages.Teacher
{
    public partial class TestSubmissions : ComponentBase
    {
        [Parameter] public string TestId { get; set; } = string.Empty;

        [Inject] protected CurrentUserService CurrentUser { get; set; } = default!;
        [Inject] protected TestService TestService { get; set; } = default!;
        [Inject] protected SubmissionService SubmissionService { get; set; } = default!;
        [Inject] protected StudentService StudentService { get; set; } = default!;
        [Inject] protected ClassPeriodService ClassPeriodService { get; set; } = default!;
        [Inject] protected QuestionService QuestionService { get; set; } = default!;
        [Inject] protected NavigationManager Nav { get; set; } = default!;

        protected bool IsLoading { get; set; } = true;

        protected TestEntity? Test { get; set; }
        protected string CourseId => Test?.PartitionKey ?? string.Empty;

        protected List<ClassPeriodEntity> Periods { get; set; } = new();
        protected string? SelectedPeriodId { get; set; }

        protected List<SubmissionView> Submissions { get; set; } = new();
        protected int TotalQuestions { get; set; }

        protected override async Task OnInitializedAsync()
        {
            IsLoading = true;

            var teacher = await CurrentUser.GetCurrentUserAsync();
            if (teacher == null)
            {
                Nav.NavigateTo("/login");
                return;
            }

            Test = await TestService.GetByTestIdAsync(TestId);
            if (Test == null)
            {
                Nav.NavigateTo("/teacher/dashboard");
                return;
            }

            // Load Periods for the Course
            Periods = await ClassPeriodService.GetByCourseAsync(Test.PartitionKey);

            // Default to first period
            SelectedPeriodId = Periods.FirstOrDefault()?.RowKey;

            // Load questions count
            var questions = await QuestionService.GetByTestAsync(TestId);
            TotalQuestions = questions.Count;

            if (SelectedPeriodId != null)
                await LoadSubmissionsAsync();

            IsLoading = false;
        }

        protected async Task SelectPeriod(string periodId)
        {
            SelectedPeriodId = periodId;
            await LoadSubmissionsAsync();
        }

        private async Task LoadSubmissionsAsync()
        {
            if (SelectedPeriodId == null) return;

            Submissions.Clear();

            // Load all students in this period
            var students = await StudentService.GetByClassPeriodAsync(SelectedPeriodId);

            // Load all submissions for this test
            var submissionEntities = await SubmissionService.GetByTestAsync(TestId);

            foreach (var student in students)
            {
                var submission = submissionEntities.FirstOrDefault(s => s.RowKey == student.RowKey);

                int answeredCount = 0;
                if (submission != null && !string.IsNullOrEmpty(submission.AnswersJson))
                {
                    try
                    {
                        var answers = JsonSerializer.Deserialize<Dictionary<string, string>>(submission.AnswersJson);
                        answeredCount = answers?.Count(a => !string.IsNullOrWhiteSpace(a.Value)) ?? 0;
                    }
                    catch { }
                }

                Submissions.Add(new SubmissionView
                {
                    AccessCode = student.RowKey,
                    Student = $"{student.PeriodId} - {student.RowKey}",
                    Started = submission != null,
                    Submitted = submission?.SubmittedDate != null,
                    StartedDate = submission?.Timestamp?.UtcDateTime,
                    SubmittedDate = submission?.SubmittedDate,
                    Score = submission?.Score,
                    MaxScore = Test?.TotalPoints,
                    QuestionsAnswered = answeredCount,
                    TotalQuestions = TotalQuestions
                });
            }

            Submissions = Submissions
                .OrderByDescending(s => s.SubmittedDate ?? s.StartedDate)
                .ToList();
        }

        protected void ViewSubmission(string accessCodeId)
        {
            Nav.NavigateTo($"/student/{accessCodeId}");
        }

        protected void GoBack() => Nav.NavigateTo("/teacher/dashboard");

        public class SubmissionView
        {
            public string AccessCode { get; set; } = string.Empty;
            public string Student { get; set; } = string.Empty;
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
