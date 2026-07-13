using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartBotBlazorApp.Components.Account;
using SmartBotBlazorApp.Data;
using Microsoft.AspNetCore.ResponseCompression;
using SmartBotBlazorApp.Components;
using SmartBotBlazorApp.Hubs;
using SmartBotBlazorApp;
using MudBlazor.Services;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, PersistingRevalidatingAuthenticationStateProvider>();

//builder.Services.AddDefaultIdentity<IdentityUser>() 
//    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

var connectionString = Environment.GetEnvironmentVariable("SmartBotDBConnectionString") ?? builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();
builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", builder =>
    {
        builder.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
        //.AllowCredentials();
    });
});

builder.Services.AddMudServices();

builder.Services.AddSignalR();

builder.Services.AddScoped<ImageProcessor>();

builder.Services.AddHttpClient();

builder.Services.AddScoped<MeasurementService>();

builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        ["application/octet-stream"]);
});

var app = builder.Build();

if (string.IsNullOrEmpty(app.Configuration["RobotApiKey"]) ||
    app.Configuration["RobotApiKey"]!.Length < SignalHubAccess.MinimumApiKeyLength)
{
    app.Logger.LogWarning("RobotApiKey is missing or shorter than {MinimumLength} characters; anonymous robot connections will be denied.", SignalHubAccess.MinimumApiKeyLength);
}

using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.Migrate();
    //var seedUserPass = Environment.GetEnvironmentVariable("SeedAdminPass") ?? builder.Configuration.GetValue<string>("SeedUserPass");
    //var userManager = scope.ServiceProvider.GetService<UserManager<ApplicationUser>>();
    //await SeedData.Initialize(services, seedUserPass);
}

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseResponseCompression();

app.UseCors("CorsPolicy");

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/signalhub") &&
        !SignalHubAccess.IsAllowed(
            context.User,
            app.Configuration["RobotApiKey"],
            context.Request.Query["access_token"].ToString()))
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.Response.WriteAsync("SignalR authentication required.");
        return;
    }

    await next();
});

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(SmartBotBlazorApp.Client._Imports).Assembly);

app.MapHub<SignalHub>("/signalhub");

app.MapAdditionalIdentityEndpoints();

app.Run();
