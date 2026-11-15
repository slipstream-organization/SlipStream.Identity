using SlipStream.Common.Events;
using SlipStream.Common.IdentityEvents;

namespace SlipStream.Identity.Core.LapAnalyze;

public class LapAnalyzedHandler : IIntegrationEventHandler<UserProfileChanged>
{
    public Task Handle(UserProfileChanged @event, CancellationToken ct)
    {
        
    }
}