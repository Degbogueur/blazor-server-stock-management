using MudBlazor.Services;
using StockManagement.Components;
using StockManagement.Components.Account;
using StockManagement.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDatabaseConnection(builder.Configuration);
builder.Services.AddDependencyInjectionContainer();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddAuthenticationServices();

builder.Services.AddMudServices();

builder.Services.AddSettings(builder.Configuration);

builder.Services.AddClaimsPrincipalFactory();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

await app.AddDefaultRolesAsync();
await app.AddAdminUserAsync();

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapAdditionalIdentityEndpoints();

app.Run();
