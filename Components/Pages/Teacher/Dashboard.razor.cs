using Microsoft.AspNetCore.Components;
using quizzer.Services;

namespace quizzer.Components.Pages.Teacher
{
    public partial class Dashboard : ComponentBase
    {
        [Inject] protected CurrentUserService CurrentUser { get; set; } = default!;
        [Inject] protected NavigationManager Nav { get; set; } = default!;

        public enum EnumDashboardTab
        {
            Tests,
            Periods,
            Courses
        }

        protected string TeacherEmail { get; set; }
        protected string TeacherId { get; set; }
        protected EnumDashboardTab ActiveTab { get; set; } = EnumDashboardTab.Tests;

        protected override async Task OnInitializedAsync()
        {
            var teacher = await CurrentUser.GetCurrentUserAsync();
            if (teacher == null)
            {
                Nav.NavigateTo("/login");
                return;
            }
        }

        protected void SwitchTab(EnumDashboardTab tab)
        {
            ActiveTab = tab;
        }
    }
}
