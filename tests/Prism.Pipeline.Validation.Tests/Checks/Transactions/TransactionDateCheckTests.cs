using FluentAssertions;
using Prism.Pipeline.Validation.Models;
using Prism.Pipeline.Validation.Checks.Transactions;
using Prism.Pipeline.Validation.Tests.Helpers;
using Xunit;

namespace Prism.Pipeline.Validation.Tests.Checks.Transactions;

public sealed class TransactionDateCheckTests
{
    private readonly TransactionDateCheck _sut = new();

    [Fact]
    public void CheckId_IsTxn002() =>
        _sut.CheckId.Should().Be("TXN-002");

    [Fact]
    public void Validate_RecentTransactionDate_ReturnsNoResults()
    {
        var t = Builders.ValidTransaction() with
        {
            TransactionDate = DateTime.UtcNow.AddDays(-5),
        };
        _sut.Validate(t).Should().BeEmpty();
    }

    [Fact]
    public void Validate_FutureTransactionDate_ReturnsError()
    {
        var t = Builders.ValidTransaction() with
        {
            TransactionDate = DateTime.UtcNow.AddDays(1),
        };
        var results = _sut.Validate(t).ToList();

        results.Should().ContainSingle(r =>
            r.FieldName == "TransactionDate" && r.Severity == CheckSeverity.Error);
    }

    [Fact]
    public void Validate_DateBefore1900_ReturnsError()
    {
        var t = Builders.ValidTransaction() with
        {
            TransactionDate = new DateTime(1899, 12, 31, 0, 0, 0, DateTimeKind.Utc),
        };
        var results = _sut.Validate(t).ToList();

        results.Should().ContainSingle(r =>
            r.FieldName == "TransactionDate" && r.Severity == CheckSeverity.Error);
    }

    [Fact]
    public void Validate_ExactlyJan1900_ReturnsNoResults()
    {
        var t = Builders.ValidTransaction() with
        {
            TransactionDate = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        };
        _sut.Validate(t).Should().BeEmpty();
    }
}
