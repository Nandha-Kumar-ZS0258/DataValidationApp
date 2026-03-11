using FluentAssertions;
using TruStage.Adaptor.Core.DataValidation.Models;
using TruStage.Adaptor.Validation.Checks.Transactions;
using TruStage.Adaptor.Validation.Tests.Helpers;
using Xunit;

namespace TruStage.Adaptor.Validation.Tests.Checks.Transactions;

public sealed class TransactionAmountCheckTests
{
    private readonly TransactionAmountCheck _sut = new();

    [Fact]
    public void CheckId_IsTxn001() =>
        _sut.CheckId.Should().Be("TXN-001");

    [Theory]
    [InlineData(100.00)]
    [InlineData(-50.00)]
    [InlineData(0.01)]
    [InlineData(-0.01)]
    public void Validate_NonZeroAmount_ReturnsNoResults(decimal amount)
    {
        var t = Builders.ValidTransaction() with { Amount = amount };
        _sut.Validate(t).Should().BeEmpty();
    }

    [Fact]
    public void Validate_ZeroAmount_ReturnsError()
    {
        var t = Builders.ValidTransaction() with { Amount = 0m };
        var results = _sut.Validate(t).ToList();

        results.Should().ContainSingle();
        results[0].Severity.Should().Be(CheckSeverity.Error);
        results[0].FieldName.Should().Be("Amount");
        results[0].ActualValue.Should().Be("0");
    }
}
