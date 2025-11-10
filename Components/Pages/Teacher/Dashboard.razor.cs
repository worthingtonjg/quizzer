using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using quizzer.Models;
using quizzer.Services;

namespace quizzer.Pages.Teacher
{
    public partial class Dashboard : ComponentBase
    {
        [Inject] protected CurrentUserService CurrentUser { get; set; } = default!;
        [Inject] protected NavigationManager Nav { get; set; } = default!;
        [Inject] protected UserService UserService { get; set; } = default!;
        [Inject] protected TestService TestService { get; set; } = default!;

        protected bool IsLoading { get; set; } = true;
        protected string? TeacherEmail { get; set; }
        protected string? TeacherId { get; set; }

        protected List<TestEntity> AllTests { get; set; } = new();
        protected List<TestEntity> FilteredTests { get; set; } = new();
        protected string SelectedCategory { get; set; } = "Draft";

        protected readonly string[] Categories = new[] { "Draft", "Published" };

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

            var tests = await TestService.GetByTeacherAsync(TeacherId);
            AllTests = tests.OrderByDescending(t => t.LastModifiedDate).ToList();

            FilterTests();
            IsLoading = false;
        }

        public void SelectCategory(string category)
        {
            SelectedCategory = category;
            FilterTests();
        }

        private void FilterTests()
        {
            FilteredTests = SelectedCategory switch
            {
                "Draft" => AllTests
                    .Where(t => !t.IsPublished)
                    .OrderByDescending(t => t.LastModifiedDate)
                    .ToList(),

                "Published" => AllTests
                    .Where(t => t.IsPublished)
                    .OrderBy(t => t.IsOpen ? 0 : 1) // open tests first, closed tests last
                    .ThenByDescending(t => t.LastModifiedDate)
                    .ToList(),

                _ => new List<TestEntity>()
            };
        }

        protected void ManageTest(string testId)
        {
            Nav.NavigateTo($"/teacher/tests/{testId}");
        }

        protected async Task CreateNewTest()
        {
            if (string.IsNullOrEmpty(TeacherId))
                return; // should never happen if user is logged in

            var newTest = new TestEntity
            {
                PartitionKey = TeacherId,   // teacher’s ID
                Title = "Untitled Test",
                Description = "",
                Instructions = "",
                IsPublished = false,
                IsOpen = false,
                QuestionCount = 0,
                TotalPoints = 0,
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow
            };

            await TestService.AddAsync(newTest);

            // Redirect to the Manage Test page for this new test
            Nav.NavigateTo($"/teacher/tests/{newTest.RowKey}");
        }

    }
}
