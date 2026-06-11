using Microsoft.AspNetCore.Mvc;
using TournoiEscrime.Api.Dtos;
using TournoiEscrime.Data.Repositories;

namespace TournoiEscrime.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TournamentsController : ControllerBase
{
    private readonly ITournamentRepository _repo;
    private readonly IScoreCalculator _calculator;
    private readonly TournamentRanking _ranking;

    public TournamentsController(ITournamentRepository repo, IScoreCalculator calculator)
    {
        _repo = repo;
        _calculator = calculator;
        _ranking = new TournamentRanking(calculator);
    }

    // GET /api/tournaments
    [HttpGet]
    public async Task<List<TournamentSummaryDto>> GetAll()
    {
        var tournaments = await _repo.GetAllAsync();
        return tournaments.Select(t => new TournamentSummaryDto(
            t.Id, t.Name, t.CreatedAt, t.IsFinished, t.Players.Count)).ToList();
    }

    // GET /api/tournaments/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<TournamentDetailDto>> GetById(int id)
    {
        var t = await _repo.GetByIdAsync(id);
        if (t is null) return NotFound();

        var players = t.Players.Select(p => new PlayerDto(
            p.Id, p.Name, p.IsDisqualified, p.PenaltyPoints,
            p.Matches.Select(m => m.Outcome).ToList(),
            _calculator.CalculateScore(
                p.Matches.Select(m => new MatchResult(Enum.Parse<MatchResult.Result>(m.Outcome))).ToList(),
                p.IsDisqualified, p.PenaltyPoints)
        )).ToList();

        return new TournamentDetailDto(t.Id, t.Name, t.CreatedAt, t.IsFinished, players);
    }

    // POST /api/tournaments
    [HttpPost]
    public async Task<ActionResult<TournamentSummaryDto>> Create([FromBody] CreateTournamentRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Name))
            return BadRequest("Le nom du tournoi est obligatoire.");

        var t = await _repo.CreateAsync(req.Name);
        var dto = new TournamentSummaryDto(t.Id, t.Name, t.CreatedAt, t.IsFinished, 0);
        return CreatedAtAction(nameof(GetById), new { id = t.Id }, dto);
    }

    // POST /api/tournaments/{id}/players
    [HttpPost("{id}/players")]
    public async Task<ActionResult<PlayerDto>> AddPlayer(int id, [FromBody] AddPlayerRequest req)
    {
        var t = await _repo.GetByIdAsync(id);
        if (t is null) return NotFound();
        if (t.IsFinished) return BadRequest("Le tournoi est terminé.");
        if (string.IsNullOrWhiteSpace(req.Name)) return BadRequest("Le nom du joueur est obligatoire.");

        var player = await _repo.AddPlayerAsync(id, req.Name);
        return new PlayerDto(player.Id, player.Name, false, 0, new List<string>(), 0);
    }

    // POST /api/tournaments/{id}/players/{playerId}/matches
    [HttpPost("{id}/players/{playerId}/matches")]
    public async Task<ActionResult> AddMatch(int id, int playerId, [FromBody] AddMatchRequest req)
    {
        var t = await _repo.GetByIdAsync(id);
        if (t is null) return NotFound("Tournoi introuvable.");
        if (t.IsFinished) return BadRequest("Le tournoi est terminé.");

        var validOutcomes = new[] { "Win", "Draw", "Loss" };
        if (!validOutcomes.Contains(req.Outcome))
            return BadRequest($"Résultat invalide. Valeurs acceptées : {string.Join(", ", validOutcomes)}");

        var player = t.Players.FirstOrDefault(p => p.Id == playerId);
        if (player is null) return NotFound("Joueur introuvable.");

        await _repo.AddMatchResultAsync(playerId, req.Outcome);
        return NoContent();
    }

    // PUT /api/tournaments/{id}/players/{playerId}/disqualify
    [HttpPut("{id}/players/{playerId}/disqualify")]
    public async Task<ActionResult> Disqualify(int id, int playerId)
    {
        var t = await _repo.GetByIdAsync(id);
        if (t is null) return NotFound("Tournoi introuvable.");

        var player = t.Players.FirstOrDefault(p => p.Id == playerId);
        if (player is null) return NotFound("Joueur introuvable.");

        await _repo.DisqualifyPlayerAsync(playerId);
        return NoContent();
    }

    // GET /api/tournaments/{id}/ranking
    [HttpGet("{id}/ranking")]
    public async Task<ActionResult<List<RankedPlayerDto>>> GetRanking(int id)
    {
        var t = await _repo.GetByIdAsync(id);
        if (t is null) return NotFound();

        var players = t.Players.Select(p => new global::TournoiEscrime.Player
        {
            Name = p.Name,
            IsDisqualified = p.IsDisqualified,
            PenaltyPoints = p.PenaltyPoints,
            Matches = p.Matches
                .Select(m => new MatchResult(Enum.Parse<MatchResult.Result>(m.Outcome)))
                .ToList()
        }).ToList();

        var ranked = _ranking.GetRanking(players);
        return ranked.Select((p, i) => new RankedPlayerDto(
            i + 1, t.Players.First(e => e.Name == p.Name).Id, p.Name,
            _calculator.CalculateScore(p.Matches, p.IsDisqualified, p.PenaltyPoints),
            p.IsDisqualified)).ToList();
    }

    // GET /api/tournaments/{id}/champion
    [HttpGet("{id}/champion")]
    public async Task<ActionResult<RankedPlayerDto?>> GetChampion(int id)
    {
        var t = await _repo.GetByIdAsync(id);
        if (t is null) return NotFound();

        var players = t.Players.Select(p => new global::TournoiEscrime.Player
        {
            Name = p.Name,
            IsDisqualified = p.IsDisqualified,
            PenaltyPoints = p.PenaltyPoints,
            Matches = p.Matches
                .Select(m => new MatchResult(Enum.Parse<MatchResult.Result>(m.Outcome)))
                .ToList()
        }).ToList();

        var champion = _ranking.GetChampion(players);
        if (champion is null) return Ok(null);

        var entity = t.Players.First(e => e.Name == champion.Name);
        return new RankedPlayerDto(
            1, entity.Id, champion.Name,
            _calculator.CalculateScore(champion.Matches, champion.IsDisqualified, champion.PenaltyPoints),
            champion.IsDisqualified);
    }

    // PUT /api/tournaments/{id}/finish
    [HttpPut("{id}/finish")]
    public async Task<ActionResult> Finish(int id)
    {
        var t = await _repo.GetByIdAsync(id);
        if (t is null) return NotFound();

        await _repo.FinishTournamentAsync(id);
        return NoContent();
    }
}
