using Microsoft.AspNetCore.Identity;
using SlipStream.Common.Extensions;
using SlipStream.Identity.Infrastructure;
using SlipStream.Identity.Infrastructure.Identity;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

builder.Services.AddHealthCheck(
    configuration.GetConnectionString("Postgres"), 
    configuration.GetConnectionString("Redis"));

builder.Services.AddMediator(typeof(Program).Assembly);
builder.Services.AddPlatformWeb();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UsePlatformWeb();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.UseHealthCheck();

app.Run();
