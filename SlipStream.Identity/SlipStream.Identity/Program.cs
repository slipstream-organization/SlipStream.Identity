using System.Globalization;
using FluentValidation;
using SlipStream.Common.Extensions;
using SlipStream.Identity.Core.Sessions;
using SlipStream.Identity.Infrastructure;
using SlipStream.Identity.Options;

ValidatorOptions.Global.LanguageManager.Culture = new CultureInfo("uk");

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddHealthCheck(
    configuration.GetConnectionString("Postgres"),
    configuration.GetConnectionString("Redis"));

builder.Services.AddMediator(typeof(Program).Assembly, typeof(RefreshCommandHandler).Assembly);
builder.Services.AddPlatformWeb();
builder.Services.AddInfrastructure(configuration);

builder.Services.Configure<FrontendOptions>(configuration.GetSection(FrontendOptions.Section));
var frontendUrl = configuration.GetSection(FrontendOptions.Section).Get<FrontendOptions>()?.Url
                  ?? new FrontendOptions().Url;

builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy
    .WithOrigins(frontendUrl.TrimEnd('/'))
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()));

var app = builder.Build();

app.UsePlatformWeb();
app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.UseHealthCheck();

app.Run();
