using FluentAssertions;
using TruStage.Adaptor.Core.DataValidation.Models;
using TruStage.Adaptor.Validation.Checks.CrossEntity;
using TruStage.Adaptor.Validation.Tests.Helpers;
using Xunit;

namespace TruStage.Adaptor.Validation.Tests.Checks.CrossEntity;

public sealed class JointOwnerSelfRefCheckTests
{
    private readonly JointOwnerSelfRefCheck _sut = new();

    [Fact]
    public void CheckId_IsRef003() =>
        _sut.CheckId.Should().Be("REF-003");

    [Fact]
    public void Validate_ValidJointOwner_ReturnsNoResults()
    {
        var members     = new[] { Builders.ValidMember("M001"), Builders.ValidMember("M002") };
        var jointOwners = new[] { Builders.ValidJointOwner("M001", "M002") };

        _sut.Validate(members, jointOwners).Should().BeEmpty();
    }

    [Fact]
    public void Validate_EmptyJointOwners_ReturnsNoResults()
    {
        var members = new[] { Builders.ValidMember("M001") };
        _sut.Validate(members, []).Should().BeEmpty();
    }

    // ── Self-reference → Error ────────────────────────────────────────────────

    [Fact]
    public void Validate_PrimaryAndJointAreSameMember_ReturnsError()
    {
        var members     = new[] { Builders.ValidMember("M001") };
        var jointOwners = new[] { Builders.ValidJointOwner("M001", "M001") };

        var results = _sut.Validate(members, jointOwners).ToList();

        results.Should().Contain(r =>
            r.Severity == CheckSeverity.Error &&
            r.Message.Contains("cannot be their own joint owner"));
    }

    // ── PrimaryMemberId not in batch → Error ──────────────────────────────────

    [Fact]
    public void Validate_PrimaryMemberNotInBatch_ReturnsError()
    {
        var members     = new[] { Builders.ValidMember("M002") };
        var jointOwners = new[] { Builders.ValidJointOwner("M999", "M002") };

        var results = _sut.Validate(members, jointOwners).ToList();

        results.Should().Contain(r =>
            r.FieldName == "PrimaryMemberId" && r.Severity == CheckSeverity.Error);
    }

    // ── JointMemberId not in batch → Warning (may be from another batch) ──────

    [Fact]
    public void Validate_JointMemberNotInBatch_ReturnsWarning()
    {
        var members     = new[] { Builders.ValidMember("M001") };
        var jointOwners = new[] { Builders.ValidJointOwner("M001", "M999") };

        var results = _sut.Validate(members, jointOwners).ToList();

        results.Should().Contain(r =>
            r.FieldName == "JointMemberId" && r.Severity == CheckSeverity.Warning);
    }
}
