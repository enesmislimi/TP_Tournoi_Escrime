using FluentAssertions;
using Moq;
using TournoiEscrime;

namespace TournoiEscrime.Tests;

public class TournamentRankingTests
{
    private readonly Mock<IScoreCalculator> _calculatorMock;
    private readonly TournamentRanking _ranking;

    public TournamentRankingTests()
    {
        // Mock<T> crée un faux objet qui implémente IScoreCalculator
        // On contrôle exactement ce qu'il retourne sans exécuter le vrai code
        _calculatorMock = new Mock<IScoreCalculator>();
        _ranking = new TournamentRanking(_calculatorMock.Object);
    }

    // -------------------------------------------------------------------------
    // Tests du classement
    // -------------------------------------------------------------------------

    [Fact]
    public void Should_Rank_Players_By_Score_Descending()
    {
        // Arrange : chaque joueur a un nombre distinct de combats pour que
        // le mock puisse les différencier via It.Is<>()
        var alice = new Player { Name = "Alice", Matches = new List<MatchResult> { new(MatchResult.Result.Win) } };
        var bob   = new Player { Name = "Bob",   Matches = new List<MatchResult> { new(MatchResult.Result.Loss), new(MatchResult.Result.Loss) } };
        var carol = new Player { Name = "Carol", Matches = new List<MatchResult> { new(MatchResult.Result.Win), new(MatchResult.Result.Win), new(MatchResult.Result.Win) } };

        // Setup : It.Is<>() avec un prédicat permet de cibler une liste précise
        _calculatorMock.Setup(c => c.CalculateScore(It.Is<List<MatchResult>>(m => m.Count == 1), false, 0)).Returns(10);
        _calculatorMock.Setup(c => c.CalculateScore(It.Is<List<MatchResult>>(m => m.Count == 2), false, 0)).Returns(5);
        _calculatorMock.Setup(c => c.CalculateScore(It.Is<List<MatchResult>>(m => m.Count == 3), false, 0)).Returns(15);

        // Act
        var result = _ranking.GetRanking(new List<Player> { alice, bob, carol });

        // Assert
        result.Select(p => p.Name).Should().ContainInOrder(
            new[] { "Carol", "Alice", "Bob" },
            "because Carol (15pts) > Alice (10pts) > Bob (5pts)");
    }

    [Fact]
    public void Should_Keep_All_Players_When_Scores_Are_Equal()
    {
        // Arrange : même score pour tout le monde
        var alice = new Player { Name = "Alice", Matches = new List<MatchResult> { new(MatchResult.Result.Win) } };
        var bob   = new Player { Name = "Bob",   Matches = new List<MatchResult> { new(MatchResult.Result.Draw) } };

        _calculatorMock.Setup(c => c.CalculateScore(It.IsAny<List<MatchResult>>(), false, 0)).Returns(7);

        // Act
        var result = _ranking.GetRanking(new List<Player> { alice, bob });

        // Assert : les deux joueurs sont bien présents même à égalité
        result.Should().HaveCount(2);
        result.Select(p => p.Name).Should().Contain("Alice").And.Contain("Bob");
    }

    // -------------------------------------------------------------------------
    // Tests du champion
    // -------------------------------------------------------------------------

    [Fact]
    public void Should_Return_Champion_With_Highest_Score()
    {
        // Arrange
        var alice = new Player { Name = "Alice", Matches = new List<MatchResult> { new(MatchResult.Result.Win), new(MatchResult.Result.Win) } };
        var bob   = new Player { Name = "Bob",   Matches = new List<MatchResult> { new(MatchResult.Result.Win) } };

        _calculatorMock.Setup(c => c.CalculateScore(It.Is<List<MatchResult>>(m => m.Count == 2), false, 0)).Returns(20);
        _calculatorMock.Setup(c => c.CalculateScore(It.Is<List<MatchResult>>(m => m.Count == 1), false, 0)).Returns(12);

        // Act
        var champion = _ranking.GetChampion(new List<Player> { alice, bob });

        // Assert
        champion.Should().NotBeNull();
        champion!.Name.Should().Be("Alice", "because Alice has the highest score (20 pts)");
    }

    [Fact]
    public void Should_Return_Null_When_Player_List_Is_Empty()
    {
        // Act
        var champion = _ranking.GetChampion(new List<Player>());

        // Assert
        champion.Should().BeNull("because there is no player to be champion");
    }

    [Fact]
    public void Should_Return_A_Player_When_All_Are_Disqualified()
    {
        // Arrange : tous disqualifiés → le mock retourne 0 pour chacun
        var alice = new Player { Name = "Alice", IsDisqualified = true };
        var bob   = new Player { Name = "Bob",   IsDisqualified = true };

        _calculatorMock.Setup(c => c.CalculateScore(It.IsAny<List<MatchResult>>(), true, 0)).Returns(0);

        // Act
        var result = _ranking.GetRanking(new List<Player> { alice, bob });

        // Assert : le classement est retourné même si tout le monde est à 0
        result.Should().HaveCount(2, "because disqualified players still appear in the ranking");
    }

    // -------------------------------------------------------------------------
    // Tests des cas limites
    // -------------------------------------------------------------------------

    [Fact]
    public void Should_Throw_When_Players_Is_Null_In_GetRanking()
    {
        Action act = () => _ranking.GetRanking(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("players");
    }

    [Fact]
    public void Should_Throw_When_Players_Is_Null_In_GetChampion()
    {
        Action act = () => _ranking.GetChampion(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("players");
    }
}
