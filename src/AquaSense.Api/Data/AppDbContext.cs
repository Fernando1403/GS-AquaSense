using AquaSense.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AquaSense.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Local> Locais => Set<Local>();
    public DbSet<Leitura> Leituras => Set<Leitura>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Local>(e =>
        {
            e.Property(l => l.Nome).IsRequired().HasMaxLength(100);
            e.HasMany(l => l.Leituras)
             .WithOne(l => l.Local)
             .HasForeignKey(l => l.LocalId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
