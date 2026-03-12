using FluentAssertions;
using TruStage.Adaptor.Validation.Models;
using TruStage.Adaptor.Core.Models.Canonical;
using TruStage.Adaptor.Validation.Checks.Accounts;
using TruStage.Adaptor.Validation.Tests.Helpers;
using Xunit;

namespace TruStage.Adaptor.Validation.Tests.Checks.Accounts;

public sealed class AccountStatusCheckTests
{
    private readonly AccountStatusCheck _sut = new();

    [Fact]
    public void CheckId_IsAcc004() =>
        _sut.CheckId.Should().Be("ACC-004");

    [Theory]
    [InlineData(AccountStatus.Active)]
    [InlineData(AccountStatus.Dormant)]
    [InlineData(AccountStatus.Closed)]
    [InlineData(AccountStatus.ChargedOff)]
    [InlineData(AccountStatus.Frozen)]
    public void Validate_KnownStatus_ReturnsNoResults(string status)
    {
        var a = Builders.ValidAccount() with { AccountStatus = status };
        _sut.Validate(a).Should().BeEmpty();
    }

    [Theory]
    [InlineData(AccountType.Share)]
    [InlineData(AccountType.DraftChecking)]
    [InlineData(AccountType.MoneyMarket)]
    [InlineData(AccountType.Certificate)]
    [InlineData(AccountType.Ira)]
    [InlineData(AccountType.Hsa)]
    [InlineData(AccountType.Other)]
    public void Validate_KnownAccountType_ReturnsNoResults(string type)
    {
        var a = Builders.ValidAccount() with { AccountType = type };
        _sut.Validate(a).Should().BeEmpty();
    }

    // ── Unknown status ────────────────────────────────────────────────────────

    [Theory]
    [InlineData("UNKNOWN")]
    [InlineData("Pending")]
    [InlineData("")]
    public void Validate_UnknownStatus_ReturnsError(string status)
    {
        var a = Builders.ValidAccount() with { AccountStatus = status };
        var results = _sut.Validate(a).ToList();

        results.Should().Contain(r =>
            r.FieldName == "AccountStatus" && r.Severity == CheckSeverity.Error);
    }

    // ── Unknown type → Warning (not Error) ───────────────────────────────────

    [Theory]
    [InlineData("Savings")]
    [InlineData("Checking")]
    [InlineData("Unknown")]
    public void Validate_UnknownAccountType_ReturnsWarning(string type)
    {
        var a = Builders.ValidAccount() with { AccountType = type };
        var results = _sut.Validate(a).ToList();

        results.Should().Contain(r =>
            r.FieldName == "AccountType" && r.Severity == CheckSeverity.Warning);
    }

    // ── OpenDate = MinValue → Error ───────────────────────────────────────────

    [Fact]
    public void Validate_OpenDateMinValue_ReturnsError()
    {
        var a = Builders.ValidAccount() with { OpenDate = DateOnly.MinValue };
        var results = _sut.Validate(a).ToList();

        results.Should().Contain(r =>
            r.FieldName == "OpenDate" && r.Severity == CheckSeverity.Error);
    }

    [Fact]
    public void Validate_ValidOpenDate_DoesNotReturnOpenDateError()
    {
        var a = Builders.ValidAccount() with { OpenDate = new DateOnly(2015, 5, 1) };
        _sut.Validate(a).Should().NotContain(r => r.FieldName == "OpenDate");
    }
}
