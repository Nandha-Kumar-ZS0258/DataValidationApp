using FluentAssertions;
using TruStage.Adaptor.Core.DataValidation.Models;
using TruStage.Adaptor.Validation.Checks.CrossEntity;
using TruStage.Adaptor.Validation.Tests.Helpers;
using Xunit;

namespace TruStage.Adaptor.Validation.Tests.Checks.CrossEntity;

public sealed class DuplicateKeyCheckTests
{
    private readonly DuplicateKeyCheck _sut = new();

    [Fact]
    public void CheckId_IsRef004() =>
        _sut.CheckId.Should().Be("REF-004");

    [Fact]
    public void Validate_UniqueKeys_ReturnsNoResults()
    {
        var members  = new[] { Builders.ValidMember("M001"), Builders.ValidMember("M002") };
        var accounts = new[] { Builders.ValidAccount("A001"), Builders.ValidAccount("A002") };
        var loans    = new[] { Builders.ValidLoan("L001"),    Builders.ValidLoan("L002") };

        _sut.Validate(members, accounts, loans).Should().BeEmpty();
    }

    // ── Duplicate MemberId ────────────────────────────────────────────────────

    [Fact]
    public void Validate_DuplicateMemberIds_ReturnsError()
    {
        var members = new[] { Builders.ValidMember("M001"), Builders.ValidMember("M001") };

        var results = _sut.Validate(members, [], []).ToList();

        results.Should().ContainSingle(r =>
            r.EntityType == "Member" &&
            r.EntityKey  == "M001"   &&
            r.Severity   == CheckSeverity.Error);
    }

    // ── Duplicate AccountId ────────────────────────────────────────────────────

    [Fact]
    public void Validate_DuplicateAccountIds_ReturnsError()
    {
        var accounts = new[] { Builders.ValidAccount("A001"), Builders.ValidAccount("A001") };

        var results = _sut.Validate([], accounts, []).ToList();

        results.Should().ContainSingle(r =>
            r.EntityType == "Account" &&
            r.EntityKey  == "A001"    &&
            r.Severity   == CheckSeverity.Error);
    }

    // ── Duplicate LoanId ──────────────────────────────────────────────────────

    [Fact]
    public void Validate_DuplicateLoanIds_ReturnsError()
    {
        var loans = new[] { Builders.ValidLoan("L001"), Builders.ValidLoan("L001") };

        var results = _sut.Validate([], [], loans).ToList();

        results.Should().ContainSingle(r =>
            r.EntityType == "Loan" &&
            r.EntityKey  == "L001" &&
            r.Severity   == CheckSeverity.Error);
    }

    // ── Duplicate detection is case-insensitive ────────────────────────────────

    [Fact]
    public void Validate_DuplicateMemberIdCaseInsensitive_ReturnsError()
    {
        var members = new[] { Builders.ValidMember("m001"), Builders.ValidMember("M001") };

        var results = _sut.Validate(members, [], []).ToList();
        results.Should().ContainSingle(r => r.EntityType == "Member");
    }

    // ── Three duplicates of same key → only one error per duplicate value ─────

    [Fact]
    public void Validate_TriplicateMemberId_ReturnsOneError()
    {
        var members = new[]
        {
            Builders.ValidMember("M001"),
            Builders.ValidMember("M001"),
            Builders.ValidMember("M001"),
        };

        var results = _sut.Validate(members, [], []).ToList();
        results.Should().ContainSingle(r => r.EntityKey == "M001");
    }
}
