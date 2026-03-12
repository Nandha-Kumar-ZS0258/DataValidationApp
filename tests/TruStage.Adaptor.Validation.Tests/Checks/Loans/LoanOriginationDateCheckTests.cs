using FluentAssertions;
using TruStage.Adaptor.Validation.Models;
using TruStage.Adaptor.Validation.Checks.Loans;
using TruStage.Adaptor.Validation.Tests.Helpers;
using Xunit;

namespace TruStage.Adaptor.Validation.Tests.Checks.Loans;

public sealed class LoanOriginationDateCheckTests
{
    private readonly LoanOriginationDateCheck _sut = new();

    [Fact]
    public void CheckId_IsLn005() =>
        _sut.CheckId.Should().Be("LN-005");

    [Fact]
    public void Validate_ValidLoan_ReturnsNoResults()
    {
        var l = Builders.ValidLoan() with { OriginationDate = new DateOnly(2018, 7, 1) };
        _sut.Validate(l).Should().BeEmpty();
    }

    [Fact]
    public void Validate_FutureOriginationDate_ReturnsError()
    {
        var future = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var l = Builders.ValidLoan() with { OriginationDate = future };
        var results = _sut.Validate(l).ToList();

        results.Should().Contain(r =>
            r.FieldName == "OriginationDate" && r.Severity == CheckSeverity.Error);
    }

    [Fact]
    public void Validate_DateBefore1900_ReturnsError()
    {
        var l = Builders.ValidLoan() with { OriginationDate = new DateOnly(1899, 1, 1) };
        var results = _sut.Validate(l).ToList();

        results.Should().Contain(r =>
            r.FieldName == "OriginationDate" && r.Severity == CheckSeverity.Error);
    }

    [Fact]
    public void Validate_PaymentDueDateBeforeOriginationDate_ReturnsError()
    {
        var origDate = new DateOnly(2020, 1, 1);
        var l = Builders.ValidLoan() with
        {
            OriginationDate = origDate,
            PaymentDueDate  = origDate.AddDays(-1),
        };
        var results = _sut.Validate(l).ToList();

        results.Should().Contain(r =>
            r.FieldName == "PaymentDueDate" && r.Severity == CheckSeverity.Error);
    }

    [Fact]
    public void Validate_PaymentDueDateAfterOriginationDate_ReturnsNoResults()
    {
        var origDate = new DateOnly(2020, 1, 1);
        var l = Builders.ValidLoan() with
        {
            OriginationDate = origDate,
            PaymentDueDate  = origDate.AddMonths(1),
        };
        _sut.Validate(l).Should().BeEmpty();
    }
}
