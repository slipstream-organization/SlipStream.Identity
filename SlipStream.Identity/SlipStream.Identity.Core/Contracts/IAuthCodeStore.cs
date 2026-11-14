namespace SlipStream.Identity.Core.Contracts;

public interface IAuthCodeStore
{
    Task<string> CreateAsync(Guid userId, CancellationToken ct = default);

    Task<Guid?> ConsumeAsync(string code, CancellationToken ct = default);
}
