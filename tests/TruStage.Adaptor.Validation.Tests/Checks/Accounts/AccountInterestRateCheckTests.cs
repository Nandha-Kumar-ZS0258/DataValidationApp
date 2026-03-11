using FluentAssertions;
using TruStage.Adaptor.Core.DataValidation.Models;
using TruStage.Adaptor.Validation.Checks.Accounts;
using TruStage.Adaptor.Validation.Tests.Helpers;
using Xunit;

namespace TruStage.Adaptor.Validation.Tests.Checks.Accounts;

public sealed class AccountInterestRateCheckTests
{
    private readonly AccountInterestRateCheck _sut = new();

    [Fact]
    public void CheckId_IsAcc002() =>
        _sut.CheckId.Should().Be("ACC-002");

    [Fact]
    public void Validate_NullInterestRate_ReturnsNoResults()
    {
        var a = Builders.ValidAccount() with { InterestRate = null };
        _sut.Validate(a).Should().BeEmpty();
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(0.005)]
    [InlineData(0.05)]
    [InlineData(1.0)]
    public void Validate_ValidDecimalRate_ReturnsNoResults(double rate)
    {
        var a = Builders.ValidAccount() with { InterestRate = (decimal)rate };
        _sut.Validate(a).Should().BeEmpty();
    }

    [Fact]
    public void Validate_NegativeRate_ReturnsError()
    {
        var a = Builders.ValidAccount() with { InterestRate = -0.01m };
        var results = _sut.Validate(a).ToList();

        results.Should().ContainSingle(r =>
            r.FieldName == "InterestRate" && r.Severity == CheckSeverity.Error);
    }

    [Theory]
    [InlineData(1.01)]
    [InlineData(5.0)]
    [InlineData(100.0)]
    public void Validate_RateAboveOne_ReturnsWarning(double rate)
    {
        var a = Builders.ValidAccount() with { InterestRate = (decimal)rate };
        var results = _sut.Validate(a).ToList();

        results.Should().ContainSingle(r =>
            r.FieldName == "InterestRate" && r.Severity == CheckSeverity.Warning);
    }
}
