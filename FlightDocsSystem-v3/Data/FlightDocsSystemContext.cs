using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace FlightDocsSystem_v3.Data
{
    public partial class FlightDocsSystemContext : DbContext
    {
        public FlightDocsSystemContext()
        {
        }

        public FlightDocsSystemContext(DbContextOptions<FlightDocsSystemContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Airport> Airports { get; set; } = null!;
        public virtual DbSet<Document> Documents { get; set; } = null!;
        public virtual DbSet<Flight> Flights { get; set; } = null!;
        public virtual DbSet<User> Users { get; set; } = null!;
        public virtual DbSet<UserFlight> UserFlights { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("Data Source=PHAMNGOCMANH;Initial Catalog=FlightDocsSystem;Integrated Security=True;TrustServerCertificate=True");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Airport>(entity =>
            {
                entity.Property(e => e.Address).HasMaxLength(50);

                entity.Property(e => e.AirportCode).HasMaxLength(50);

                entity.Property(e => e.AirportLevel).HasMaxLength(2);

                entity.Property(e => e.AirportName).HasMaxLength(50);

                entity.Property(e => e.RunwayType).HasMaxLength(50);
            });

            modelBuilder.Entity<Document>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.DocumentType).HasMaxLength(50);

                entity.Property(e => e.Title).HasMaxLength(50);

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

                entity.HasOne(d => d.CreatedByNavigation)
                    .WithMany(p => p.DocumentCreatedByNavigations)
                    .HasForeignKey(d => d.CreatedBy)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Documents_Users");

                entity.HasOne(d => d.Flight)
                    .WithMany(p => p.Documents)
                    .HasForeignKey(d => d.FlightId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Documents_Flights");

                entity.HasOne(d => d.UpdatedByNavigation)
                    .WithMany(p => p.DocumentUpdatedByNavigations)
                    .HasForeignKey(d => d.UpdatedBy)
                    .HasConstraintName("FK_Documents_Users1");
            });

            modelBuilder.Entity<Flight>(entity =>
            {
                entity.Property(e => e.AircraftType).HasMaxLength(50);

                entity.Property(e => e.ArrivalAirportId).HasColumnName("ArrivalAirportID");

                entity.Property(e => e.DepartureAirportId).HasColumnName("DepartureAirportID");

                entity.Property(e => e.DepatureDate).HasColumnType("datetime");

                entity.Property(e => e.Status).HasMaxLength(50);

                entity.HasOne(d => d.ArrivalAirport)
                    .WithMany(p => p.FlightArrivalAirports)
                    .HasForeignKey(d => d.ArrivalAirportId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Flights_Airports1");

                entity.HasOne(d => d.DepartureAirport)
                    .WithMany(p => p.FlightDepartureAirports)
                    .HasForeignKey(d => d.DepartureAirportId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Flights_Airports");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.Email).HasMaxLength(50);

                entity.Property(e => e.PhoneNumber)
                    .HasMaxLength(10)
                    .IsFixedLength();

                entity.Property(e => e.Role).HasMaxLength(50);

                entity.Property(e => e.Username).HasMaxLength(50);
            });

            modelBuilder.Entity<UserFlight>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.FlightId });

                entity.ToTable("UserFlight");

                entity.HasOne(d => d.Flight)
            .WithMany(p => p.UserFlights)
            .HasForeignKey(d => d.FlightId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_UserFlight_Flights");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserFlights)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserFlight_Users");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
