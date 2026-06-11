using TournoiEscrime.Data.Entities;

namespace TournoiEscrime.Data.Repositories;

public interface ITournamentRepository
{
    Task<List<TournamentEntity>> GetAllAsync();
    Task<TournamentEntity?> GetByIdAsync(int id);
    Task<TournamentEntity> CreateAsync(string name);
    Task<PlayerEntity> AddPlayerAsync(int tournamentId, string playerName);
    Task<MatchResultEntity> AddMatchResultAsync(int playerId, string outcome);
    Task DisqualifyPlayerAsync(int playerId);
    Task FinishTournamentAsync(int tournamentId);
}
