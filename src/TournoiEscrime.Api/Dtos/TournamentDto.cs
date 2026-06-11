namespace TournoiEscrime.Api.Dtos;

// DTOs : objets qui transitent sur le réseau (pas les entités EF ni les classes métier)

public record TournamentSummaryDto(int Id, string Name, DateTime CreatedAt, bool IsFinished, int PlayerCount);

public record TournamentDetailDto(int Id, string Name, DateTime CreatedAt, bool IsFinished, List<PlayerDto> Players);

public record PlayerDto(int Id, string Name, bool IsDisqualified, int PenaltyPoints, List<string> Matches, int Score);

public record RankedPlayerDto(int Rank, int Id, string Name, int Score, bool IsDisqualified);

public record CreateTournamentRequest(string Name);

public record AddPlayerRequest(string Name);

public record AddMatchRequest(string Outcome);
