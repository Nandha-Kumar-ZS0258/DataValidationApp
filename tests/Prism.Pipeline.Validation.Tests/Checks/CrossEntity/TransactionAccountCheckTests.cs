using FluentAssertions;
using Prism.Pipeline.Validation.Models;
using Prism.Pipeline.Validation.Checks.CrossEntity;
using Prism.Pipeline.Validation.Tests.Helpers;
using Xunit;

namespace Prism.Pipeline.Validation.Tests.Checks.CrossEntity;

public sealed class TransactionAccountCheckTests
{
    private readonly TransactionAccountCheck _sut = new();

    [Fact]
    public void CheckId_IsRef002() =>
        _sut.CheckId.Should().Be("REF-002");

    [Fact]
    public void Validate_AllTransactionsReferenceKnownAccounts_ReturnsNoResults()
    {
        var accounts     = new[] { Builders.ValidAccount("A001"), Builders.ValidAccount("A002") };
        var transactions = new[]
        {
            Builders.ValidTransaction("T001", "A001"),
            Builders.ValidTransaction("T002", "A002"),
        };

        _sut.Validate(accounts, transactions).Should().BeEmpty();
    }

    [Fact]
    public void Validate_TransactionReferencesUnknownAccount_ReturnsError()
    {
        var accounts     = new[] { Builders.ValidAccount("A001") };
        var transactions = new[] { Builders.ValidTransaction("T001", "A999") };

        var results = _sut.Validate(accounts, transactions).ToList();

        results.Should().ContainSingle(r =>
            r.EntityType == "Transaction" &&
            r.EntityKey  == "T001"        &&
            r.Severity   == CheckSeverity.Error);
    }

    [Fact]
    public void Validate_AccountIdLookupIsCaseInsensitive()
    {
        var accounts     = new[] { Builders.ValidAccount("a001") };
        var transactions = new[] { Builders.ValidTransaction("T001", "A001") };

        _sut.Validate(accounts, transactions).Should().BeEmpty();
    }

    [Fact]
    public void Validate_EmptyCollections_ReturnsNoResults()
    {
        _sut.Validate([], []).Should().BeEmpty();
    }
}
