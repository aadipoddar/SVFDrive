using Scalar.AspNetCore;
using SVFDriveLibrary.DataAccess;

SqlDataAccess.SetupConfiguration();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
	app.MapScalarApiReference();
}

app.MapGet("/", () => "File Explorer API");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
