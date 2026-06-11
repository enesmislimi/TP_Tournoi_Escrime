using Microsoft.EntityFrameworkCore;
using TournoiEscrime.Data.Entities;

namespace TournoiEscrime.Data.Repositories;

public class TournamentRepository : ITournamentRepository
{
    private readonly TournamentDbContext _db;

    public TournamentRepository(TournamentDbContext db)
    {
        _db = db;
    }

    public async Task<List<TournamentEntity>> GetAllAsync()
        => await _db.Tournaments
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

    public async Task<TournamentEntity?> GetByIdAsync(int id)
        => await _db.Tournaments
            .Include(t => t.Players)
                .ThenInclude(p => p.Matches)
            .FirstOrDefaultAsync(t => t.Id == id);

    public async Task<TournamentEntity> CreateAsync(string name)
    {
        var tournament = new TournamentEntity { Name = name };
        _db.Tournaments.Add(tournament);
        await _db.SaveChangesAsync();
        return tournament;
    }

    public async Task<PlayerEntity> AddPlayerAsync(int tournamentId, string playerName)
    {
        var player = new PlayerEntity { Name = playerName, TournamentId = tournamentId };
        _db.Players.Add(player);
        await _db.SaveChangesAsync();
        return player;
    }

    public async Task<MatchResultEntity> AddMatchResultAsync(int playerId, string outcome)
    {
        var match = new MatchResultEntity { PlayerId = playerId, Outcome = outcome };
        _db.MatchResults.Add(match);
        await _db.SaveChangesAsync();
        return match;
    }

    public async Task DisqualifyPlayerAsync(int playerId)
    {
        var player = await _db.Players.FindAsync(playerId);
        if (player is not null)
        {
            player.IsDisqualified = true;
            await _db.SaveChangesAsync();
        }
    }

    public async Task FinishTournamentAsync(int tournamentId)
    {
        var tournament = await _db.Tournaments.FindAsync(tournamentId);
        if (tournament is not null)
        {
            tournament.IsFinished = true;
            await _db.SaveChangesAsync();
        }
    }
}
