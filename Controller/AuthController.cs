using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using quizzer.Services;

namespace quizzer.Controllers
{
    [Route("auth")]
    public class AuthController : Controller
    {
        private readonly UserService _userService;

        public AuthController(UserService userService)
        {
            _userService = userService;
        }

        // ✅ GET /auth/login?returnUrl=...
        [HttpGet("login")]
        public IActionResult LoginPage([FromQuery] string returnUrl = "/")
        {
            // Redirect to the Blazor login page, preserving returnUrl
            return Redirect($"/login?returnUrl={Uri.EscapeDataString(returnUrl ?? "/")}");
        }

        // ✅ POST /auth/login?returnUrl=...
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromForm] LoginRequest request, [FromQuery] string returnUrl = "/")
        {
            var user = await _userService.ValidateCredentialsAsync(request.Email, request.Password);
            if (user == null)
                return Redirect($"/login?error=invalid&returnUrl={Uri.EscapeDataString(returnUrl ?? "/")}");

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, user.Email),
                new(ClaimTypes.Role, user.PartitionKey)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            // Redirect to original destination if it's safe and local
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return Redirect("/");
        }

        [HttpGet("logout")]
        public async Task<IActionResult> Logout([FromQuery] string returnUrl = "/login")
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect(returnUrl ?? "/login");
        }

        [HttpGet("whoami")]
        public IActionResult WhoAmI()
        {
            return Ok(User?.Identity?.Name ?? "anonymous");
        }
    }

    // ✅ For [FromForm] binding
    public record LoginRequest(string Email, string Password);
}
