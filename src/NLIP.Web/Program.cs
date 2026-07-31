using Microsoft.AspNetCore.Components.Authorization;
using NLIP.Web.Components;
using NLIP.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped<AuthSessionService>();
builder.Services.AddScoped<AuthenticationStateProvider, NlipAuthStateProvider>();
builder.Services.AddAuthorizationCore();
builder.Services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationPolicyProvider, WebPermissionPolicyProvider>();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddHttpClient<NlipApiClient>(client =>
{
    var apiBaseUrl = builder.Configuration["Api:BaseUrl"] ?? "https://localhost:7443/";
    client.BaseAddress = new Uri(apiBaseUrl);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
