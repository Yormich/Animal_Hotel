using Animal_Hotel.Models.DatabaseModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Animal_Hotel.Models.ModelConfigurations
{
    public class ContractConfiguration : IEntityTypeConfiguration<Contract>
    {
        public void Configure(EntityTypeBuilder<Contract> builder)
        {
            builder.HasOne(c => c.Enclosure)
                .WithMany(e => e.Contracts)
                .HasForeignKey(c => c.EnclosureId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(c => c.Animal)
                .WithMany(a => a.Contracts)
                .HasForeignKey(c => c.AnimalId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
