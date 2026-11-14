using System.Globalization;
using FluentValidation;
using SlipStream.Common.Extensions;
using SlipStream.Identity.Core.Registration;
using SlipStream.Identity.Infrastructure;

ValidatorOptions.Global.LanguageManager.Culture = new CultureInfo("uk");

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddHealthCheck(
    configuration.GetConnectionString("Postgres"),
    configuration.GetConnectionString("Redis"));

builder.Services.AddMediator(typeof(Program).Assembly, typeof(RegisterCommandHandler).Assembly);
builder.Services.AddPlatformWeb();
builder.Services.AddInfrastructure(configuration);

var app = builder.Build();

app.UsePlatformWeb();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.UseHealthCheck();

app.Run();
