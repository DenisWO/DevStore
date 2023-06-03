using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevStore.Identity.API.Data.Mappings
{
    public class RoleMapping : IEntityTypeConfiguration<IdentityRole>
    {
        public void Configure(EntityTypeBuilder<IdentityRole> builder)
        {
            builder.HasData(
                new IdentityRole()
                {
                    Name = "User",
                    NormalizedName = "USER",
                },
                new IdentityRole()
                {
                    Name = "Admin",
                    NormalizedName = "ADMINISTRATOR"
                }
            );
        }
    }
}
