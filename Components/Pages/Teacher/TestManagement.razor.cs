using Microsoft.AspNetCore.Components;
using quizzer.Data.Entities;
using quizzer.Services;

namespace quizzer.Pages.Teacher
{
    public partial class TestManagement : ComponentBase
    {
        [Parameter] public string TestId { get; set; } = string.Empty;

        [Inject] protected CurrentUserService CurrentUser { get; set; } = default!;
        [Inject] protected NavigationManager Nav { get; set; } = default!;
        [Inject] protected TestService TestService { get; set; } = default!;
        [Inject] protected QuestionService QuestionService { get; set; } = default!;
        [Inject] protected UserService UserService { get; set; } = default!;  // 🔹 used for current teacher

        protected TestEntity Test { get; set; }
        protected List<QuestionEntity> Questions { get; set; } = new();
        protected bool IsLoading { get; set; } = true;
        protected string TeacherId { get; set; }
        protected string TeacherEmail { get; set; }
        protected readonly string[] Tabs = new[] { "Edit", "Access Codes", "Submissions" };
        protected string ActiveTab { get; set; } = "Edit";
        protected bool ShowDeleteModal { get; set; } = false;
        protected void ShowDeleteConfirm() => ShowDeleteModal = true;
        protected void HideDeleteConfirm() => ShowDeleteModal = false;
        protected bool IsSaving { get; set; } = false;
        protected bool AddingQuestion { get; set; } = false;
        protected string NewQuestionPrompt { get; set; } = string.Empty;
        protected int NewQuestionPoints { get; set; } = 1;
        protected string NewQuestionExpectedAnswer { get; set; } = string.Empty;
        protected string NewQuestionRubric { get; set; } = string.Empty;
        protected bool ShowDeleteQuestionModal { get; set; }
        protected QuestionEntity QuestionToDelete { get; set; }
        protected bool EditingQuestion { get; set; } = false;
        protected QuestionEntity QuestionBeingEdited { get; set; }
        protected string EditPrompt { get; set; } = string.Empty;
        protected string EditExpectedAnswer { get; set; } = string.Empty;
        protected string EditRubric { get; set; } = string.Empty;
        protected int EditPoints { get; set; } = 1;

        protected override async Task OnInitializedAsync()
        {
            IsLoading = true;

            var teacher = await CurrentUser.GetCurrentUserAsync();
            if (teacher == null)
            {
                Nav.NavigateTo("/login");
                return;
            }

            TeacherId = teacher.RowKey;
            TeacherEmail = teacher.Email;

            Test = await TestService.GetByIdAsync(TeacherId, TestId);
            Questions = await QuestionService.GetByTestAsync(TestId);
            IsLoading = false;
        }

        protected void EditQuestion(QuestionEntity question)
        {
            EditingQuestion = true;
            QuestionBeingEdited = question;

            // Pre-fill fields
            EditPrompt = question.Prompt;
            EditExpectedAnswer = question.ExpectedAnswer;
            EditRubric = question.Rubric;
            EditPoints = (int)question.MaxPoints;
        }

        protected void CancelEditQuestion()
        {
            EditingQuestion = false;
            QuestionBeingEdited = null;
        }

        protected async Task SaveEditedQuestion()
        {
            if (QuestionBeingEdited == null || Test == null)
                return;

            // Update fields
            QuestionBeingEdited.Prompt = EditPrompt.Trim();
            QuestionBeingEdited.ExpectedAnswer = EditExpectedAnswer.Trim();
            QuestionBeingEdited.Rubric = EditRubric.Trim();
            QuestionBeingEdited.MaxPoints = EditPoints;
            QuestionBeingEdited.Timestamp = DateTime.UtcNow;

            // Persist to storage
            await QuestionService.UpdateAsync(QuestionBeingEdited);

            // Re-compute totals
            Test.QuestionCount = Questions.Count;
            Test.TotalPoints = (int)Questions.Sum(q => q.MaxPoints);
            await TestService.UpdateAsync(Test);

            // Reset state
            EditingQuestion = false;
            QuestionBeingEdited = null;
            StateHasChanged();
        }


