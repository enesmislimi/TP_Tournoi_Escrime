namespace TournoiEscrime;

public class TournamentRanking
{
    private readonly IScoreCalculator _scoreCalculator;

    public TournamentRanking(IScoreCalculator scoreCalculator)
    {
        _scoreCalculator = scoreCalculator;
    }

    public List<Player> GetRanking(List<Player> players)
    {
        if (players == null)
            throw new ArgumentNullException(nameof(players));

        return players
            .OrderByDescending(p => _scoreCalculator.CalculateScore(p.Matches, p.IsDisqualified, p.PenaltyPoints))
            .ToList();
    }

    public Player? GetChampion(List<Player> players)
    {
        if (players == null)
            throw new ArgumentNullException(nameof(players));

        if (players.Count == 0)
            return null;

        return players
            .OrderByDescending(p => _scoreCalculator.CalculateScore(p.Matches, p.IsDisqualified, p.PenaltyPoints))
            .First();
    }
}
