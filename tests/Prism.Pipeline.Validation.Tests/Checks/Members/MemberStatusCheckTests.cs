using FluentAssertions;
using Prism.Pipeline.Validation.Models;
using Prism.Pipeline.Core.Models.Canonical;
using Prism.Pipeline.Validation.Checks.Members;
using Prism.Pipeline.Validation.Tests.Helpers;
using Xunit;

namespace Prism.Pipeline.Validation.Tests.Checks.Members;

public sealed class MemberStatusCheckTests
{
    private readonly MemberStatusCheck _sut = new();

    [Fact]
    public void CheckId_IsMbr005() =>
        _sut.CheckId.Should().Be("MBR-005");

    [Theory]
    [InlineData("Active")]
    [InlineData("Inactive")]
    [InlineData("Deceased")]
    [InlineData("Closed")]
    [InlineData("Suspended")]
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
