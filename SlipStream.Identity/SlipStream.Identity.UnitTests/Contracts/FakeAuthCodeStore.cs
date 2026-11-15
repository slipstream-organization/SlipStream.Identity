using SlipStream.Identity.Core.Contracts;

namespace SlipStream.Identity.UnitTests.Contracts;

internal sealed class FakeAuthCodeStore : IAuthCodeStore
{
    public FakeAuthCodeStore()
    {
        Codes = [];
    }

    public Dictionary<string, Guid> Codes { get; }

    public Task<string> CreateAsync(Guid userId, CancellationToken ct = default)
    {
        var code = Guid.NewGuid().ToString("N");
        Codes[code] = userId;
        return Task.FromResult(code);
    }

    public Task<Guid?> ConsumeAsync(string code, CancellationToken ct = default)
    {
        if (!Codes.Remove(code, out var id))
        {
            return Task.FromResult<Guid?>(null);
        }

        return Task.FromResult<Guid?>(id);
    }
}
