using FluentAssertions;
using TruStage.Adaptor.Validation.Models;
using TruStage.Adaptor.Validation.Checks.Loans;
using TruStage.Adaptor.Validation.Tests.Helpers;
using Xunit;

namespace TruStage.Adaptor.Validation.Tests.Checks.Loans;

public sealed class LoanAmountCheckTests
{
    private readonly LoanAmountCheck _sut = new();

    [Fact]
    public void CheckId_IsLn001() =>
        _sut.CheckId.Should().Be("LN-001");

    [Fact]
    public void Validate_ValidLoan_ReturnsNoResults()
    {
        var l = Builders.ValidLoan() with { LoanAmount = 10_000m, CurrentBalance = 7_500m };
        _sut.Validate(l).Should().BeEmpty();
    }

    // ── LoanAmount <= 0 → Error ────────────────────────────────────────────────

    [Theory]
    [InlineData(0.00)]
    [InlineData(-1.00)]
    [InlineData(-50_000.00)]
    public void Validate_NonPositiveLoanAmount_ReturnsError(decimal amount)
    {
        var l = Builders.ValidLoan() with { LoanAmount = amount };
        var results = _sut.Validate(l).ToList();

        results.Should().Contain(r =>
            r.FieldName == "LoanAmount" && r.Severity == CheckSeverity.Error);
    }

    // ── Negative CurrentBalance → Error ───────────────────────────────────────

    [Fact]
    public void Validate_NegativeCurrentBalance_ReturnsError()
    {
        var l = Builders.ValidLoan() with { CurrentBalance = -100m };
        var results = _sut.Validate(l).ToList();

        results.Should().Contain(r =>
            r.FieldName == "CurrentBalance" && r.Severity == CheckSeverity.Error);
    }

    // ── CurrentBalance > LoanAmount → Warning (accrued interest) ─────────────

    [Fact]
    public void Validate_CurrentBalanceExceedsLoanAmount_ReturnsWarning()
    {
        var l = Builders.ValidLoan() with { LoanAmount = 10_000m, CurrentBalance = 10_500m };
        var results = _sut.Validate(l).ToList();

        results.Should().ContainSingle(r =>
            r.FieldName == "CurrentBalance" && r.Severity == CheckSeverity.Warning);
    }

    [Fact]
    public void Validate_CurrentBalanceEqualsLoanAmount_ReturnsNoResults()
    {
        var l = Builders.ValidLoan() with { LoanAmount = 5_000m, CurrentBalance = 5_000m };
        _sut.Validate(l).Should().BeEmpty();
    }
}
