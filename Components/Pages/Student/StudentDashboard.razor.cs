using Microsoft.AspNetCore.Components;
using quizzer.Data.Entities;
using quizzer.Services;
using System.Runtime.InteropServices;

namespace quizzer.Components.Pages.Student
{
    public partial class StudentDashboard : ComponentBase
    {
        [Parameter]
        public string StudentAccessCode { get; set; } = string.Empty;

        [Inject] public StudentService StudentService { get; set; } = default!;
        [Inject] public CourseService CourseService { get; set; } = default!;
        [Inject] public ClassPeriodService ClassPeriodService { get; set; } = default!;
        [Inject] public TestService TestService { get; set; } = default!;
        [Inject] public SubmissionService SubmissionService { get; set; } = default!;
        [Inject] public NavigationManager Nav { get; set; } = default!;

        protected bool IsLoading { get; set; } = true;

        protected List<StudentTestViewModel> NeedsCompletion { get; set; } = new();
        protected List<StudentTestViewModel> Submitted { get; set; } = new();
        protected List<StudentTestViewModel> Graded { get; set; } = new();


        protected override async Task OnInitializedAsync()
        {
            try
            {
                // Load the student which gives us the teacher ID
                var student = await StudentService.GetByAccessCodeAsync(StudentAccessCode);
                
                if (student == null)
                {
                    IsLoading = false;
                    return;
                }

                // Load the courses and periods for the teacher
                var courses = await CourseService.GetByTeacherAsync(student.TeacherId);
                var periods = await ClassPeriodService.GetByTeacherAsync(student.TeacherId);

                var period = periods.FirstOrDefault(p => p.RowKey == student["ClassPeriodId"]);
                var course = courses.FirstOrDefault(c => c.RowKey == period["CourseId"]);

                // Load all test attempts for the student
                var submissions = await SubmissionService.GetSubmissionsForStudentAsync(student.RowKey);

                // Load all the tests for the course
                var tests = await TestService.GetByCourseAsync(course.RowKey);

                var submissionsByTest = submissions.ToDictionary(s => s.PartitionKey, s => s);

                // Build test view models
                foreach (var test in tests.OrderBy(t => t.Title))
                {
                    submissionsByTest.TryGetValue(test.RowKey, out var sub);

                    var vm = new StudentTestViewModel
                    {
                        TestId = test.RowKey,
                        Title = test.Title,
                        Description = test.Description,
                        IsOpen = test.IsOpen,
                        HasSubmission = sub != null,
                        IsGraded = sub?.Graded ?? false,
                        Score = sub?.Score
                    };

                    // Category logic
                    if (vm.IsOpen && !vm.HasSubmission)
                    {
                        // Needs to be completed
                        NeedsCompletion.Add(vm);
                    }
                    else if (vm.HasSubmission && !vm.IsGraded)
                    {
                        // Submitted but not yet graded
                        Submitted.Add(vm);
                    }
                    else if (vm.HasSubmission && vm.IsGraded)
                    {
                        // Graded
                        Graded.Add(vm);
                    }
                }

            }
            finally
            {
                IsLoading = false;
            }
        }

        protected void GoToTest(string testId)
        {
            Nav.NavigateTo($"/student/{StudentAccessCode}/test/{testId}");
        }

        public class StudentTestViewModel
        {
            public string TestId { get; set; } = string.Empty;
            public string Title { get; set; } = string.Empty;
            public string Description { get; set; }
            public bool IsOpen { get; set; }
            public bool IsClosed { get; set; }
            public bool HasSubmission { get; set; }
            public bool IsGraded { get; set; }
            public double? Score { get; set; }
        }
    }
}
