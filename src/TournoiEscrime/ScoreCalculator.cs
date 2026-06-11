namespace TournoiEscrime;

public class ScoreCalculator
{
    public int CalculateScore(List<MatchResult> matches, bool isDisqualified = false, int penaltyPoints = 0)
    {
        if (matches == null)
            throw new ArgumentNullException(nameof(matches), "matches cannot be null");

        if (penaltyPoints < 0)
            throw new ArgumentException("Penalty points cannot be negative.", nameof(penaltyPoints));

        if (isDisqualified)
            return 0;

        int score = 0;
        int consecutiveWins = 0;
        bool bonusGrantedForCurrentStreak = false;

        foreach (var match in matches)
        {
            if (match.Outcome == MatchResult.Result.Win)
            {
                score += 3;
                consecutiveWins++;

                // Le bonus est accordé une seule fois par série, dès que l'on atteint 3 wins consécutifs
                if (consecutiveWins >= 3 && !bonusGrantedForCurrentStreak)
                {
                    score += 5;
                    bonusGrantedForCurrentStreak = true;
                }
            }
            else
            {
                if (match.Outcome == MatchResult.Result.Draw)
                    score += 1;

                // La série est interrompue : on réinitialise
                consecutiveWins = 0;
                bonusGrantedForCurrentStreak = false;
            }
        }

        score -= penaltyPoints;

        return Math.Max(0, score);
    }
}
