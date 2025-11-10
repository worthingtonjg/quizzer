using Microsoft.AspNetCore.Components;
using quizzer.Services;

namespace quizzer.Pages
{
    public partial class Home : ComponentBase
    {
        [Inject] protected CurrentUserService CurrentUser { get; set; } = default!;
        [Inject] protected NavigationManager Nav { get; set; } = default!;

        protected bool IsLoading { get; set; } = true;
        
        protected string AccessCode { get; set; } = string.Empty;
        protected string? ErrorMessage { get; set; }
        

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                var teacher = await CurrentUser.GetCurrentUserAsync();
                if (teacher != null)
                {
                    await Task.Yield();
                    Nav.NavigateTo("/teacher/dashboard", forceLoad: true);
                }
            }

            IsLoading = false;
            StateHasChanged();
        }


        protected void GoToLogin() => Nav.NavigateTo("/teacher/dashboard");
        protected void GoToRegister() => Nav.NavigateTo("/teacher/register");

        protected async Task StartTest()
        {
            if (string.IsNullOrWhiteSpace(AccessCode))
            {
                ErrorMessage = "Please enter an access code.";
                return;
            }

            Nav.NavigateTo($"/student/{AccessCode.Trim().ToUpper()}");
        }
    }
}
