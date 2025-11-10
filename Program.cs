using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using quizzer.Components;
using quizzer.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

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

// TEMPORARY: Create first teacher account
using (var scope = app.Services.CreateScope())
{
    var userService = scope.ServiceProvider.GetRequiredService<UserService>();

    // Replace these with your desired login credentials
    string seedEmail = "teacher@example.com";
    string seedPassword = "Pass123!";

    var existing = await userService.GetByEmailAsync(seedEmail);
    if (existing == null)
    {
        bool success = await userService.RegisterAsync(seedEmail, seedPassword);
        if (success)
            Console.WriteLine($"✅ Seeded teacher user: {seedEmail} / {seedPassword}");
        else
            Console.WriteLine($"⚠️ Could not seed user (already exists?)");
    }
    else
    {
        Console.WriteLine($"ℹ️ User '{seedEmail}' already exists — skipping seed.");
    }
}


app.Run();
