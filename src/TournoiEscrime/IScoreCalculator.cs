namespace TournoiEscrime;

public interface IScoreCalculator
{
    int CalculateScore(List<MatchResult> matches, bool isDisqualified = false, int penaltyPoints = 0);
}
