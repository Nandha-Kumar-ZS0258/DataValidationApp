using FluentAssertions;
using TruStage.Adaptor.Core.DataValidation.Models;
using TruStage.Adaptor.Validation.Models;
using TruStage.Adaptor.Validation.Tests.Helpers;
using Xunit;

namespace TruStage.Adaptor.Validation.Tests;

/// <summary>
/// Unit tests for <see cref="PipelineValidationService"/>.
///
/// These tests use fully in-memory canonical entities (no mocking required —
/// the service has no external dependencies).  Each test verifies gate
/// structure, snapshot counts, blocked-record filtering, or report assembly.
/// </summary>
public sealed class PipelineValidationServiceTests
{
    private readonly PipelineValidationService _sut = new();

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static DataSnapshot SourceSnapshot(
        int members      = 1,
        int accounts     = 1,
        int loans        = 1,
        int transactions = 1,
        int jointOwners  = 0)
        => new()
        {
            Stage            = DataSnapshot.Stages.Source,
            MemberCount      = members,
            AccountCount     = accounts,
            LoanCount        = loans,
            TransactionCount = transactions,
            JointOwnerCount  = jointOwners,
        };

    // ── RunGate1And2 — happy path ─────────────────────────────────────────────

    [Fact]
    public void RunGate1And2_CleanData_Gate1HasNoFailures()
    {
        var result = Builders.ValidResult();
        var source = SourceSnapshot();

        var gate = _sut.RunGate1And2(source, result);

        gate.Gate1.ConsistencyFailures.Should().BeEmpty();
        gate.Gate1.CountGaps.Should().BeEmpty();
    }

    [Fact]
    public void RunGate1And2_CleanData_Gate2HasNoFailures()
    {
        var result = Builders.ValidResult();
        var source = SourceSnapshot();

        var gate = _sut.RunGate1And2(source, result);

        gate.Gate2.ConsistencyFailures.Should().BeEmpty();
    }

    [Fact]
    public void RunGate1And2_CleanData_CleanResultMatchesInput()
    {
        var result = Builders.ValidResult();
        var source = SourceSnapshot();

        var gate = _sut.RunGate1And2(source, result);

        gate.CleanResult.Members.Should().HaveCount(result.Members.Count);
        gate.CleanResult.Accounts.Should().HaveCount(result.Accounts.Count);
        gate.CleanResult.Loans.Should().HaveCount(result.Loans.Count);
        gate.CleanResult.Transactions.Should().HaveCount(result.Transactions.Count);
    }

    // ── Gate IDs and descriptions ─────────────────────────────────────────────

    [Fact]
    public void RunGate1And2_Gate1HasCorrectMetadata()
    {
        var gate = _sut.RunGate1And2(SourceSnapshot(), Builders.ValidResult());

        gate.Gate1.GateId.Should().Be("Gate1");
        gate.Gate1.Description.Should().Contain("Transformed");
    }

    [Fact]
    public void RunGate1And2_Gate2HasCorrectMetadata()
    {
        var gate = _sut.RunGate1And2(SourceSnapshot(), Builders.ValidResult());

        gate.Gate2.GateId.Should().Be("Gate2");
        gate.Gate2.Description.Should().Contain("ReadyForProd");
    }

    // ── Transformed snapshot counts ───────────────────────────────────────────

    [Fact]
    public void RunGate1And2_TransformedSnapshotReflectsInputCounts()
    {
        var members  = new[] { Builders.ValidMember("M001"), Builders.ValidMember("M002") };
        var accounts = new[] { Builders.ValidAccount("A001") };
        var result   = Builders.ValidResult(members: members, accounts: accounts);
        var source   = SourceSnapshot(members: 2, accounts: 1);

        var gate = _sut.RunGate1And2(source, result);

        gate.TransformedSnapshot.MemberCount.Should().Be(2);
        gate.TransformedSnapshot.AccountCount.Should().Be(1);
        gate.TransformedSnapshot.Stage.Should().Be(DataSnapshot.Stages.Transformed);
    }

    // ── Error-severity records are blocked ────────────────────────────────────

    [Fact]
    public void RunGate1And2_MemberWithInvalidRequiredFields_IsBlockedFromCleanResult()
    {
        // MBR-001: blank FirstName → Error → member blocked
        var badMember = Builders.ValidMember("M001") with { FirstName = "Unknown" };
        var result    = Builders.ValidResult(members: [badMember]);
        var source    = SourceSnapshot();

        var gate = _sut.RunGate1And2(source, result);

        gate.CleanResult.Members.Should().BeEmpty("member with Error-severity finding must be blocked");
        gate.ReadyForProdSnapshot.DqBlocked.Should().Be(1);
    }

    [Fact]
    public void RunGate1And2_AccountWithNegativeBalance_IsBlockedFromCleanResult()
    {
        // ACC-001: negative balance → Error → account blocked
        var badAccount = Builders.ValidAccount("A001") with { Balance = -100m };
        var result     = Builders.ValidResult(accounts: [badAccount]);
        var source     = SourceSnapshot();

        var gate = _sut.RunGate1And2(source, result);

        gate.CleanResult.Accounts.Should().BeEmpty();
    }

    [Fact]
    public void RunGate1And2_LoanWithZeroAmount_IsBlockedFromCleanResult()
    {
        var badLoan = Builders.ValidLoan("L001") with { LoanAmount = 0m };
        var result  = Builders.ValidResult(loans: [badLoan]);
        var source  = SourceSnapshot();

        var gate = _sut.RunGate1And2(source, result);

        gate.CleanResult.Loans.Should().BeEmpty();
    }

    // ── Warning-severity records pass through ─────────────────────────────────

