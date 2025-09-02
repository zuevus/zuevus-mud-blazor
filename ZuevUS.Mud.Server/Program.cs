using Microsoft.AspNetCore.Authentication.JwtBearer;
using MudBlazor.Services;
using ZuevUS.Mud.Client.Pages;
using ZuevUS.Mud.Server.Components;

var builder = WebApplication.CreateBuilder(args);


// Add MudBlazor services
builder.Services.AddMudServices();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.Authority = "https://your-openid-provider"; // Replace with your OpenID provider
    options.Audience = "your-api-audience";
    options.RequireHttpsMetadata = false; // for dev only
});

builder.Services.AddAuthorization();
builder.Services.AddScoped<CustomAuthStateProvider>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(ZuevUS.Mud.Client._Imports).Assembly);

app.Run();
