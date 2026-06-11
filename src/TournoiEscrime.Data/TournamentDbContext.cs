using Microsoft.EntityFrameworkCore;
using TournoiEscrime.Data.Entities;

namespace TournoiEscrime.Data;

public class TournamentDbContext : DbContext
{
    public TournamentDbContext(DbContextOptions<TournamentDbContext> options) : base(options) { }

    // Chaque DbSet<T> correspond à une table en base
    public DbSet<TournamentEntity> Tournaments => Set<TournamentEntity>();
    public DbSet<PlayerEntity> Players => Set<PlayerEntity>();
    public DbSet<MatchResultEntity> MatchResults => Set<MatchResultEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TournamentEntity>(e =>
        {
            e.HasKey(t => t.Id);
            e.Property(t => t.Name).IsRequired().HasMaxLength(200);
            e.HasMany(t => t.Players)
             .WithOne(p => p.Tournament)
             .HasForeignKey(p => p.TournamentId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PlayerEntity>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Name).IsRequired().HasMaxLength(100);
            e.HasMany(p => p.Matches)
             .WithOne(m => m.Player)
             .HasForeignKey(m => m.PlayerId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MatchResultEntity>(e =>
        {
            e.HasKey(m => m.Id);
            e.Property(m => m.Outcome).IsRequired().HasMaxLength(10);
        });
    }
}
