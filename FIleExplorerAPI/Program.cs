using FileExplorerAPI.Services;
using SVFDriveLibrary.DataAccess;

SqlDataAccess.SetupConfiguration();

var builder = WebApplication.CreateBuilder(args);

// Allow TB-scale upload bodies. Per-endpoint validation still applies.
builder.WebHost.ConfigureKestrel(o => o.Limits.MaxRequestBodySize = null);

builder.Services.AddCors(o => o.AddDefaultPolicy(p => p
	.AllowAnyOrigin()
	.AllowAnyHeader()
	.AllowAnyMethod()
	.WithExposedHeaders("Content-Disposition")));

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHostedService<RelayConnectionService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
}

app.MapGet("/", () => "File Explorer API");

app.UseCors();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
