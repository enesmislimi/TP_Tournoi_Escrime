using FluentAssertions;
using TournoiEscrime;

namespace TournoiEscrime.Tests;

public class ScoreCalculatorTests
{
    private readonly ScoreCalculator _calculator;

    public ScoreCalculatorTests()
    {
        _calculator = new ScoreCalculator();
    }

    // -------------------------------------------------------------------------
    // Tests de base
    // -------------------------------------------------------------------------

    [Fact]
    public void Should_Calculate_Simple_Score_With_Win_Draw_Loss()
    {
        // Arrange
        var matches = new List<MatchResult>
        {
            new(MatchResult.Result.Win),
            new(MatchResult.Result.Draw),
            new(MatchResult.Result.Loss)
        };

        // Act
        var score = _calculator.CalculateScore(matches);

        // Assert
        score.Should().Be(4, "because 3+1+0 = 4 points without bonus");
    }

    [Fact]
    public void Should_Return_Six_Points_For_Two_Wins()
    {
        var matches = new List<MatchResult>
        {
            new(MatchResult.Result.Win),
            new(MatchResult.Result.Win)
        };

        var score = _calculator.CalculateScore(matches);

        score.Should().Be(6, "because 3+3 = 6 points, no bonus without 3 consecutive wins");
    }

    [Fact]
    public void Should_Return_Three_Points_For_Three_Draws()
    {
        var matches = new List<MatchResult>
        {
            new(MatchResult.Result.Draw),
            new(MatchResult.Result.Draw),
            new(MatchResult.Result.Draw)
        };

        var score = _calculator.CalculateScore(matches);

        score.Should().Be(3, "because 1+1+1 = 3 points");
    }

    [Fact]
    public void Should_Return_Zero_For_Only_Losses()
    {
        var matches = new List<MatchResult>
        {
            new(MatchResult.Result.Loss),
            new(MatchResult.Result.Loss)
        };

        var score = _calculator.CalculateScore(matches);

        score.Should().Be(0, "because losses give 0 points");
    }

    // -------------------------------------------------------------------------
    // Tests du bonus de série
    // -------------------------------------------------------------------------

    [Fact]
    public void Should_Add_Bonus_For_Three_Consecutive_Wins()
    {
        // Win-Win-Win : 9 points de base + 5 bonus = 14
        var matches = new List<MatchResult>
        {
            new(MatchResult.Result.Win),
            new(MatchResult.Result.Win),
            new(MatchResult.Result.Win)
        };

        var score = _calculator.CalculateScore(matches);

        score.Should().Be(14, "because 3*3 = 9 + 5 streak bonus = 14");
    }

    [Fact]
    public void Should_Add_Bonus_Only_Once_For_Four_Consecutive_Wins()
    {
        // Win-Win-Win-Win : 12 points + 5 bonus (une seule fois) = 17
        var matches = new List<MatchResult>
        {
            new(MatchResult.Result.Win),
            new(MatchResult.Result.Win),
            new(MatchResult.Result.Win),
            new(MatchResult.Result.Win)
        };

        var score = _calculator.CalculateScore(matches);

        score.Should().Be(17, "because 4*3 = 12 + 5 bonus (granted once) = 17");
    }

    [Fact]
    public void Should_Not_Add_Bonus_When_Streak_Is_Interrupted()
    {
        // Win-Win-Loss-Win : pas de 3 victoires consécutives
        var matches = new List<MatchResult>
        {
            new(MatchResult.Result.Win),
            new(MatchResult.Result.Win),
            new(MatchResult.Result.Loss),
            new(MatchResult.Result.Win)
        };

        var score = _calculator.CalculateScore(matches);

        score.Should().Be(9, "because no streak of 3 consecutive wins, 3+3+0+3 = 9");
    }

