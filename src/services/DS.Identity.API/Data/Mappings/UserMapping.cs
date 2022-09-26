using DS.Identity.API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DS.Identity.API.Data.Mappings
{
    public class UserMapping : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(u => u.Id);
            builder.Property(u => u.PasswordHash).IsRequired().HasColumnType("varchar(256)");
            builder.Property(u => u.PasswordSaltHash).IsRequired().HasColumnType("varchar(256)");

            builder.ToTable("Users");
        }
    }
}
