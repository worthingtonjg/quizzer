using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using quizzer.Data.Entities;
using quizzer.Services;

namespace quizzer.Components.Pages.Teacher
{
    public partial class DashboardPeriods : ComponentBase
    {
        [Inject] protected CurrentUserService CurrentUser { get; set; } = default!;
        [Inject] protected ClassPeriodService ClassPeriodService { get; set; } = default!;
        [Inject] protected StudentService StudentService { get; set; } = default!;
        [Inject] protected CourseService CourseService { get; set; } = default!;
        [Inject] protected NavigationManager Nav { get; set; } = default!;
        [Inject] protected IJSRuntime JS { get; set; } = default!;

        protected bool IsLoading { get; set; } = true;
        protected bool IsGenerating { get; set; }
        protected string TeacherId { get; set; } = string.Empty;
        protected string SelectedCourseId { get; set; } = string.Empty;
        protected string SelectedPeriodId { get; set; } = string.Empty;
        protected string NewPeriodName { get; set; } = string.Empty;
        protected List<CourseEntity> Courses { get; set; } = new();
        protected List<ClassPeriodEntity> Periods { get; set; } = new();
        protected Dictionary<string, List<StudentEntity>> StudentsByPeriod { get; set; } = new();
        protected int StudentsToGenerate { get; set; } = 30;
        protected bool ShowDeleteStudentModal { get; set; }
        protected string? StudentToDeleteAccessCode { get; set; }
        protected string? StudentToDeletePeriodId { get; set; }

        protected bool ShowDeleteAllModal { get; set; }
        protected void ShowDeleteAllConfirm() => ShowDeleteAllModal = true;
        protected void HideDeleteAllConfirm() => ShowDeleteAllModal = false;

        protected override async Task OnInitializedAsync()
        {
            await LoadInitialData();
        }

        private async Task LoadInitialData()
        {
            var teacher = await CurrentUser.GetCurrentUserAsync();
            TeacherId = teacher.RowKey;

            Courses = await CourseService.GetByTeacherAsync(TeacherId);

            // Auto-select first course if available
            if (Courses.Any())
                SelectedCourseId = Courses.First()["CourseId"];

            await LoadPeriods();
        }

        protected async Task OnCourseChanged()
        {
            SelectedPeriodId = string.Empty;
            await LoadPeriods();
        }

        protected Task SelectPeriod(string periodId)
        {
            SelectedPeriodId = periodId;
            StudentsToGenerate = 30;
            return Task.CompletedTask;
        }

        protected async Task LoadPeriods()
        {
            IsLoading = true;

            if (string.IsNullOrWhiteSpace(SelectedCourseId)) return;

            Periods = await ClassPeriodService.GetByCourseAsync(SelectedCourseId);

            StudentsByPeriod.Clear();
            foreach (var p in Periods)
            {
                StudentsByPeriod[p["ClassPeriodId"]] = await StudentService.GetByClassPeriodAsync(p["ClassPeriodId"]);
            }

            SelectedPeriodId = Periods.FirstOrDefault()?.RowKey;

            IsLoading = false;
        }

        protected async Task AddPeriod()
        {
            if (string.IsNullOrWhiteSpace(NewPeriodName))
                return;

            IsLoading = true;

            await ClassPeriodService.CreateAsync(SelectedCourseId, TeacherId, NewPeriodName);

            NewPeriodName = string.Empty;
            await LoadPeriods();
        }

        protected async Task AddStudent(string classPeriodId)
        {
            await StudentService.CreateAsync(classPeriodId, TeacherId);
            await LoadPeriods();
        }

        protected async Task DeletePeriod(ClassPeriodEntity period)
        {
            if (StudentsByPeriod.TryGetValue(period["ClassPeriodId"], out var students) &&
                students.Count > 0)
            {
                // Prevent deletion if students exist
                return;
            }

            IsLoading = true;
            await ClassPeriodService.DeleteAsync(SelectedCourseId, period["ClassPeriodId"]);
            await LoadPeriods();
        }

        protected async Task GenerateStudents(string classPeriodId)
        {
            if (StudentsToGenerate < 1)
                return;

            IsGenerating = true;

            for (int i = 0; i < StudentsToGenerate; i++)
                await StudentService.CreateAsync(classPeriodId, TeacherId);

            // Reset to 1 for convenience
            StudentsToGenerate = 1;

            await LoadPeriods();

            IsGenerating = false;
        }

        protected string BuildLink(string accessCode)
           => $"{Nav.BaseUri}student/{accessCode}";

        protected async Task DeleteAllStudents()
        {
            IsGenerating = true;
            HideDeleteAllConfirm();
            await StudentService.DeleteAllByClassPeriodAsync(SelectedPeriodId);
            await LoadPeriods();
            IsGenerating = false;
        }

        protected async Task CopyLink(string link)
        {
            await JS.InvokeVoidAsync("navigator.clipboard.writeText", link);
        }

        protected async Task CopyAllLinks()
        {
            if (string.IsNullOrWhiteSpace(SelectedPeriodId))
                return;

            if (!StudentsByPeriod.TryGetValue(SelectedPeriodId, out var students))
                return;

            if (students.Count == 0)
                return;

            // Build newline-separated list of links
            var allLinks = string.Join("\n",
                students.Select(s => BuildLink(s.RowKey)));

            // Copy to clipboard
            await JS.InvokeVoidAsync("navigator.clipboard.writeText", allLinks);
        }

        protected void ConfirmDeleteStudent(string periodId, string accessCode)
        {
            StudentToDeletePeriodId = periodId;
            StudentToDeleteAccessCode = accessCode;
            ShowDeleteStudentModal = true;
        }

        protected void HideDeleteStudentConfirm()
        {
            ShowDeleteStudentModal = false;
            StudentToDeleteAccessCode = null;
            StudentToDeletePeriodId = null;
        }

        protected async Task DeleteStudentConfirmed()
        {
            if (StudentToDeletePeriodId != null && StudentToDeleteAccessCode != null)
            {
                await StudentService.DeleteAsync(StudentToDeletePeriodId, StudentToDeleteAccessCode);
            }

            HideDeleteStudentConfirm();
            await LoadPeriods();
        }


    }
}
