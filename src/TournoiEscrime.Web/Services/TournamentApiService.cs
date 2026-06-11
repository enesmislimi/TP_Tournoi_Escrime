using System.Net.Http.Json;

namespace TournoiEscrime.Web.Services;

// Service qui encapsule tous les appels HTTP vers l'API REST
// Blazor l'injecte automatiquement dans les composants qui en ont besoin
public class TournamentApiService
{
    private readonly HttpClient _http;

    public TournamentApiService(HttpClient http) => _http = http;

    public Task<List<TournamentSummary>?> GetTournamentsAsync()
        => _http.GetFromJsonAsync<List<TournamentSummary>>("api/tournaments");

    public Task<TournamentDetail?> GetTournamentAsync(int id)
        => _http.GetFromJsonAsync<TournamentDetail>($"api/tournaments/{id}");

    public async Task<TournamentSummary?> CreateTournamentAsync(string name)
    {
        var resp = await _http.PostAsJsonAsync("api/tournaments", new { name });
        return resp.IsSuccessStatusCode
            ? await resp.Content.ReadFromJsonAsync<TournamentSummary>()
            : null;
    }

    public async Task<PlayerDto?> AddPlayerAsync(int tournamentId, string playerName)
    {
        var resp = await _http.PostAsJsonAsync($"api/tournaments/{tournamentId}/players", new { name = playerName });
        return resp.IsSuccessStatusCode
            ? await resp.Content.ReadFromJsonAsync<PlayerDto>()
            : null;
    }

    public async Task<bool> AddMatchAsync(int tournamentId, int playerId, string outcome)
    {
        var resp = await _http.PostAsJsonAsync(
            $"api/tournaments/{tournamentId}/players/{playerId}/matches", new { outcome });
        return resp.IsSuccessStatusCode;
    }

    public async Task<bool> DisqualifyAsync(int tournamentId, int playerId)
    {
        var resp = await _http.PutAsync(
            $"api/tournaments/{tournamentId}/players/{playerId}/disqualify", null);
        return resp.IsSuccessStatusCode;
    }

    public Task<List<RankedPlayer>?> GetRankingAsync(int tournamentId)
        => _http.GetFromJsonAsync<List<RankedPlayer>>($"api/tournaments/{tournamentId}/ranking");

    public Task<RankedPlayer?> GetChampionAsync(int tournamentId)
        => _http.GetFromJsonAsync<RankedPlayer>($"api/tournaments/{tournamentId}/champion");

    public async Task<bool> FinishTournamentAsync(int tournamentId)
    {
        var resp = await _http.PutAsync($"api/tournaments/{tournamentId}/finish", null);
        return resp.IsSuccessStatusCode;
    }
}

// Modèles correspondant aux DTOs de l'API
public record TournamentSummary(int Id, string Name, DateTime CreatedAt, bool IsFinished, int PlayerCount);
public record TournamentDetail(int Id, string Name, DateTime CreatedAt, bool IsFinished, List<PlayerDto> Players);
public record PlayerDto(int Id, string Name, bool IsDisqualified, int PenaltyPoints, List<string> Matches, int Score);
public record RankedPlayer(int Rank, int Id, string Name, int Score, bool IsDisqualified);
