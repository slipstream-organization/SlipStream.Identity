using SlipStream.Common;
using SlipStream.Common.Extensions;
using SlipStream.Identity.Core.LapAnalyze;
using SlipStream.Identity.Infrastructure;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddMediator(typeof(LapAnalyzedHandler).Assembly);
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddMessaging(builder.Configuration, "identity", events =>
{
    events.Handle<LapAnalyzed, LapAnalyzedHandler>();
});

builder.Build().Run();