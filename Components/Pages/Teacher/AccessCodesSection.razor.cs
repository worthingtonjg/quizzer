using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using quizzer.Models;
using quizzer.Services;

namespace quizzer.Pages.Teacher
{
    public partial class AccessCodesSection : ComponentBase
    {
        [Parameter] public string TestId { get; set; } = string.Empty;

        [Inject] protected IJSRuntime JS { get; set; } = default!;
        [Inject] protected CurrentUserService CurrentUser { get; set; } = default!;
        [Inject] protected AccessCodeService AccessCodeService { get; set; } = default!;
        [Inject] protected NavigationManager Nav { get; set; } = default!;

        protected string? TeacherId { get; set; }
        protected string? TeacherEmail { get; set; }
        protected List<AccessCodeEntity> Codes { get; set; } = new();
        protected bool IsLoading { get; set; } = true;
        protected bool IsGenerating { get; set; } = false;
        protected int GenerateCount { get; set; } = 30;

        protected override async Task OnInitializedAsync()
        {
            var teacher = await CurrentUser.GetCurrentUserAsync();
            if (teacher == null)
            {
                Nav.NavigateTo("/login");
                return;
            }

            TeacherId = teacher.RowKey;
            TeacherEmail = teacher.Email;


            await LoadCodesAsync();
        }

        protected async Task LoadCodesAsync()
        {
            IsLoading = true;
            Codes = await AccessCodeService.GetByTestAsync(TestId);
            IsLoading = false;
        }

        protected async Task GenerateCodes()
        {
            if (GenerateCount <= 0) return;

            IsGenerating = true;
            await AccessCodeService.GenerateAsync(TeacherId, TestId, GenerateCount);
            await LoadCodesAsync();
            IsGenerating = false;
        }

        protected string BuildLink(AccessCodeEntity code)
            => $"{Nav.BaseUri}student/{code.RowKey}";

        protected async Task CopyLink(string link)
        {
            await JS.InvokeVoidAsync("navigator.clipboard.writeText", link);
        }

        protected async Task CopyAllLinks()
        {
            if (Codes.Count == 0) return;

            var allLinks = string.Join(Environment.NewLine, Codes.Select(BuildLink));
            await JS.InvokeVoidAsync("navigator.clipboard.writeText", allLinks);
        }

    }
}