    [Fact]
    public void Should_Add_Bonus_Twice_For_Two_Separate_Streaks()
    {
        // Win×3, Loss, Win×4 : deux séries distinctes → deux bonus
        // Points de base : 3+3+3+0+3+3+3+3 = 21
        // Bonus : +5 (première série) + 5 (deuxième série) = +10
        var matches = new List<MatchResult>
        {
            new(MatchResult.Result.Win),
            new(MatchResult.Result.Win),
            new(MatchResult.Result.Win),
            new(MatchResult.Result.Loss),
            new(MatchResult.Result.Win),
            new(MatchResult.Result.Win),
            new(MatchResult.Result.Win),
            new(MatchResult.Result.Win)
        };

        var score = _calculator.CalculateScore(matches);

        score.Should().Be(31, "because 21 base points + 5 + 5 bonuses = 31");
    }

    [Fact]
    public void Should_Not_Add_Bonus_When_Draw_Interrupts_Wins()
    {
        // Win-Draw-Win-Win : la série est interrompue par le Draw
        var matches = new List<MatchResult>
        {
            new(MatchResult.Result.Win),
            new(MatchResult.Result.Draw),
            new(MatchResult.Result.Win),
            new(MatchResult.Result.Win)
        };

        var score = _calculator.CalculateScore(matches);

        score.Should().Be(10, "because Win+Draw breaks the streak: 3+1+3+3 = 10, no bonus");
    }

    // -------------------------------------------------------------------------
    // Tests de disqualification
    // -------------------------------------------------------------------------

    [Fact]
    public void Should_Return_Zero_When_Player_Is_Disqualified_With_Positive_Score()
    {
        var matches = new List<MatchResult>
        {
            new(MatchResult.Result.Win),
            new(MatchResult.Result.Win),
            new(MatchResult.Result.Win)
        };

        var score = _calculator.CalculateScore(matches, isDisqualified: true);

        score.Should().Be(0, "because disqualification resets score to 0 regardless of performance");
    }

    [Fact]
    public void Should_Return_Zero_When_Player_Is_Disqualified_Without_Matches()
    {
        var matches = new List<MatchResult>();

        var score = _calculator.CalculateScore(matches, isDisqualified: true);

        score.Should().Be(0, "because a disqualified player with no matches still scores 0");
    }

    // -------------------------------------------------------------------------
    // Tests des pénalités
    // -------------------------------------------------------------------------

    [Fact]
    public void Should_Subtract_Penalty_From_Score()
    {
        // 3 victoires + 1 nul = 10 points, pénalité 3 → 7 points
        var matches = new List<MatchResult>
        {
            new(MatchResult.Result.Win),
            new(MatchResult.Result.Win),
            new(MatchResult.Result.Win),
            new(MatchResult.Result.Draw)
        };

        var score = _calculator.CalculateScore(matches, penaltyPoints: 3);

        score.Should().Be(12, "because 9+1+5 bonus = 15 - 3 penalty = 12");
    }

    [Fact]
    public void Should_Return_Zero_When_Penalty_Exceeds_Score()
    {
        // Win+Draw = 4 points, pénalité 8 → 0 (pas -4)
        var matches = new List<MatchResult>
        {
            new(MatchResult.Result.Win),
            new(MatchResult.Result.Draw)
        };

        var score = _calculator.CalculateScore(matches, penaltyPoints: 8);

        score.Should().Be(0, "because score cannot be negative: 4 - 8 = 0");
    }

    [Fact]
    public void Should_Return_Zero_When_Penalty_Equals_Score()
    {
        // 2 victoires + 1 nul = 7 points, pénalité 7 → 0
        var matches = new List<MatchResult>
        {
            new(MatchResult.Result.Win),
            new(MatchResult.Result.Win),
            new(MatchResult.Result.Draw)
        };

        var score = _calculator.CalculateScore(matches, penaltyPoints: 7);

        score.Should().Be(0, "because 7 - 7 = 0");
    }

    // -------------------------------------------------------------------------
    // Tests des cas limites
    // -------------------------------------------------------------------------

