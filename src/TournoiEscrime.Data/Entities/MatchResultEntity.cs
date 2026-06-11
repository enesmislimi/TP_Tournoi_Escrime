namespace TournoiEscrime.Data.Entities;

public class MatchResultEntity
{
    public int Id { get; set; }

    // Stocké comme string en base ("Win", "Draw", "Loss")
    public string Outcome { get; set; } = string.Empty;

    public int PlayerId { get; set; }
    public PlayerEntity Player { get; set; } = null!;
}
