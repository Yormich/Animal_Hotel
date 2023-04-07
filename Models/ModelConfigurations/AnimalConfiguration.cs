using Animal_Hotel.Models.DatabaseModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Animal_Hotel.Models.ModelConfigurations
{
    public class AnimalConfiguration : IEntityTypeConfiguration<Animal>
    {
        public void Configure(EntityTypeBuilder<Animal> builder)
        {
            builder.HasOne(a => a.Owner)
                .WithMany(c => c.Animals)
                .HasForeignKey(a => a.OwnerId);

            builder.HasOne(a => a.Booking)
                .WithOne(b => b.Animal)
                .HasForeignKey<Booking>(b => b.AnimalId);

            builder.HasOne(a => a.AnimalType)
                .WithMany(at => at.Animals)
                .HasForeignKey(a => a.TypeId);

            builder.HasMany(a => a.Contracts)
                .WithOne(c => c.Animal)
                .HasForeignKey(c => c.AnimalId);
        }
    }
}
