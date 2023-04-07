using Animal_Hotel.Models.DatabaseModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Animal_Hotel.Models.ModelConfigurations
{
    public class RoomEmployeeConfiguration : IEntityTypeConfiguration<RoomEmployee>
    {
        public void Configure(EntityTypeBuilder<RoomEmployee> builder)
        {
            builder.HasKey(re => new { re.RoomId, re.EmployeeId });

            builder.HasOne(re => re.Employee)
                .WithMany(e => e.RoomEmployees)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(re => re.Room)
                .WithMany(r => r.RoomEmployees)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
