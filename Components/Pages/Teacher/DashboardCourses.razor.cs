using Microsoft.AspNetCore.Components;
using quizzer.Data.Entities;
using quizzer.Services;

namespace quizzer.Components.Pages.Teacher
{
    public partial class DashboardCourses : ComponentBase
    {
        [Inject] protected CurrentUserService CurrentUser { get; set; } = default!;
        [Inject] protected CourseService CourseService { get; set; } = default!;
        [Inject] protected TestService TestService { get; set; } = default!;

        protected bool IsLoading { get; set; } = true;
        protected string TeacherId { get; set; }
        protected string NewCourseName { get; set; } = string.Empty;
        protected List<CourseEntity> Courses { get; set; } = new();

        protected override async Task OnInitializedAsync()
        {
            await LoadCourses();
        }

        protected async Task LoadCourses()
        {
            IsLoading = true;

            var teacher = await CurrentUser.GetCurrentUserAsync();

            TeacherId = teacher.RowKey;

            Courses = await CourseService.GetByTeacherAsync(TeacherId, false); 
            IsLoading = false;
        }

        protected async Task AddCourse()
        {
            if (string.IsNullOrWhiteSpace(NewCourseName))
                return;

            var newCourse = new CourseEntity(TeacherId, NewCourseName);

            await CourseService.UpsertAsync(newCourse);

            // Clear the input box after adding
            NewCourseName = string.Empty;

            await LoadCourses();
        }

        protected async Task TryDeleteCourse(CourseEntity course)
        {
            var tests = await TestService.GetByCourseAsync(course["CourseId"]);

            if (tests.Count > 0)
            {
                // Deactivate instead of delete
                course.IsActive = false;
                await CourseService.UpsertAsync(course);
            }
            else
            {
                await CourseService.DeleteAsync(TeacherId, course["CourseId"]);
            }

            await LoadCourses();
        }

        protected async Task ActivateCourse(CourseEntity course)
        {
            course.IsActive = true;
            await CourseService.UpsertAsync(course);
            await LoadCourses();
        }
    }
}
