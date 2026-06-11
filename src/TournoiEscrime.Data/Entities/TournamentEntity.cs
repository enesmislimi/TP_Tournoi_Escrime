namespace TournoiEscrime.Data.Entities;

public class TournamentEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsFinished { get; set; }

    // Navigation property : EF charge automatiquement les joueurs liés
    public List<PlayerEntity> Players { get; set; } = new();
}
