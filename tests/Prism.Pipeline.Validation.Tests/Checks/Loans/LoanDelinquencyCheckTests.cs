using FluentAssertions;
using Prism.Pipeline.Validation.Models;
using Prism.Pipeline.Core.Models.Canonical;
using Prism.Pipeline.Validation.Checks.Loans;
using Prism.Pipeline.Validation.Tests.Helpers;
using Xunit;

namespace Prism.Pipeline.Validation.Tests.Checks.Loans;

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
    [InlineData(0,   "Current")]
    [InlineData(15,  "Current")]
    [InlineData(29,  "Current")]
    [InlineData(30,  "Delinquent30")]
    [InlineData(59,  "Delinquent30")]
    [InlineData(60,  "Delinquent60")]
    [InlineData(89,  "Delinquent60")]
    [InlineData(90,  "Delinquent90Plus")]
    [InlineData(180, "Delinquent90Plus")]
    public void Validate_CorrectStatusForDpd_ReturnsNoResults(int dpd, string status)
    {
        var l = Builders.ValidLoan() with { DaysPastDue = dpd, DelinquencyStatus = status };
        _sut.Validate(l).Should().BeEmpty();
    }

    [Theory]
    [InlineData(0,  "Delinquent30")]
    [InlineData(29, "Delinquent30")]
    [InlineData(30, "Current")]
    [InlineData(90, "Delinquent30")]
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
            DelinquencyStatus = "ChargedOff",
        };
        _sut.Validate(l).Should().BeEmpty();
    }

    [Fact]
    public void Validate_Paid_AlwaysPassesRegardlessOfDpd()
    {
        var l = Builders.ValidLoan() with
        {
            DaysPastDue       = 120,
            DelinquencyStatus = "Paid",
        };
        _sut.Validate(l).Should().BeEmpty();
    }
}
