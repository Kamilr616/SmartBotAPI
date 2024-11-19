using SmartBotWebAPI;
//using SignalRChat.Hubs;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "SmartBot API",
        Version = "v1",
        Description = "API for processing images and interacting with the SmartBot system"
    });
});
builder.Services.AddSignalR();
builder.Services.AddScoped<ImageProcessor>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", builder =>
    {
        builder.WithOrigins("*") // Adres Blazor WebAssembly
            .AllowAnyHeader()
            .AllowAnyMethod();
        //.AllowCredentials();
    });
});

//builder.Logging.ClearProviders();
builder.Logging.AddDebug();
builder.Logging.AddConsole();


var app = builder.Build();

// Enable WebSocket support
var webSocketOptions = new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromSeconds(120) // Keep alive interval
};


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerUI();
    app.Use(async (context, next) =>
    {
        var logger = app.Logger;
        logger.LogInformation("Request: {Method} {Path}", context.Request.Method, context.Request.Path);
        await next.Invoke();
        logger.LogInformation("Response: {StatusCode}", context.Response.StatusCode);
    });
}
app.UseSwagger();

app.UseCors("CorsPolicy");

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseWebSockets(webSocketOptions);

app.MapHub<SignalHub>("/signalhub");

app.MapControllers();

app.Run();
