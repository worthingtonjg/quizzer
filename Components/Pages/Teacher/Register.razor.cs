using Microsoft.AspNetCore.Components;
using quizzer.Services;
using System.ComponentModel.DataAnnotations;

namespace quizzer.Pages.Teacher
{
    public partial class Register : ComponentBase
    {
        [Inject] protected UserService UserService { get; set; } = default!;
        [Inject] protected NavigationManager Nav { get; set; } = default!;

        protected TeacherRegistrationModel teacher = new();
        protected string? Message;

        protected async Task RegisterAsync()
        {
            var success = await UserService.RegisterAsync(
                teacher.Name,
                teacher.Email,
                teacher.Password
            );

            if (!success)
            {
                Message = "A teacher with this email already exists.";
                return;
            }

            Message = "Registration successful! Redirecting...";
            await Task.Delay(1500);
            Nav.NavigateTo("/teacher/dashboard");
        }

        public class TeacherRegistrationModel
        {
            [Required]
            public string Name { get; set; } = string.Empty;

            [Required, EmailAddress]
            public string Email { get; set; } = string.Empty;

            [Required, MinLength(6)]
            public string Password { get; set; } = string.Empty;
        }
    }
}
