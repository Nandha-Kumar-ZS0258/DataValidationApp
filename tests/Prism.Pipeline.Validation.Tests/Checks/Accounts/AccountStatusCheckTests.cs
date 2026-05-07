using FluentAssertions;
using Prism.Pipeline.Validation.Models;
using Prism.Pipeline.Core.Models.Canonical;
using Prism.Pipeline.Validation.Checks.Accounts;
using Prism.Pipeline.Validation.Tests.Helpers;
using Xunit;

namespace Prism.Pipeline.Validation.Tests.Checks.Accounts;

public sealed class AccountStatusCheckTests
{
    private readonly AccountStatusCheck _sut = new();

    [Fact]
    public void CheckId_IsAcc004() =>
        _sut.CheckId.Should().Be("ACC-004");

    [Theory]
    [InlineData("Active")]
    [InlineData("Dormant")]
    [InlineData("Closed")]
    [InlineData("ChargedOff")]
    [InlineData("Frozen")]
    public void Validate_KnownStatus_ReturnsNoResults(string status)
    {
        var a = Builders.ValidAccount() with { AccountStatus = status };
        _sut.Validate(a).Should().BeEmpty();
    }

    [Theory]
    [InlineData("Share")]
    [InlineData("Draft/Checking")]
    [InlineData("MoneyMarket")]
    [InlineData("Certificate")]
    [InlineData("IRA")]
    [InlineData("HSA")]
    [InlineData("Other")]
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
