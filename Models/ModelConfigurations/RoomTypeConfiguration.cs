using Animal_Hotel.Models.DatabaseModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Identity.Client;

namespace Animal_Hotel.Models.ModelConfigurations
{

    public class RoomTypeConfiguration : IEntityTypeConfiguration<RoomType>
    {
        public void Configure(EntityTypeBuilder<RoomType> builder)
        {
            builder.HasOne(rt => rt.PreferredEmployeeType)
                .WithOne(ut => ut.RoomType)
                .HasForeignKey<RoomType>(rt => rt.PreferredEmployeeTypeId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(rt => rt.Rooms)
                .WithOne(r => r.RoomType);
        }
    }
}
