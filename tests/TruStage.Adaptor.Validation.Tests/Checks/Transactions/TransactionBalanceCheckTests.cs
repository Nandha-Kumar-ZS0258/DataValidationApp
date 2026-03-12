using FluentAssertions;
using TruStage.Adaptor.Validation.Models;
using TruStage.Adaptor.Validation.Checks.Transactions;
using TruStage.Adaptor.Validation.Tests.Helpers;
using Xunit;

namespace TruStage.Adaptor.Validation.Tests.Checks.Transactions;

public sealed class TransactionBalanceCheckTests
{
    private readonly TransactionBalanceCheck _sut = new();

    [Fact]
    public void CheckId_IsTxn003() =>
        _sut.CheckId.Should().Be("TXN-003");

    [Fact]
    public void Validate_NullBalanceAfter_ReturnsNoResults()
    {
        var t = Builders.ValidTransaction() with { BalanceAfter = null };
        _sut.Validate(t).Should().BeEmpty();
    }

    [Theory]
    [InlineData(0.00)]
    [InlineData(500.00)]
    [InlineData(10_000.00)]
    public void Validate_PositiveOrZeroBalanceAfter_ReturnsNoResults(decimal balance)
    {
        var t = Builders.ValidTransaction() with { BalanceAfter = balance };
        _sut.Validate(t).Should().BeEmpty();
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(-250.00)]
    public void Validate_NegativeBalanceAfter_ReturnsWarning(decimal balance)
    {
        var t = Builders.ValidTransaction() with { BalanceAfter = balance };
        var results = _sut.Validate(t).ToList();

        results.Should().ContainSingle(r =>
            r.FieldName == "BalanceAfter" && r.Severity == CheckSeverity.Warning);
    }
}
