using Animal_Hotel.Models.DatabaseModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Animal_Hotel.Models.ModelConfigurations
{
    public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
    {
        public void Configure(EntityTypeBuilder<Employee> builder)
        {
            builder.Property(e => e.Salary).HasPrecision(30, 3);

            builder.HasOne(e => e.LoginInfo)
                .WithOne(uli => uli.Employee)
                .HasForeignKey<UserLoginInfo>(uli => uli.EmployeeId);

            builder.HasMany(e => e.RoomEmployees)
                .WithOne(e => e.Employee);

            builder.HasMany(e => e.Rooms)
                .WithMany(r => r.Employees)
                .UsingEntity<RoomEmployee>();

            builder.HasMany(e => e.Requests)
                .WithOne(r => r.Writer);
        }
    }
}
