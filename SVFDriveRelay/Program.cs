using SVFDriveRelay;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR(options =>
{
    options.MaximumReceiveMessageSize = 32 * 1024 * 1024; // 32 MB
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
});

var app = builder.Build();

app.MapHub<RelayHub>("/relay");
app.MapGet("/", () => "SVFDrive Relay is running.");

app.Run();
