using FluentAssertions;
using Prism.Pipeline.Validation.Models;
using Prism.Pipeline.Validation.Checks.Members;
using Prism.Pipeline.Validation.Tests.Helpers;
using Xunit;

namespace Prism.Pipeline.Validation.Tests.Checks.Members;

public sealed class MemberDateOfBirthCheckTests
{
    private readonly MemberDateOfBirthCheck _sut = new();

    [Fact]
    public void CheckId_IsMbr003() =>
        _sut.CheckId.Should().Be("MBR-003");

    [Fact]
    public void Validate_NormalDateOfBirth_ReturnsNoResults()
    {
        var m = Builders.ValidMember() with { DateOfBirth = new DateOnly(1980, 6, 15) };
        _sut.Validate(m).Should().BeEmpty();
    }

    // ── Future date ───────────────────────────────────────────────────────────

    [Fact]
    public void Validate_FutureDateOfBirth_ReturnsError()
    {
        var future = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var m = Builders.ValidMember() with { DateOfBirth = future };
        var results = _sut.Validate(m).ToList();

        results.Should().ContainSingle();
        results[0].Severity.Should().Be(CheckSeverity.Error);
        results[0].FieldName.Should().Be("DateOfBirth");
    }

    // ── Pre-1900 date ─────────────────────────────────────────────────────────

    [Fact]
    public void Validate_DateBefore1900_ReturnsError()
    {
        var m = Builders.ValidMember() with { DateOfBirth = new DateOnly(1899, 12, 31) };
        var results = _sut.Validate(m).ToList();

        results.Should().ContainSingle();
        results[0].Severity.Should().Be(CheckSeverity.Error);
        results[0].FieldName.Should().Be("DateOfBirth");
    }

    [Fact]
    public void Validate_ExactlyJan1900_ReturnsNoError()
    {
        // 1900-01-01 is the minimum allowed year boundary — no Error should fire.
        // An age Warning may fire in future years when the implied age exceeds 120.
        var m = Builders.ValidMember() with { DateOfBirth = new DateOnly(1900, 1, 1) };
        _sut.Validate(m).Should().NotContain(r => r.Severity == CheckSeverity.Error);
    }

    // ── Implausibly old (> 120 years) → Warning, not Error ───────────────────

    [Fact]
    public void Validate_AgeOver120_ReturnsWarning()
    {
        var veryOld = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-125);
        var m = Builders.ValidMember() with { DateOfBirth = veryOld };
        var results = _sut.Validate(m).ToList();

        results.Should().ContainSingle();
        results[0].Severity.Should().Be(CheckSeverity.Warning);
        results[0].FieldName.Should().Be("DateOfBirth");
    }

    [Fact]
    public void Validate_AgeExactly120_ReturnsNoResults()
    {
        var exactly120 = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-120);
        var m = Builders.ValidMember() with { DateOfBirth = exactly120 };
        _sut.Validate(m).Should().BeEmpty();
    }
}
