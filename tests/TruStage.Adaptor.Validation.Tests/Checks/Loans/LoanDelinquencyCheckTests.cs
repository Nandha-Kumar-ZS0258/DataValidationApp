using FluentAssertions;
using TruStage.Adaptor.Core.DataValidation.Models;
using TruStage.Adaptor.Core.Models.Canonical;
using TruStage.Adaptor.Validation.Checks.Loans;
using TruStage.Adaptor.Validation.Tests.Helpers;
using Xunit;

namespace TruStage.Adaptor.Validation.Tests.Checks.Loans;

public sealed class LoanDelinquencyCheckTests
{
    private readonly LoanDelinquencyCheck _sut = new();

    [Fact]
    public void CheckId_IsLn002() =>
        _sut.CheckId.Should().Be("LN-002");

    [Fact]
    public void Validate_NullDaysPastDue_ReturnsNoResults()
    {
        var l = Builders.ValidLoan() with { DaysPastDue = null };
        _sut.Validate(l).Should().BeEmpty();
    }

    [Theory]
    [InlineData(0,  DelinquencyStatus.Current)]
    [InlineData(15, DelinquencyStatus.Current)]
    [InlineData(29, DelinquencyStatus.Current)]
    [InlineData(30, DelinquencyStatus.Delinquent30)]
    [InlineData(59, DelinquencyStatus.Delinquent30)]
    [InlineData(60, DelinquencyStatus.Delinquent60)]
    [InlineData(89, DelinquencyStatus.Delinquent60)]
    [InlineData(90, DelinquencyStatus.Delinquent90Plus)]
    [InlineData(180, DelinquencyStatus.Delinquent90Plus)]
    public void Validate_CorrectStatusForDpd_ReturnsNoResults(int dpd, string status)
    {
        var l = Builders.ValidLoan() with { DaysPastDue = dpd, DelinquencyStatus = status };
        _sut.Validate(l).Should().BeEmpty();
    }

    [Theory]
    [InlineData(0,  DelinquencyStatus.Delinquent30)]
    [InlineData(29, DelinquencyStatus.Delinquent30)]
    [InlineData(30, DelinquencyStatus.Current)]
    [InlineData(90, DelinquencyStatus.Delinquent30)]
    public void Validate_IncorrectStatusForDpd_ReturnsError(int dpd, string status)
    {
        var l = Builders.ValidLoan() with { DaysPastDue = dpd, DelinquencyStatus = status };
        var results = _sut.Validate(l).ToList();

        results.Should().ContainSingle(r =>
            r.FieldName == "DelinquencyStatus" && r.Severity == CheckSeverity.Error);
    }

    [Fact]
    public void Validate_ChargedOff_AlwaysPassesRegardlessOfDpd()
    {
        var l = Builders.ValidLoan() with
        {
            DaysPastDue       = 5,
            DelinquencyStatus = DelinquencyStatus.ChargedOff,
        };
        _sut.Validate(l).Should().BeEmpty();
    }

    [Fact]
    public void Validate_Paid_AlwaysPassesRegardlessOfDpd()
    {
        var l = Builders.ValidLoan() with
        {
            DaysPastDue       = 120,
            DelinquencyStatus = DelinquencyStatus.Paid,
        };
        _sut.Validate(l).Should().BeEmpty();
    }
}
