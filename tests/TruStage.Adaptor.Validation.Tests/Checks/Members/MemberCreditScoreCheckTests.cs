using FluentAssertions;
using TruStage.Adaptor.Core.DataValidation.Models;
using TruStage.Adaptor.Validation.Checks.Members;
using TruStage.Adaptor.Validation.Tests.Helpers;
using Xunit;

namespace TruStage.Adaptor.Validation.Tests.Checks.Members;

public sealed class MemberCreditScoreCheckTests
{
    private readonly MemberCreditScoreCheck _sut = new();

    [Fact]
    public void CheckId_IsMbr002() =>
        _sut.CheckId.Should().Be("MBR-002");

    [Fact]
    public void Validate_NullCreditScore_ReturnsNoResults()
    {
        var m = Builders.ValidMember() with { CreditScore = null };
        _sut.Validate(m).Should().BeEmpty();
    }

    [Theory]
    [InlineData(300)]
    [InlineData(650)]
    [InlineData(850)]
    public void Validate_ValidFicoRange_ReturnsNoResults(int score)
    {
        var m = Builders.ValidMember() with { CreditScore = score };
        _sut.Validate(m).Should().BeEmpty();
    }

    [Theory]
    [InlineData(299)]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_ScoreBelowFicoMin_ReturnsError(int score)
    {
        var m = Builders.ValidMember() with { CreditScore = score };
        var results = _sut.Validate(m).ToList();

        results.Should().ContainSingle();
        results[0].Severity.Should().Be(CheckSeverity.Error);
        results[0].FieldName.Should().Be("CreditScore");
    }

    [Theory]
    [InlineData(851)]
    [InlineData(999)]
    [InlineData(1000)]
    public void Validate_ScoreAboveFicoMax_ReturnsError(int score)
    {
        var m = Builders.ValidMember() with { CreditScore = score };
        var results = _sut.Validate(m).ToList();

        results.Should().ContainSingle();
        results[0].Severity.Should().Be(CheckSeverity.Error);
        results[0].FieldName.Should().Be("CreditScore");
        results[0].ActualValue.Should().Be(score.ToString());
        results[0].ExpectedRange.Should().Contain("300");
        results[0].ExpectedRange.Should().Contain("850");
    }
}
