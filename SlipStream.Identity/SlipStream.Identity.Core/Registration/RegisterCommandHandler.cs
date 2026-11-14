using MediatR;
using SlipStream.Identity.Core.Contracts;
using SlipStream.Identity.Domain.Users;

namespace SlipStream.Identity.Core.Registration;

public sealed record RegisterCommand(string userName, string email, string password) : IRequest<Guid>;

public class RegisterCommandHandler(IAccountService accounts, IUserRepository users)
    : IRequestHandler<RegisterCommand, Guid>
{
    public async Task<Guid> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var id = await accounts.CreateAsync(request.userName, request.email, request.password, cancellationToken);
        
        var profile = User.Create(id, request.email, request.userName);
        await users.CreateAsync(profile, cancellationToken);

        return id;
    }
}
