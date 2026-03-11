using FluentAssertions;
using TruStage.Adaptor.Core.DataValidation.Models;
using TruStage.Adaptor.Core.Models.Canonical;
using TruStage.Adaptor.Validation.Checks.Loans;
using TruStage.Adaptor.Validation.Tests.Helpers;
using Xunit;

namespace TruStage.Adaptor.Validation.Tests.Checks.Loans;

public sealed class LoanChargeOffCheckTests
{
    private readonly LoanChargeOffCheck _sut = new();

    [Fact]
    public void CheckId_IsLn003() =>
        _sut.CheckId.Should().Be("LN-003");

    [Fact]
    public void Validate_CurrentLoanWithoutChargeOffDate_ReturnsNoResults()
    {
        var l = Builders.ValidLoan() with
        {
            DelinquencyStatus = DelinquencyStatus.Current,
            ChargeOffDate     = null,
        };
        _sut.Validate(l).Should().BeEmpty();
    }

    [Fact]
    public void Validate_ChargedOffWithDate_ReturnsNoResults()
    {
        var origDate = new DateOnly(2020, 1, 1);
        var l = Builders.ValidLoan() with
        {
            OriginationDate   = origDate,
            DelinquencyStatus = DelinquencyStatus.ChargedOff,
            ChargeOffDate     = origDate.AddMonths(18),
        };
        _sut.Validate(l).Should().BeEmpty();
    }

    // ── ChargeOffDate present but status is not ChargedOff → Error ────────────

    [Fact]
    public void Validate_ChargeOffDateWithoutChargedOffStatus_ReturnsError()
    {
        var l = Builders.ValidLoan() with
        {
            DelinquencyStatus = DelinquencyStatus.Current,
            ChargeOffDate     = new DateOnly(2023, 6, 1),
        };
        var results = _sut.Validate(l).ToList();

        results.Should().Contain(r =>
            r.FieldName == "ChargeOffDate" && r.Severity == CheckSeverity.Error);
    }

    // ── ChargedOff status but no ChargeOffDate → Warning ─────────────────────

    [Fact]
    public void Validate_ChargedOffWithoutDate_ReturnsWarning()
    {
        var l = Builders.ValidLoan() with
        {
            DelinquencyStatus = DelinquencyStatus.ChargedOff,
            ChargeOffDate     = null,
        };
        var results = _sut.Validate(l).ToList();

        results.Should().ContainSingle(r =>
            r.FieldName == "ChargeOffDate" && r.Severity == CheckSeverity.Warning);
    }

    // ── ChargeOffDate before OriginationDate → Error ──────────────────────────

    [Fact]
    public void Validate_ChargeOffDateBeforeOriginationDate_ReturnsError()
    {
        var origDate = new DateOnly(2022, 3, 1);
        var l = Builders.ValidLoan() with
        {
            OriginationDate   = origDate,
            DelinquencyStatus = DelinquencyStatus.ChargedOff,
            ChargeOffDate     = origDate.AddDays(-1),
        };
        var results = _sut.Validate(l).ToList();

        results.Should().Contain(r =>
            r.FieldName == "ChargeOffDate" && r.Severity == CheckSeverity.Error
            && r.Message.Contains("OriginationDate"));
    }
}
