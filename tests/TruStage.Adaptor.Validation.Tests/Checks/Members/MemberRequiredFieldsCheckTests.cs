using FluentAssertions;
using TruStage.Adaptor.Validation.Models;
using TruStage.Adaptor.Validation.Checks.Members;
using TruStage.Adaptor.Validation.Tests.Helpers;
using Xunit;

namespace TruStage.Adaptor.Validation.Tests.Checks.Members;

public sealed class MemberRequiredFieldsCheckTests
{
    private readonly MemberRequiredFieldsCheck _sut = new();

    [Fact]
    public void CheckId_IsMbr001() =>
        _sut.CheckId.Should().Be("MBR-001");

    [Fact]
    public void Validate_ValidMember_ReturnsNoResults()
    {
        var results = _sut.Validate(Builders.ValidMember()).ToList();
        results.Should().BeEmpty();
    }

    // ── MemberId ──────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_BlankMemberId_ReturnsError(string? memberId)
    {
        var m = Builders.ValidMember() with { MemberId = memberId! };
        var results = _sut.Validate(m).ToList();

        results.Should().ContainSingle(r =>
            r.FieldName == "MemberId" && r.Severity == CheckSeverity.Error);
    }

    // ── FirstName ─────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("Unknown")]
    public void Validate_InvalidFirstName_ReturnsError(string firstName)
    {
        var m = Builders.ValidMember() with { FirstName = firstName };
        var results = _sut.Validate(m).ToList();

        results.Should().ContainSingle(r =>
            r.FieldName == "FirstName" && r.Severity == CheckSeverity.Error);
    }

    [Fact]
    public void Validate_ValidFirstName_DoesNotReturnFirstNameError()
    {
        var m = Builders.ValidMember() with { FirstName = "Alice" };
        _sut.Validate(m).Should().NotContain(r => r.FieldName == "FirstName");
    }

    // ── LastName ──────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("Unknown")]
    public void Validate_InvalidLastName_ReturnsError(string lastName)
    {
        var m = Builders.ValidMember() with { LastName = lastName };
        var results = _sut.Validate(m).ToList();

        results.Should().ContainSingle(r =>
            r.FieldName == "LastName" && r.Severity == CheckSeverity.Error);
    }

    // ── DateOfBirth ───────────────────────────────────────────────────────────

    [Fact]
    public void Validate_DefaultDateOfBirth_ReturnsError()
    {
        var m = Builders.ValidMember() with { DateOfBirth = default };
        var results = _sut.Validate(m).ToList();

        results.Should().ContainSingle(r =>
            r.FieldName == "DateOfBirth" && r.Severity == CheckSeverity.Error);
    }

    [Fact]
    public void Validate_MinValueDateOfBirth_ReturnsError()
    {
        var m = Builders.ValidMember() with { DateOfBirth = DateOnly.MinValue };
        var results = _sut.Validate(m).ToList();

        results.Should().ContainSingle(r =>
            r.FieldName == "DateOfBirth" && r.Severity == CheckSeverity.Error);
    }

    // ── Multiple failures ─────────────────────────────────────────────────────

    [Fact]
    public void Validate_MultipleInvalidFields_ReturnsOneErrorPerField()
    {
        var m = Builders.ValidMember() with
        {
            FirstName   = "Unknown",
            LastName    = "",
            DateOfBirth = DateOnly.MinValue,
        };

        var results = _sut.Validate(m).ToList();
        results.Should().HaveCount(3);
        results.Should().AllSatisfy(r => r.Severity.Should().Be(CheckSeverity.Error));
    }
}
