using Microsoft.AspNetCore.Components;
using quizzer.Data.Entities;
using quizzer.Services;

namespace quizzer.Components.Pages.Teacher
{
    public partial class DashboardTests : ComponentBase
    {
        [Inject] protected CurrentUserService CurrentUser { get; set; } = default!;
        [Inject] protected NavigationManager Nav { get; set; } = default!;
        [Inject] protected UserService UserService { get; set; } = default!;
        [Inject] protected TestService TestService { get; set; } = default!;
        [Inject] protected StudentService PeriodService { get; set; } = default!;
        [Inject] protected CourseService CourseService { get; set; } = default!;
        [Inject] protected ClassPeriodService ClassPeriodService { get; set; } = default!;

        protected bool IsLoading { get; set; } = true;
        public string TeacherEmail { get; set; }
        public string TeacherId { get; set; }
        protected List<CourseEntity> Courses { get; set; } = new();
        protected string SelectedCourseId { get; set; }
        protected List<TestEntity> AllTests { get; set; } = new();

        protected override async Task OnInitializedAsync()
        {
            IsLoading = true;

            var teacher = await CurrentUser.GetCurrentUserAsync();
            if(teacher == null)
            {
                return;
            }
            TeacherId = teacher.RowKey;
            TeacherEmail = teacher.Email;

            Courses = await CourseService.GetByTeacherAsync(TeacherId);

            if (Courses.Any())
            {
                SelectedCourseId = Courses.First().RowKey;
                await LoadTestsAsync();
            }

            IsLoading = false;
        }

        private async Task LoadTestsAsync()
        {
            AllTests.Clear();

            if (!string.IsNullOrEmpty(SelectedCourseId))
            {
                AllTests = await TestService.GetByCourseAsync(SelectedCourseId);
            }
        }

        public async Task CourseChanged(ChangeEventArgs e)
        {
            SelectedCourseId = e.Value.ToString();
            await LoadTestsAsync();
        }

        protected void ViewSubmissions(string testId)
        {
            Nav.NavigateTo($"/teacher/submissions/{testId}");
        }

        protected void EditTest(string testId)
        {
            Nav.NavigateTo($"/teacher/edit/{testId}");
        }

        protected async Task CreateNewTest()
        {
            if (string.IsNullOrEmpty(SelectedCourseId))
                return;

            var newTest = new TestEntity
            (
                courseId: SelectedCourseId,
                title: "Untitled Test"
            );

            await TestService.AddAsync(newTest);

            // Redirect to the Manage Test page for this new test
            Nav.NavigateTo($"/teacher/edit/{newTest.RowKey}");
        }


    }
}
