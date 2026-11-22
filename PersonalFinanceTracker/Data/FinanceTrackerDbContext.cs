using Microsoft.EntityFrameworkCore;
using PersonalFinanceTracker.Entities;

namespace PersonalFinanceTracker.Data;

public class FinanceTrackerDbContext : DbContext
{
    public FinanceTrackerDbContext(DbContextOptions<FinanceTrackerDbContext> options) : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Category
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
            entity.Property(c => c.Type).HasConversion<string>(); // Store enum as string or int (string is friendly)
        });

        // Configure Account
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Name).IsRequired().HasMaxLength(100);
            entity.Property(a => a.Type).IsRequired().HasMaxLength(50);
            entity.Property(a => a.CurrentBalance).HasPrecision(18, 2);
        });

        // Configure Transaction & Relationships
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Amount).HasPrecision(18, 2);
            entity.Property(t => t.BeforeBalance).HasPrecision(18, 2);
            entity.Property(t => t.AfterBalance).HasPrecision(18, 2);
            entity.Property(t => t.Description).HasMaxLength(500);

            entity.HasOne(t => t.Category)
                  .WithMany(c => c.Transactions)
                  .HasForeignKey(t => t.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(t => t.Account)
                  .WithMany(a => a.Transactions)
                  .HasForeignKey(t => t.AccountId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
