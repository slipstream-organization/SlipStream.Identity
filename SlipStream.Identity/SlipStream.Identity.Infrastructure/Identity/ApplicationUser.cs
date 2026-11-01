using Microsoft.AspNetCore.Identity;
using SlipStream.Identity.Domain.Users;

namespace SlipStream.Identity.Infrastructure.Identity;
public sealed class ApplicationUser : IdentityUser<Guid>
{
    public User Profile { get; set; }
}
