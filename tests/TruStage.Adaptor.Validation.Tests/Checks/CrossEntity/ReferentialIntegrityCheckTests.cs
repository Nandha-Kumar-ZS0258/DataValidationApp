using FluentAssertions;
using TruStage.Adaptor.Core.DataValidation.Models;
using TruStage.Adaptor.Validation.Checks.CrossEntity;
using TruStage.Adaptor.Validation.Tests.Helpers;
using Xunit;

namespace TruStage.Adaptor.Validation.Tests.Checks.CrossEntity;

public sealed class ReferentialIntegrityCheckTests
{
    private readonly ReferentialIntegrityCheck _sut = new();

    [Fact]
    public void CheckId_IsRef001() =>
        _sut.CheckId.Should().Be("REF-001");

    [Fact]
    public void Validate_AllAccountsAndLoansReferenceKnownMembers_ReturnsNoResults()
    {
        var members  = new[] { Builders.ValidMember("M001"), Builders.ValidMember("M002") };
        var accounts = new[] { Builders.ValidAccount("A001", "M001"), Builders.ValidAccount("A002", "M002") };
        var loans    = new[] { Builders.ValidLoan("L001", "M001") };

        _sut.Validate(members, accounts, loans).Should().BeEmpty();
    }

    [Fact]
    public void Validate_AccountReferencesUnknownMember_ReturnsError()
    {
        var members  = new[] { Builders.ValidMember("M001") };
        var accounts = new[] { Builders.ValidAccount("A001", "M999") };
        var loans    = Array.Empty<TruStage.Adaptor.Core.Models.Canonical.CuLoan>();

        var results = _sut.Validate(members, accounts, loans).ToList();

        results.Should().ContainSingle(r =>
            r.EntityType == "Account" &&
            r.EntityKey  == "A001"    &&
            r.Severity   == CheckSeverity.Error);
    }

    [Fact]
    public void Validate_LoanReferencesUnknownMember_ReturnsError()
    {
        var members  = new[] { Builders.ValidMember("M001") };
        var accounts = Array.Empty<TruStage.Adaptor.Core.Models.Canonical.CuAccount>();
        var loans    = new[] { Builders.ValidLoan("L001", "M999") };

        var results = _sut.Validate(members, accounts, loans).ToList();

        results.Should().ContainSingle(r =>
            r.EntityType == "Loan" &&
            r.EntityKey  == "L001" &&
            r.Severity   == CheckSeverity.Error);
    }

    [Fact]
    public void Validate_MemberIdLookupIsCaseInsensitive()
    {
        var members  = new[] { Builders.ValidMember("m001") };  // lower-case
        var accounts = new[] { Builders.ValidAccount("A001", "M001") };  // upper-case
        var loans    = Array.Empty<TruStage.Adaptor.Core.Models.Canonical.CuLoan>();

        _sut.Validate(members, accounts, loans).Should().BeEmpty();
    }

    [Fact]
    public void Validate_EmptyBatch_ReturnsNoResults()
    {
        _sut.Validate([], [], []).Should().BeEmpty();
    }

    [Fact]
    public void Validate_MultipleOrphanAccounts_ReturnsOneErrorEach()
    {
        var members  = new[] { Builders.ValidMember("M001") };
        var accounts = new[]
        {
            Builders.ValidAccount("A001", "UNKNOWN1"),
            Builders.ValidAccount("A002", "UNKNOWN2"),
        };
        var loans = Array.Empty<TruStage.Adaptor.Core.Models.Canonical.CuLoan>();

        var results = _sut.Validate(members, accounts, loans).ToList();
        results.Should().HaveCount(2);
    }
}
