using FluentAssertions;
using TruStage.Adaptor.Core.DataValidation.Models;
using TruStage.Adaptor.Core.Models.Canonical;
using TruStage.Adaptor.Validation.Checks.Members;
using TruStage.Adaptor.Validation.Tests.Helpers;
using Xunit;

namespace TruStage.Adaptor.Validation.Tests.Checks.Members;

public sealed class MemberStatusCheckTests
{
    private readonly MemberStatusCheck _sut = new();

    [Fact]
    public void CheckId_IsMbr005() =>
        _sut.CheckId.Should().Be("MBR-005");

    [Theory]
    [InlineData(MemberStatus.Active)]
    [InlineData(MemberStatus.Inactive)]
    [InlineData(MemberStatus.Deceased)]
    [InlineData(MemberStatus.Closed)]
    [InlineData(MemberStatus.Suspended)]
    public void Validate_AllKnownStatuses_ReturnsNoResults(string status)
    {
        var m = Builders.ValidMember() with { MemberStatus = status };
        _sut.Validate(m).Should().BeEmpty();
    }

    [Theory]
    [InlineData("active")]    // case-insensitive — should still pass
    [InlineData("ACTIVE")]
    [InlineData("Inactive")]
    public void Validate_KnownStatusesCaseInsensitive_ReturnsNoResults(string status)
    {
        var m = Builders.ValidMember() with { MemberStatus = status };
        _sut.Validate(m).Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("Unknown")]
    [InlineData("PENDING")]
    [InlineData("Expired")]
    [InlineData("Archived")]
    public void Validate_UnknownStatus_ReturnsError(string status)
    {
        var m = Builders.ValidMember() with { MemberStatus = status };
        var results = _sut.Validate(m).ToList();

        results.Should().ContainSingle();
        results[0].Severity.Should().Be(CheckSeverity.Error);
        results[0].FieldName.Should().Be("MemberStatus");
        results[0].ActualValue.Should().Be(status);
    }
}
