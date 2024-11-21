using SmartBotWebAPI;
//using Microsoft.AspNetCore.ResponseCompression;


var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddSignalR(options =>
{
    // Configure options for the SignalR hub
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(60); // Timeout for clients
    options.KeepAliveInterval = TimeSpan.FromSeconds(45); // Interval for sending "ping" messages to keep connection alive
    options.HandshakeTimeout = TimeSpan.FromSeconds(30); // Timeout for the handshake
    options.MaximumParallelInvocationsPerClient = 4; // Max number of simultaneous invocations per client
    //options.MaximumReceiveMessageSize = 32 * 1024; // Maximum size for a message (32 KB)
    options.EnableDetailedErrors = true; // Enable detailed error messages for debugging
});
//builder.Services.AddResponseCompression(opts =>
//{
//    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
//        ["application/octet-stream"]);
//});
builder.Services.AddScoped<ImageProcessor>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", builder =>
    {
        builder.WithOrigins("*") // Adres Blazor WebAssembly?
            .AllowAnyHeader()
            .AllowAnyMethod();
        //.AllowCredentials();
    });
});

//builder.Logging.ClearProviders();
builder.Logging.AddDebug();
builder.Logging.AddConsole();

var app = builder.Build();

var webSocketOptions = new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromSeconds(120) // Keep alive interval
};

//app.UseResponseCompression();

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
