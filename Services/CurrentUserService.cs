using Microsoft.AspNetCore.Components.Authorization;
using quizzer.Models;
using quizzer.Models.YourAppNamespace.Models;
using quizzer.Services;
using System.Security.Claims;

namespace quizzer.Services
{
    public class CurrentUserService
    {
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly UserService _userService;

        private UserEntity? _cachedUser;

        public CurrentUserService(AuthenticationStateProvider authStateProvider, UserService userService)
        {
            _authStateProvider = authStateProvider;
            _userService = userService;
        }

        public async Task<UserEntity?> GetCurrentUserAsync()
        {
            if (_cachedUser != null)
                return _cachedUser;

            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            var principal = authState.User;

            if (principal?.Identity is null || !principal.Identity.IsAuthenticated)
                return null;

            var email = principal.Identity.Name
                        ?? principal.FindFirstValue(ClaimTypes.Email)
                        ?? principal.FindFirstValue("email");

            if (string.IsNullOrWhiteSpace(email))
                return null;

            _cachedUser = await _userService.GetByEmailAsync(email);
            return _cachedUser;
        }

        public async Task<string?> GetTeacherIdAsync()
        {
            var user = await GetCurrentUserAsync();
            return user?.RowKey;
        }

        public async Task<string?> GetEmailAsync()
        {
            var user = await GetCurrentUserAsync();
            return user?.Email;
        }
    }
}
