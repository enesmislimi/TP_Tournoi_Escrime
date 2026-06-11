namespace TournoiEscrime.Data.Entities;

public class PlayerEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsDisqualified { get; set; }
    public int PenaltyPoints { get; set; }

    // Clé étrangère vers le tournoi
    public int TournamentId { get; set; }
    public TournamentEntity Tournament { get; set; } = null!;

    public List<MatchResultEntity> Matches { get; set; } = new();
}
