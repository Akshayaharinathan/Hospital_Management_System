using System.Collections.Generic;
using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore;
using RoomService.Models;

namespace RoomService.Data
{
    public class RoomDbContext : DbContext
    {
        public RoomDbContext(DbContextOptions<RoomDbContext> options) : base(options) { }

        public DbSet<Room> Rooms => Set<Room>();
        public DbSet<WardBed> WardBeds => Set<WardBed>();
        public DbSet<Allocation> Allocations => Set<Allocation>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Room>()
                .HasIndex(r => r.RoomNumber)
                .IsUnique();

            modelBuilder.Entity<WardBed>()
                .HasIndex(w => new { w.WardName, w.BedNumber })
                .IsUnique();

            // Unique active allocation per patient
            modelBuilder.Entity<Allocation>()
                .HasIndex(a => a.PatientId)
                .IsUnique()
                .HasFilter("[EndTime] IS NULL"); // SQL Server filtered index

            base.OnModelCreating(modelBuilder);
        }
    }
}