        protected void ConfirmDeleteQuestion(QuestionEntity question)
        {
            QuestionToDelete = question;
            ShowDeleteQuestionModal = true;
        }

        protected void CancelDeleteQuestion()
        {
            QuestionToDelete = null;
            ShowDeleteQuestionModal = false;
        }

        protected async Task DeleteQuestionConfirmed()
        {
            if (QuestionToDelete == null || Test == null)
                return;

            // 🔹 Use your service’s proper delete method
            await QuestionService.DeleteAsync(Test.RowKey, QuestionToDelete.RowKey);

            // 🔹 Remove locally
            Questions.Remove(QuestionToDelete);

            // 🔹 Update test metadata
            Test.QuestionCount = Questions.Count;
            Test.TotalPoints = (int)Questions.Sum(q => q.MaxPoints);
            await TestService.UpdateAsync(Test);

            // 🔹 Reset state
            QuestionToDelete = null;
            ShowDeleteQuestionModal = false;
        }

        protected void StartAddQuestion()
        {
            AddingQuestion = true;
            NewQuestionPrompt = string.Empty;
            NewQuestionPoints = 1;
        }

        protected void CancelNewQuestion()
        {
            AddingQuestion = false;
        }

        protected async Task SaveNewQuestion()
        {
            if (Test == null || string.IsNullOrWhiteSpace(NewQuestionPrompt))
                return;

            var newQuestion = new QuestionEntity
            {
                PartitionKey = Test.RowKey,
                RowKey = Guid.NewGuid().ToString(),
                Prompt = NewQuestionPrompt.Trim(),
                ExpectedAnswer = NewQuestionExpectedAnswer.Trim(),
                Rubric = NewQuestionRubric.Trim(),
                MaxPoints = NewQuestionPoints,
            };

            await QuestionService.AddAsync(newQuestion);
            Questions.Add(newQuestion);

            // Update test stats
            Test.QuestionCount = Questions.Count;
            Test.TotalPoints = (int)Questions.Sum(q => q.MaxPoints);
            await TestService.UpdateAsync(Test);

            // Reset UI
            AddingQuestion = false;
            NewQuestionPrompt = string.Empty;
            NewQuestionExpectedAnswer = string.Empty;
            NewQuestionRubric = string.Empty;
            NewQuestionPoints = 1;

            StateHasChanged();
        }



        protected async Task AutoSaveAsync()
        {
            if (Test == null) return;

            try
            {
                IsSaving = true;
                Test.LastModifiedDate = DateTime.UtcNow;
                await TestService.UpdateAsync(Test);

                // brief visible feedback
                StateHasChanged();
                await Task.Delay(1000);
            }
            finally
            {
                IsSaving = false;
                StateHasChanged();
            }
        }

        protected async Task ConfirmDelete()
        {
            if (Test == null)
                return;

            // Delete from Table Storage
            await TestService.DeleteAsync(Test.PartitionKey, Test.RowKey);

            // Close modal and navigate back
            ShowDeleteModal = false;
            Nav.NavigateTo("/teacher/dashboard", forceLoad: true);
        }

        protected void SwitchTab(string tab) => ActiveTab = tab;

        protected async Task PublishTest()
        {
            if (Test == null) return;

            Test.IsPublished = true;
            Test.IsOpen = false;

            await TestService.UpdateAsync(Test);
            StateHasChanged();
        }

        protected async Task UnpublishTest()
        {
            if (Test == null) return;

            Test.IsPublished = false;
            Test.IsOpen = false;

            await TestService.UpdateAsync(Test);
            StateHasChanged();
        }

        protected async Task OpenTest()
        {
            if (Test == null) return;

            Test.IsPublished = true; 
            Test.IsOpen = true;

            await TestService.UpdateAsync(Test);
            StateHasChanged();
        }

        protected async Task CloseTest()
        {
            if (Test == null) return;

            Test.IsOpen = false;

            await TestService.UpdateAsync(Test);
            StateHasChanged();
        }

        protected async Task ReopenTest()
        {
            if (Test == null) return;

            Test.IsOpen = true;

            await TestService.UpdateAsync(Test);
            StateHasChanged();
        }

        protected void GoBack() => Nav.NavigateTo("/teacher/dashboard");
    }
}
