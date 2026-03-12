using FluentAssertions;
using TruStage.Adaptor.Validation.Models;
using TruStage.Adaptor.Validation.Checks.Accounts;
using TruStage.Adaptor.Validation.Tests.Helpers;
using Xunit;

namespace TruStage.Adaptor.Validation.Tests.Checks.Accounts;

public sealed class AccountBalanceCheckTests
{
    private readonly AccountBalanceCheck _sut = new();

    [Fact]
    public void CheckId_IsAcc001() =>
        _sut.CheckId.Should().Be("ACC-001");

    [Fact]
    public void Validate_ValidAccount_ReturnsNoResults()
    {
        var a = Builders.ValidAccount() with { Balance = 500m, AvailableBalance = 400m };
        _sut.Validate(a).Should().BeEmpty();
    }

    [Fact]
    public void Validate_ZeroBalance_ReturnsNoResults()
    {
        var a = Builders.ValidAccount() with { Balance = 0m };
        _sut.Validate(a).Should().BeEmpty();
    }

    [Fact]
    public void Validate_NullAvailableBalance_ReturnsNoResults()
    {
        var a = Builders.ValidAccount() with { Balance = 100m, AvailableBalance = null };
        _sut.Validate(a).Should().BeEmpty();
    }

    // ── Negative balance ──────────────────────────────────────────────────────

    [Theory]
    [InlineData(-0.01)]
    [InlineData(-100.00)]
    [InlineData(-9999.99)]
    public void Validate_NegativeBalance_ReturnsError(decimal balance)
    {
        var a = Builders.ValidAccount() with { Balance = balance };
        var results = _sut.Validate(a).ToList();

        results.Should().ContainSingle(r =>
            r.FieldName == "Balance" && r.Severity == CheckSeverity.Error);
    }

    // ── AvailableBalance > Balance ────────────────────────────────────────────

    [Fact]
    public void Validate_AvailableBalanceExceedsBalance_ReturnsError()
    {
        var a = Builders.ValidAccount() with { Balance = 100m, AvailableBalance = 101m };
        var results = _sut.Validate(a).ToList();

        results.Should().ContainSingle(r =>
            r.FieldName == "AvailableBalance" && r.Severity == CheckSeverity.Error);
    }

    [Fact]
    public void Validate_AvailableBalanceEqualsBalance_ReturnsNoResults()
    {
        var a = Builders.ValidAccount() with { Balance = 250m, AvailableBalance = 250m };
        _sut.Validate(a).Should().BeEmpty();
    }
}