    [Fact]
    public void RunGate1And2_LoanBalanceExceedsLoanAmount_RecordPassesWithWarning()
    {
        // LN-001: CurrentBalance > LoanAmount → Warning (not Error) → record passes
        var warnedLoan = Builders.ValidLoan("L001") with
        {
            LoanAmount     = 10_000m,
            CurrentBalance = 11_000m,
        };
        var result = Builders.ValidResult(loans: [warnedLoan]);
        var source = SourceSnapshot();

        var gate = _sut.RunGate1And2(source, result);

        gate.CleanResult.Loans.Should().HaveCount(1, "Warning-severity records must not be blocked");
        gate.ReadyForProdSnapshot.DqWarnings.Should().BeGreaterThan(0);
    }

    // ── ReadyForProd snapshot ─────────────────────────────────────────────────

    [Fact]
    public void RunGate1And2_ReadyForProdSnapshotHasCorrectStage()
    {
        var gate = _sut.RunGate1And2(SourceSnapshot(), Builders.ValidResult());
        gate.ReadyForProdSnapshot.Stage.Should().Be(DataSnapshot.Stages.ReadyForProd);
    }

    // ── RunGate3AndBuildReport ────────────────────────────────────────────────

    [Fact]
    public void RunGate3AndBuildReport_AllCountsMatch_AllGatesPass()
    {
        var source          = SourceSnapshot(members: 2, accounts: 2, loans: 1, transactions: 5);
        var transformed     = source with { Stage = DataSnapshot.Stages.Transformed };
        var readyForProd    = source with { Stage = DataSnapshot.Stages.ReadyForProd };
        var prod            = source with { Stage = DataSnapshot.Stages.Prod };

        var gate1 = new ValidationGate
        {
            GateId = "Gate1", Description = "Source → Transformed",
            From = source, To = transformed,
            CountGaps = [], ConsistencyFailures = [],
        };
        var gate2 = new ValidationGate
        {
            GateId = "Gate2", Description = "Source → ReadyForProd",
            From = source, To = readyForProd,
            CountGaps = [], ConsistencyFailures = [],
        };

        var report = _sut.RunGate3AndBuildReport(
            source, transformed, readyForProd, prod,
            gate1, gate2,
            ingestionBatchId: "batch-001",
            cuId: "CU-TEST",
            sourceFileName: "members.csv");

        report.AllGatesPassed.Should().BeTrue();
        report.OverallStatus.Should().Be(PipelineValidationStatus.Passed);
        report.TotalErrors.Should().Be(0);
    }

    [Fact]
    public void RunGate3AndBuildReport_MembersLostInProd_Gate3Fails()
    {
        var source       = SourceSnapshot(members: 3);
        var transformed  = source with { Stage = DataSnapshot.Stages.Transformed };
        var readyForProd = source with { Stage = DataSnapshot.Stages.ReadyForProd };
        var prod         = source with { Stage = DataSnapshot.Stages.Prod, MemberCount = 2 };

        var gate1 = new ValidationGate
        {
            GateId = "Gate1", Description = "Source → Transformed",
            From = source, To = transformed,
            CountGaps = [], ConsistencyFailures = [],
        };
        var gate2 = new ValidationGate
        {
            GateId = "Gate2", Description = "Source → ReadyForProd",
            From = source, To = readyForProd,
            CountGaps = [], ConsistencyFailures = [],
        };

        var report = _sut.RunGate3AndBuildReport(
            source, transformed, readyForProd, prod,
            gate1, gate2,
            "batch-001", "CU-TEST", "data.csv");

        report.Gate3_SourceToProd.Passed.Should().BeFalse();
        report.AllGatesPassed.Should().BeFalse();
        report.OverallStatus.Should().Be(PipelineValidationStatus.PartiallyPassed);
    }

    [Fact]
    public void RunGate3AndBuildReport_TransactionDedup_IsInfoNotError()
    {
        // Transaction dedup (fewer in prod) is allowed — Info severity
        var source = SourceSnapshot(transactions: 10);
        var prod   = source with { Stage = DataSnapshot.Stages.Prod, TransactionCount = 8 };

        var emptyGate = new ValidationGate
        {
            GateId = "G", Description = "test",
            From = source, To = source,
            CountGaps = [], ConsistencyFailures = [],
        };

        var report = _sut.RunGate3AndBuildReport(
            source, source, source, prod,
            emptyGate, emptyGate,
            "batch-001", "CU-TEST", "data.csv");

        var txnGap = report.Gate3_SourceToProd.CountGaps
            .FirstOrDefault(g => g.EntityType == "Transaction");

        txnGap.Should().NotBeNull();
        txnGap!.Severity.Should().Be(GapSeverity.Info,
            "transaction deduplication must be Info, not Error");
    }

    // ── Report metadata ───────────────────────────────────────────────────────

    [Fact]
    public void RunGate3AndBuildReport_SetsIngestionMetadataCorrectly()
    {
        var snapshot = SourceSnapshot();
        var gate = new ValidationGate
        {
            GateId = "G", Description = "test",
            From = snapshot, To = snapshot,
            CountGaps = [], ConsistencyFailures = [],
        };

        var report = _sut.RunGate3AndBuildReport(
            snapshot, snapshot, snapshot, snapshot,
            gate, gate,
            ingestionBatchId: "BATCH-XYZ",
            cuId: "CU-ALPHA",
            sourceFileName: "upload.csv");

        report.IngestionBatchId.Should().Be("BATCH-XYZ");
        report.CuId.Should().Be("CU-ALPHA");
        report.SourceFileName.Should().Be("upload.csv");
        report.ReportedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
