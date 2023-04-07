﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;
using Animal_Hotel.Services;
using Animal_Hotel.Models.DatabaseModels;
using Animal_Hotel.Models.ModelConfigurations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Animal_Hotel
{
    public class AnimalHotelDbContext : DbContext
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IDbConnectionProvider _connectionProvider;


        public DbSet<UserLoginInfo> LoginInfos { get; set; }

        public DbSet<UserType> UserTypes { get; set; }

        public DbSet<Client> Clients { get; set; }

        public DbSet<Review> Reviews { get; set; }

        public DbSet<AnimalType> AnimalTypes { get; set; }

        public DbSet<Animal> Animals { get; set; }

        public DbSet<RequestStatus> RequestStatuses { get; set; }

        public DbSet<Employee> Employees { get; set; }

        public DbSet<Request> Requests { get; set; }

        public DbSet<RoomEmployee> RoomEmployees { get; set; }

        public DbSet<RoomType> RoomTypes { get; set; }

        public DbSet<Room> Rooms { get; set; }

        public DbSet<EnclosureType> EnclosureTypes { get; set; }

        public DbSet<AnimalEnclosure> Enclosures { get; set; }

        public DbSet<Booking> Bookings { get; set; }

        public DbSet<Contract> Contracts { get; set; }

        public AnimalHotelDbContext(IDbConnectionProvider provider, IHttpContextAccessor accessor) : base() 
        {
            _connectionProvider = provider;
            _contextAccessor = accessor;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.EnableSensitiveDataLogging();

            var context = _contextAccessor.HttpContext;
            optionsBuilder.UseSqlServer(_connectionProvider.GetConnection(context));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            //User Login Info
            modelBuilder.ApplyConfiguration(new UserLoginInfoConfiguration());

            //User Type
            ConfigureUserType(modelBuilder.Entity<UserType>());

            //Client Review
            ConfigureClientReview(modelBuilder.Entity<Review>());

            //Client
            modelBuilder.ApplyConfiguration(new ClientConfiguration());

            //Employee
            modelBuilder.ApplyConfiguration(new EmployeeConfiguration());

            //Request status
            ConfigureRequestStatus(modelBuilder.Entity<RequestStatus>());

            //Request
            modelBuilder.ApplyConfiguration(new RequestConfiguration());

            //RoomType
            modelBuilder.ApplyConfiguration(new RoomTypeConfiguration());

            //Room
            modelBuilder.ApplyConfiguration(new RoomConfiguration());

            //RoomEmployee
            modelBuilder.ApplyConfiguration(new RoomEmployeeConfiguration());

            //Enclosure type
            ConfigureEnclosureType(modelBuilder.Entity<EnclosureType>());

            //Animal Type
            ConfigureAnimalType(modelBuilder.Entity<AnimalType>());

            //Animal Enclosure
            modelBuilder.ApplyConfiguration(new AnimalEnclosureConfiguration());

            //Booking
            ConfigureBooking(modelBuilder.Entity<Booking>());

            //Animal
            modelBuilder.ApplyConfiguration(new AnimalConfiguration());

            //Contract
            modelBuilder.ApplyConfiguration(new ContractConfiguration());
        }


        //for small configurations 
        private static void ConfigureUserType(EntityTypeBuilder<UserType> userTypeBuilder)
        {
            userTypeBuilder.HasMany(ut => ut.UserLoginInfos)
                .WithOne(uli => uli.UserType);

            userTypeBuilder.HasOne(ut => ut.RoomType)
                .WithOne(rt => rt.PreferredEmployeeType)
                .HasForeignKey<RoomType>(rt => rt.PreferredEmployeeTypeId);
        }

        private static void ConfigureRequestStatus(EntityTypeBuilder<RequestStatus> rsBuilder)
        {
            rsBuilder.HasMany(rs => rs.Requests)
                .WithOne(r => r.Status);
        }

        private static void ConfigureEnclosureType(EntityTypeBuilder<EnclosureType> etBuilder)
        {
            etBuilder.HasMany(et => et.Enclosures)
                .WithOne(ae => ae.EnclosureType);
        }

        private static void ConfigureClientReview(EntityTypeBuilder<Review> crBuilder)
        {
            crBuilder.Property(cr => cr.WritingDate).HasDefaultValueSql("(GETDATE())");

            crBuilder.HasOne(cr => cr.Client)
                .WithOne(c => c.Review)
                .HasForeignKey<Review>(cr => cr.ClientId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        private static void ConfigureAnimalType(EntityTypeBuilder<AnimalType> atBuilder)
        {
            atBuilder.HasMany(at => at.Enclosures)
                .WithOne(e => e.AnimalType);

            atBuilder.HasMany(at => at.Animals)
                .WithOne(a => a.AnimalType);
        }

        private static void ConfigureBooking(EntityTypeBuilder<Booking> bBuilder)
        {
            bBuilder.HasOne(b => b.Enclosure)
                .WithMany(e => e.Bookings)
                .HasForeignKey(b => b.EnclosureId)
                .OnDelete(DeleteBehavior.NoAction);

            bBuilder.HasOne(b => b.Animal)
                .WithOne(a => a.Booking)
                .HasForeignKey<Booking>(b => b.AnimalId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}