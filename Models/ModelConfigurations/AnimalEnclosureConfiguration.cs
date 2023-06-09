﻿using Animal_Hotel.Models.DatabaseModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Animal_Hotel.Models.ModelConfigurations
{
    public class AnimalEnclosureConfiguration : IEntityTypeConfiguration<AnimalEnclosure>
    {
        public void Configure(EntityTypeBuilder<AnimalEnclosure> builder)
        {
            builder.Property(e => e.PricePerDay).HasPrecision(40, 4);

            builder.HasOne(ae => ae.Room)
                .WithMany(r => r.Enclosures)
                .HasForeignKey(ae => ae.RoomId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ae => ae.EnclosureType)
                .WithMany(et => et.Enclosures)
                .HasForeignKey(ae => ae.EnclosureTypeId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(ae => ae.AnimalType)
                .WithMany(ae => ae.Enclosures)
                .HasForeignKey(ae => ae.AnimalTypeId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(ae => ae.Contracts)
                .WithOne(c => c.Enclosure)
                .HasForeignKey(c => c.EnclosureId);

            builder.HasOne(ae => ae.Booking)
                .WithOne(b => b.Enclosure);
        }
    }
}
