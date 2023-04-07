using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Animal_Hotel.Models.DatabaseModels;

namespace Animal_Hotel.Models.ModelConfigurations
{
    public class UserLoginInfoConfiguration : IEntityTypeConfiguration<UserLoginInfo>
    {
        public void Configure(EntityTypeBuilder<UserLoginInfo> builder)
        {
            builder.HasOne(uli => uli.UserType)
                .WithMany(ut => ut.UserLoginInfos)
                .HasForeignKey(uli => uli.UserTypeId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(uli => uli.Employee)
                .WithOne(e => e.LoginInfo)
                .HasForeignKey<UserLoginInfo>(uli => uli.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(uli => uli.Client)
                .WithOne(e => e.LoginInfo)
                .HasForeignKey<UserLoginInfo>(uli => uli.ClientId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
