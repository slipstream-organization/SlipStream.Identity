using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SlipStream.Identity.Infrastructure.Identity;

namespace SlipStream.Identity.Infrastructure.Persistence.Configurations;

internal sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.HasIndex(x => x.NormalizedEmail)
            .HasDatabaseName("EmailIndex")
            .IsUnique();
    }
}