    [Fact]
    public void Should_Return_Zero_For_Empty_Match_List()
    {
        var matches = new List<MatchResult>();

        var score = _calculator.CalculateScore(matches);

        score.Should().Be(0, "because no matches means no points");
    }

    [Fact]
    public void Should_Throw_ArgumentNullException_When_Matches_Is_Null()
    {
        // Act & Assert
        Action act = () => _calculator.CalculateScore(null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("matches")
            .WithMessage("*cannot be null*");
    }

    [Fact]
    public void Should_Throw_ArgumentException_When_Penalty_Is_Negative()
    {
        var matches = new List<MatchResult>();

        Action act = () => _calculator.CalculateScore(matches, penaltyPoints: -5);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("penaltyPoints");
    }

    [Fact]
    public void Should_Handle_Long_Tournament_With_Complex_Pattern()
    {
        // 100 combats : groupes de Win×3 + Loss répétés = 25 séries
        // Chaque groupe : 3*3 + 5 bonus + 0 = 14 points
        // 25 groupes × 14 = 350 points
        var matches = new List<MatchResult>();
        for (int i = 0; i < 25; i++)
        {
            matches.Add(new(MatchResult.Result.Win));
            matches.Add(new(MatchResult.Result.Win));
            matches.Add(new(MatchResult.Result.Win));
            matches.Add(new(MatchResult.Result.Loss));
        }

        var score = _calculator.CalculateScore(matches);

        score.Should().Be(350, "because 25 groups of (Win×3 + Loss) = 25 × (9+5) = 350");
    }

    // -------------------------------------------------------------------------
    // Tests paramétrés (Theory + InlineData)
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(3, 0, 0, 14)]  // 3 wins → 9 + 5 bonus = 14
    [InlineData(2, 1, 0, 7)]   // 2 wins, 1 draw → 7, pas de bonus (pas consécutifs ici)
    [InlineData(0, 0, 3, 0)]   // 3 losses → 0
    [InlineData(1, 1, 1, 4)]   // 1 win, 1 draw, 1 loss → 4
    public void Should_Calculate_Score_With_Different_Result_Combinations(
        int wins, int draws, int losses, int expected)
    {
        // Arrange : on met tous les wins d'abord, puis draws, puis losses
        var matches = new List<MatchResult>();
        for (int i = 0; i < wins; i++) matches.Add(new(MatchResult.Result.Win));
        for (int i = 0; i < draws; i++) matches.Add(new(MatchResult.Result.Draw));
        for (int i = 0; i < losses; i++) matches.Add(new(MatchResult.Result.Loss));

        // Act
        var score = _calculator.CalculateScore(matches);

        // Assert
        score.Should().Be(expected);
    }

    // -------------------------------------------------------------------------
    // Tests paramétrés (Theory + MemberData)
    // -------------------------------------------------------------------------

    public static IEnumerable<object[]> ComplexScenarios =>
        new List<object[]>
        {
            // Exemple 1 du TP : Win, Draw, Loss, Win → 7
            new object[] { new[] { "Win", "Draw", "Loss", "Win" }, 7 },
            // Exemple 2 du TP : Win, Win, Win, Draw → 15
            new object[] { new[] { "Win", "Win", "Win", "Draw" }, 15 },
            // Exemple 3 du TP : Win×3, Loss, Win×4 → 31
            new object[] { new[] { "Win", "Win", "Win", "Loss", "Win", "Win", "Win", "Win" }, 31 },
        };

    [Theory]
    [MemberData(nameof(ComplexScenarios))]
    public void Should_Calculate_Score_For_Complex_Scenarios(string[] results, int expected)
    {
        // Arrange
        var matches = results.Select(r => r switch
        {
            "Win"  => new MatchResult(MatchResult.Result.Win),
            "Draw" => new MatchResult(MatchResult.Result.Draw),
            _      => new MatchResult(MatchResult.Result.Loss)
        }).ToList();

        // Act
        var score = _calculator.CalculateScore(matches);

        // Assert
        score.Should().Be(expected);
    }
}
