using FluentAssertions;
using TruStage.Adaptor.Core.DataValidation.Models;
using TruStage.Adaptor.Validation.Checks.Members;
using TruStage.Adaptor.Validation.Tests.Helpers;
using Xunit;

namespace TruStage.Adaptor.Validation.Tests.Checks.Members;

public sealed class MemberContactCheckTests
{
    private readonly MemberContactCheck _sut = new();

    [Fact]
    public void CheckId_IsMbr006() =>
        _sut.CheckId.Should().Be("MBR-006");

    [Fact]
    public void Validate_NullEmailAndState_ReturnsNoResults()
    {
        var m = Builders.ValidMember() with { Email = null, State = null };
        _sut.Validate(m).Should().BeEmpty();
    }

    // ── Email ─────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("user@example.com")]
    [InlineData("jane.doe+alias@company.org")]
    [InlineData("TEST@DOMAIN.COM")]
    public void Validate_ValidEmail_ReturnsNoResults(string email)
    {
        var m = Builders.ValidMember() with { Email = email };
        _sut.Validate(m).Should().BeEmpty();
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("missing@tld")]
    [InlineData("@nodomain.com")]
    [InlineData("spaces in@email.com")]
    public void Validate_InvalidEmail_ReturnsWarning(string email)
    {
        var m = Builders.ValidMember() with { Email = email };
        var results = _sut.Validate(m).ToList();

        results.Should().ContainSingle(r =>
            r.FieldName == "Email" && r.Severity == CheckSeverity.Warning);
    }

    // ── State ─────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("CA")]
    [InlineData("TX")]
    [InlineData("NY")]
    [InlineData("PR")]
    [InlineData("DC")]
    public void Validate_ValidState_ReturnsNoResults(string state)
    {
        var m = Builders.ValidMember() with { State = state };
        _sut.Validate(m).Should().BeEmpty();
    }

    [Theory]
    [InlineData("XX")]
    [InlineData("ZZ")]
    [InlineData("California")]
    public void Validate_InvalidState_ReturnsWarning(string state)
    {
        var m = Builders.ValidMember() with { State = state };
        var results = _sut.Validate(m).ToList();

        results.Should().ContainSingle(r =>
            r.FieldName == "State" && r.Severity == CheckSeverity.Warning);
    }

    [Fact]
    public void Validate_BothEmailAndStateInvalid_ReturnsTwoWarnings()
    {
        var m = Builders.ValidMember() with { Email = "invalid", State = "ZZ" };
        var results = _sut.Validate(m).ToList();

        results.Should().HaveCount(2);
        results.Should().AllSatisfy(r => r.Severity.Should().Be(CheckSeverity.Warning));
    }
}
