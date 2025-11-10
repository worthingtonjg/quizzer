using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using quizzer.Components;
using quizzer.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSingleton<AccessCodeService>();
builder.Services.AddScoped<CurrentUserService>();
builder.Services.AddSingleton<QuestionService>();
builder.Services.AddSingleton<SubmissionService>();
builder.Services.AddSingleton<TestService>();
builder.Services.AddSingleton<UserService>();

// Add cookie-based authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.AccessDeniedPath = "/login";
        options.ReturnUrlParameter = "returnUrl";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.None;           
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always; 
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

await DataSeeder.SeedAsync(app.Services);

app.Run();
