using Prism.Pipeline.Core.Models;
using Prism.Pipeline.Core.Models.Canonical;

namespace Prism.Pipeline.Validation.Tests.Helpers;

/// <summary>
/// Factory helpers that create fully valid canonical entities for use in tests.
/// Override only the fields relevant to the scenario under test.
/// </summary>
internal static class Builders
{
    private static readonly DateOnly Today       = DateOnly.FromDateTime(DateTime.UtcNow);
    private static readonly DateOnly Yesterday   = Today.AddDays(-1);
    private static readonly DateOnly Dob1980     = new(1980, 6, 15);
    private static readonly DateOnly JoinDate    = new(2005, 3, 20);

    // ── Members ───────────────────────────────────────────────────────────────

    internal static CuMember ValidMember(string memberId = "M001") => new()
    {
        CuId             = "CU-TEST",
        MemberId         = memberId,
        FirstName        = "Jane",
        LastName         = "Smith",
        DateOfBirth      = Dob1980,
        MemberStatus     = "Active",
        IngestionBatchId = "batch-001",
        EffectiveDate    = Yesterday,
    };

    // ── Accounts ──────────────────────────────────────────────────────────────

    internal static CuAccount ValidAccount(
        string accountId = "A001",
        string memberId  = "M001") => new()
    {
        CuId             = "CU-TEST",
        AccountId        = accountId,
        MemberId         = memberId,
        AccountType      = "Share",
        AccountStatus    = "Active",
        OpenDate         = new DateOnly(2010, 1, 15),
        Balance          = 1_500.00m,
        IngestionBatchId = "batch-001",
        EffectiveDate    = Yesterday,
    };

    // ── Loans ─────────────────────────────────────────────────────────────────

    internal static CuLoan ValidLoan(
        string loanId    = "L001",
        string memberId  = "M001") => new()
    {
        CuId              = "CU-TEST",
        LoanId            = loanId,
        MemberId          = memberId,
        LoanType          = LoanType.Auto,
        OriginationDate   = new DateOnly(2020, 4, 10),
        LoanAmount        = 20_000.00m,
        CurrentBalance    = 14_500.00m,
        InterestRate      = 0.065m,
        DelinquencyStatus = "Current",
        DaysPastDue       = 0,
        IngestionBatchId  = "batch-001",
        EffectiveDate     = Yesterday,
    };

    // ── Transactions ──────────────────────────────────────────────────────────

    internal static CuTransaction ValidTransaction(
        string transactionId = "T001",
        string accountId     = "A001",
        string memberId      = "M001") => new()
    {
        CuId             = "CU-TEST",
        TransactionId    = transactionId,
        AccountId        = accountId,
        MemberId         = memberId,
        TransactionDate  = DateTime.UtcNow.AddDays(-2),
        Amount           = 250.00m,
        TransactionType  = TransactionType.DirectDeposit,
        IngestionBatchId = "batch-001",
    };

    // ── Joint Owners ──────────────────────────────────────────────────────────

    internal static CuJointOwner ValidJointOwner(
        string primaryMemberId = "M001",
        string jointMemberId   = "M002",
        string accountId       = "A001") => new()
    {
        CuId             = "CU-TEST",
        PrimaryMemberId  = primaryMemberId,
        JointMemberId    = jointMemberId,
        AccountId        = accountId,
        EffectiveDate    = Yesterday,
        IngestionBatchId = "batch-001",
    };

    // ── CanonicalPipelineResult ────────────────────────────────────────────────

    internal static CanonicalPipelineResult ValidResult(
        IReadOnlyList<CuMember>?      members      = null,
        IReadOnlyList<CuAccount>?     accounts     = null,
        IReadOnlyList<CuLoan>?        loans        = null,
        IReadOnlyList<CuTransaction>? transactions = null,
        IReadOnlyList<CuJointOwner>?  jointOwners  = null)
        => CanonicalPipelineResult.Success(
            creditUnionId:  "CU-TEST",
            sourceFilePath: "test-data.csv",
            members:        members      ?? [ValidMember()],
            accounts:       accounts     ?? [ValidAccount()],
            loans:          loans        ?? [ValidLoan()],
            transactions:   transactions ?? [ValidTransaction()],
            jointOwners:    jointOwners  ?? [],
            batch:          ValidBatch(),
            registry:       ValidRegistry());

    // ── Support models ────────────────────────────────────────────────────────

    internal static IngestionBatch ValidBatch(string batchId = "batch-001") => new()
    {
        IngestionBatchId = batchId,
        CuId             = "CU-TEST",
        RunType          = BatchRunType.Bau,
        SourceFilePath   = "test-data.csv",
    };

    internal static CuRegistry ValidRegistry() => new()
    {
        CuId             = "CU-TEST",
        CuName           = "Test Credit Union",
        SourceFileFormat = SourceFileFormat.Json,
    };
}
