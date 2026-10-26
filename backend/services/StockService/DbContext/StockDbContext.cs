using Microsoft.EntityFrameworkCore;
using StockService.Models;

namespace StockService.Data
{
    public class StockDbContext : DbContext
    {
        public StockDbContext(DbContextOptions<StockDbContext> options) : base(options) { }

        public DbSet<Medicine> Medicines => Set<Medicine>();
        public DbSet<StockTransaction> StockTransactions => Set<StockTransaction>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Medicine>()
                .HasIndex(m => m.Name)
                .IsUnique();

            modelBuilder.Entity<StockTransaction>()
                .HasOne(st => st.Medicine)
                .WithMany()
                .HasForeignKey(st => st.MedicineId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}