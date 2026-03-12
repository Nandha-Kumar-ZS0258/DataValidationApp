using FluentAssertions;
using TruStage.Adaptor.Validation.Models;
using TruStage.Adaptor.Validation.Checks.Loans;
using TruStage.Adaptor.Validation.Tests.Helpers;
using Xunit;

namespace TruStage.Adaptor.Validation.Tests.Checks.Loans;

public sealed class LoanRateCheckTests
{
    private readonly LoanRateCheck _sut = new();

    [Fact]
    public void CheckId_IsLn004() =>
        _sut.CheckId.Should().Be("LN-004");

    [Fact]
    public void Validate_ValidRateAndNullDti_ReturnsNoResults()
    {
        var l = Builders.ValidLoan() with { InterestRate = 0.065m, DebtToIncomeRatio = null };
        _sut.Validate(l).Should().BeEmpty();
    }

    [Fact]
    public void Validate_ValidRateAndDti_ReturnsNoResults()
    {
        var l = Builders.ValidLoan() with { InterestRate = 0.065m, DebtToIncomeRatio = 0.38m };
        _sut.Validate(l).Should().BeEmpty();
    }

    // ── Negative rate → Error ─────────────────────────────────────────────────

    [Fact]
    public void Validate_NegativeInterestRate_ReturnsError()
    {
        var l = Builders.ValidLoan() with { InterestRate = -0.01m };
        var results = _sut.Validate(l).ToList();

        results.Should().Contain(r =>
            r.FieldName == "InterestRate" && r.Severity == CheckSeverity.Error);
    }

    // ── Rate > 30% → Warning ──────────────────────────────────────────────────

    [Theory]
    [InlineData(0.31)]
    [InlineData(1.00)]
    public void Validate_RateAbove30Percent_ReturnsWarning(decimal rate)
    {
        var l = Builders.ValidLoan() with { InterestRate = rate };
        var results = _sut.Validate(l).ToList();

        results.Should().Contain(r =>
            r.FieldName == "InterestRate" && r.Severity == CheckSeverity.Warning);
    }

    // ── Negative DTI → Error ──────────────────────────────────────────────────

    [Fact]
    public void Validate_NegativeDti_ReturnsError()
    {
        var l = Builders.ValidLoan() with { DebtToIncomeRatio = -0.1m };
        var results = _sut.Validate(l).ToList();

        results.Should().Contain(r =>
            r.FieldName == "DebtToIncomeRatio" && r.Severity == CheckSeverity.Error);
    }

    // ── DTI > 200% → Warning ──────────────────────────────────────────────────

    [Fact]
    public void Validate_DtiAbove200Percent_ReturnsWarning()
    {
        var l = Builders.ValidLoan() with { DebtToIncomeRatio = 2.01m };
        var results = _sut.Validate(l).ToList();

        results.Should().Contain(r =>
            r.FieldName == "DebtToIncomeRatio" && r.Severity == CheckSeverity.Warning);
    }

    [Fact]
    public void Validate_DtiExactly200Percent_ReturnsNoResults()
    {
        var l = Builders.ValidLoan() with { DebtToIncomeRatio = 2.00m };
        _sut.Validate(l).Should().BeEmpty();
    }
}
