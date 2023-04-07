using Animal_Hotel.Models.DatabaseModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Animal_Hotel.Models.ModelConfigurations
{
    public class ClientConfiguration : IEntityTypeConfiguration<Client>
    {
        public void Configure(EntityTypeBuilder<Client> builder)
        {
            builder.HasOne(c => c.LoginInfo)
                .WithOne(uli => uli.Client)
                .HasForeignKey<UserLoginInfo>(uli => uli.ClientId);

            builder.HasOne(c => c.Review)
                .WithOne(r => r.Client)
                .HasForeignKey<Review>(r => r.ClientId);

            builder.HasMany(c => c.Animals)
                .WithOne(a => a.Owner);
        }
    }
}
