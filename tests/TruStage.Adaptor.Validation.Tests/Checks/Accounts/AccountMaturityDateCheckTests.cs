using FluentAssertions;
using TruStage.Adaptor.Validation.Models;
using TruStage.Adaptor.Core.Models.Canonical;
using TruStage.Adaptor.Validation.Checks.Accounts;
using TruStage.Adaptor.Validation.Tests.Helpers;
using Xunit;

namespace TruStage.Adaptor.Validation.Tests.Checks.Accounts;

public sealed class AccountMaturityDateCheckTests
{
    private readonly AccountMaturityDateCheck _sut = new();

    [Fact]
    public void CheckId_IsAcc003() =>
        _sut.CheckId.Should().Be("ACC-003");

    [Fact]
    public void Validate_NullMaturityDate_ReturnsNoResults()
    {
        var a = Builders.ValidAccount() with { MaturityDate = null };
        _sut.Validate(a).Should().BeEmpty();
    }

    [Fact]
    public void Validate_CertificateWithFutureMaturityDate_ReturnsNoResults()
    {
        var openDate   = new DateOnly(2022, 1, 1);
        var maturity   = openDate.AddYears(1);
        var a = Builders.ValidAccount() with
        {
            AccountType  = AccountType.Certificate,
            OpenDate     = openDate,
            MaturityDate = maturity,
        };
        _sut.Validate(a).Should().BeEmpty();
    }

    // ── MaturityDate <= OpenDate → Error ──────────────────────────────────────

    [Fact]
    public void Validate_MaturityDateEqualToOpenDate_ReturnsError()
    {
        var date = new DateOnly(2020, 6, 1);
        var a = Builders.ValidAccount() with
        {
            AccountType  = AccountType.Certificate,
            OpenDate     = date,
            MaturityDate = date,
        };
        var results = _sut.Validate(a).ToList();

        results.Should().Contain(r =>
            r.FieldName == "MaturityDate" && r.Severity == CheckSeverity.Error);
    }

    [Fact]
    public void Validate_MaturityDateBeforeOpenDate_ReturnsError()
    {
        var openDate = new DateOnly(2022, 3, 1);
        var a = Builders.ValidAccount() with
        {
            AccountType  = AccountType.Certificate,
            OpenDate     = openDate,
            MaturityDate = openDate.AddDays(-1),
        };
        var results = _sut.Validate(a).ToList();

        results.Should().Contain(r =>
            r.FieldName == "MaturityDate" && r.Severity == CheckSeverity.Error);
    }

    // ── Non-term account type with MaturityDate → Warning ────────────────────

    [Fact]
    public void Validate_ShareAccountWithMaturityDate_ReturnsWarning()
    {
        var openDate = new DateOnly(2019, 1, 1);
        var a = Builders.ValidAccount() with
        {
            AccountType  = AccountType.Share,
            OpenDate     = openDate,
            MaturityDate = openDate.AddYears(2),
        };
        var results = _sut.Validate(a).ToList();

        results.Should().Contain(r =>
            r.FieldName == "MaturityDate" && r.Severity == CheckSeverity.Warning);
    }
}
