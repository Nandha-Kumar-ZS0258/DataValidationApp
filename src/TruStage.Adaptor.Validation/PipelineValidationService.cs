using TruStage.Adaptor.Core.DataValidation.Models;
using TruStage.Adaptor.Core.Models;
using TruStage.Adaptor.Core.Models.Canonical;
using TruStage.Adaptor.Validation.Checks;
using TruStage.Adaptor.Validation.Checks.Accounts;
using TruStage.Adaptor.Validation.Checks.CrossEntity;
using TruStage.Adaptor.Validation.Checks.Loans;
using TruStage.Adaptor.Validation.Checks.Members;
using TruStage.Adaptor.Validation.Checks.Transactions;
using TruStage.Adaptor.Validation.Models;

namespace TruStage.Adaptor.Validation;

/// <summary>
/// Runs all consistency checks and assembles the full three-gate
/// <see cref="PipelineValidationReport"/> for one ingestion run.
/// </summary>
public sealed class PipelineValidationService : IPipelineValidationService
{
    // ── Gate 1: field-level checks run on the Transformed snapshot ───────────
    private static readonly IConsistencyCheck<CuMember>[]      MemberGate1Checks =
    [
        new MemberRequiredFieldsCheck(),
        new MemberStatusCheck(),
        new MemberDateOfBirthCheck()
    ];

    private static readonly IConsistencyCheck<CuAccount>[]     AccountGate1Checks =
    [
        new AccountStatusCheck()
    ];

    private static readonly IConsistencyCheck<CuLoan>[]        LoanGate1Checks =
    [
        new LoanOriginationDateCheck()
    ];

    private static readonly IConsistencyCheck<CuTransaction>[] TxnGate1Checks =
    [
        new TransactionAmountCheck(),
        new TransactionDateCheck()
    ];

    // ── Gate 2: deeper business-rule checks run on the Transformed snapshot ──
    private static readonly IConsistencyCheck<CuMember>[]      MemberGate2Checks =
    [
        new MemberCreditScoreCheck(),
        new MemberMembershipDateCheck(),
        new MemberContactCheck()
    ];

    private static readonly IConsistencyCheck<CuAccount>[]     AccountGate2Checks =
    [
        new AccountBalanceCheck(),
        new AccountInterestRateCheck(),
        new AccountMaturityDateCheck()
    ];

    private static readonly IConsistencyCheck<CuLoan>[]        LoanGate2Checks =
    [
        new LoanAmountCheck(),
        new LoanDelinquencyCheck(),
        new LoanChargeOffCheck(),
        new LoanRateCheck()
    ];

    private static readonly IConsistencyCheck<CuTransaction>[] TxnGate2Checks =
    [
        new TransactionBalanceCheck()
    ];

    // ── Cross-entity checks (run once per batch on the full collection) ───────
    private static readonly ReferentialIntegrityCheck RefIntegrityCheck = new();
    private static readonly TransactionAccountCheck   TxnAccountCheck   = new();
    private static readonly JointOwnerSelfRefCheck    JointOwnerCheck   = new();
    private static readonly DuplicateKeyCheck         DuplicateCheck    = new();

    // ─────────────────────────────────────────────────────────────────────────

    public Gate1And2Result RunGate1And2(
        DataSnapshot           sourceSnapshot,
        CanonicalAdapterResult transformedResult)
    {
        var members      = transformedResult.Members;
        var accounts     = transformedResult.Accounts;
        var loans        = transformedResult.Loans;
        var transactions = transformedResult.Transactions;
        var jointOwners  = transformedResult.JointOwners;

        // ── Transformed snapshot ─────────────────────────────────────────────
        var transformedSnapshot = new DataSnapshot
        {
            Stage          = DataSnapshot.Stages.Transformed,
            MemberCount     = members.Count,
            AccountCount    = accounts.Count,
            LoanCount       = loans.Count,
            TransactionCount= transactions.Count,
            JointOwnerCount = jointOwners.Count,
            MappingErrors  = transformedResult.Errors.Count
        };

        // ── Gate 1: count reconciliation + basic field checks ────────────────
        var gate1CountGaps    = BuildGate1CountGaps(sourceSnapshot, transformedSnapshot);
        var gate1Consistency  = RunPerEntityChecks(members, accounts, loans, transactions, MemberGate1Checks, AccountGate1Checks, LoanGate1Checks, TxnGate1Checks);

        var gate1 = new ValidationGate
        {
            GateId               = "Gate1",
            Description          = "Source → Transformed",
            From                 = sourceSnapshot,
            To                   = transformedSnapshot,
            CountGaps            = gate1CountGaps,
            ConsistencyFailures  = gate1Consistency
        };

        // ── Gate 2: business-rule checks + cross-entity checks ───────────────
        var gate2Consistency = RunPerEntityChecks(members, accounts, loans, transactions, MemberGate2Checks, AccountGate2Checks, LoanGate2Checks, TxnGate2Checks);

        // Cross-entity checks
        var crossChecks = new List<CheckResult>();
        crossChecks.AddRange(RefIntegrityCheck.Validate(members, accounts, loans));
        crossChecks.AddRange(TxnAccountCheck.Validate(accounts, transactions));
        crossChecks.AddRange(JointOwnerCheck.Validate(members, jointOwners));
        crossChecks.AddRange(DuplicateCheck.Validate(members, accounts, loans));

        var allGate2Failures = gate2Consistency.Concat(crossChecks).ToList();

        // All errors from both gates contribute to blocking decisions
        var allErrors = gate1Consistency.Concat(allGate2Failures).ToList();

        // Determine which records are blocked (have at least one Error-severity finding)
        var blockedMemberIds  = allErrors
            .Where(f => f.Severity == CheckSeverity.Error && f.EntityType == "Member")
            .Select(f => f.EntityKey).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var blockedAccountIds = allErrors
            .Where(f => f.Severity == CheckSeverity.Error && f.EntityType == "Account")
            .Select(f => f.EntityKey).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var blockedLoanIds = allErrors
            .Where(f => f.Severity == CheckSeverity.Error && f.EntityType == "Loan")
            .Select(f => f.EntityKey).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var blockedTxnIds = allErrors
            .Where(f => f.Severity == CheckSeverity.Error && f.EntityType == "Transaction")
            .Select(f => f.EntityKey).ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Warning-severity records pass through but are counted
        var warnedCount = allErrors
            .Where(f => f.Severity == CheckSeverity.Warning)
            .Select(f => f.EntityKey).Distinct().Count();

        // Build clean collections (exclude blocked records)
        var cleanMembers      = members     .Where(m => !blockedMemberIds .Contains(m.MemberId))     .ToList();
        var cleanAccounts     = accounts    .Where(a => !blockedAccountIds.Contains(a.AccountId))    .ToList();
        var cleanLoans        = loans       .Where(l => !blockedLoanIds   .Contains(l.LoanId))       .ToList();
        var cleanTransactions = transactions.Where(t => !blockedTxnIds    .Contains(t.TransactionId)).ToList();

        // ── ReadyForProd snapshot ────────────────────────────────────────────
        var readyForProdSnapshot = new DataSnapshot
        {
            Stage          = DataSnapshot.Stages.ReadyForProd,
            MemberCount     = cleanMembers.Count,
            AccountCount    = cleanAccounts.Count,
            LoanCount       = cleanLoans.Count,
            TransactionCount= cleanTransactions.Count,
            JointOwnerCount = jointOwners.Count,
            DqBlocked      = blockedMemberIds.Count + blockedAccountIds.Count + blockedLoanIds.Count + blockedTxnIds.Count,
            DqWarnings     = warnedCount
        };

        var gate2CountGaps = BuildGate2CountGaps(sourceSnapshot, readyForProdSnapshot);

        var gate2 = new ValidationGate
        {
            GateId              = "Gate2",
            Description         = "Source → ReadyForProd",
            From                = sourceSnapshot,
            To                  = readyForProdSnapshot,
            CountGaps           = gate2CountGaps,
            ConsistencyFailures = allGate2Failures
        };

        // Build clean CanonicalAdapterResult
        var cleanResult = transformedResult.IsSuccess
            ? CanonicalAdapterResult.Success(
                transformedResult.CreditUnionId, transformedResult.SourceFilePath,
                cleanMembers, cleanAccounts, cleanLoans, cleanTransactions,
                jointOwners, transformedResult.Batch, transformedResult.Registry)
            : CanonicalAdapterResult.Partial(
                transformedResult.CreditUnionId, transformedResult.SourceFilePath,
                cleanMembers, cleanAccounts, cleanLoans, cleanTransactions,
                jointOwners, transformedResult.Batch, transformedResult.Registry,
                transformedResult.Errors);

        return new Gate1And2Result
        {
            TransformedSnapshot  = transformedSnapshot,
            ReadyForProdSnapshot = readyForProdSnapshot,
            Gate1                = gate1,
            Gate2                = gate2,
            CleanResult          = cleanResult
        };
    }

    public PipelineValidationReport RunGate3AndBuildReport(
        DataSnapshot  sourceSnapshot,
        DataSnapshot  transformedSnapshot,
        DataSnapshot  readyForProdSnapshot,
        DataSnapshot  prodSnapshot,
        ValidationGate gate1,
        ValidationGate gate2,
        string         ingestionBatchId,
        string         cuId,
        string         sourceFileName)
    {
        var gate3 = BuildGate3(sourceSnapshot, prodSnapshot);

        return new PipelineValidationReport
        {
            IngestionBatchId     = ingestionBatchId,
            CuId                 = cuId,
            SourceFileName       = sourceFileName,
            SourceSnapshot       = sourceSnapshot,
            TransformedSnapshot  = transformedSnapshot,
            ReadyForProdSnapshot = readyForProdSnapshot,
            ProdSnapshot         = prodSnapshot,
            Gate1_SourceToTransformed   = gate1,
            Gate2_SourceToReadyForProd  = gate2,
            Gate3_SourceToProd          = gate3
        };
    }

    // ── Gate builders ─────────────────────────────────────────────────────────

    private static List<ReconciliationGap> BuildGate1CountGaps(
        DataSnapshot source, DataSnapshot transformed)
    {
        var gaps = new List<ReconciliationGap>();

        // Declared vs actual in source
        if (source.DeclaredMemberCount.HasValue &&
            source.DeclaredMemberCount != source.MemberCount)
            gaps.Add(new ReconciliationGap
            {
                EntityType = "Member",
                Expected   = source.DeclaredMemberCount.Value,
                Actual     = source.MemberCount,
                Message    = $"Source file declared {source.DeclaredMemberCount} members " +
                             $"but contained {source.MemberCount}.",
                Severity   = GapSeverity.Error
            });

        AddCountGap(gaps, "Member",
            source.MemberCount,
            transformed.MemberCount + transformed.MappingErrors,
            "members lost during mapping (not accounted for by mapping errors).");

        AddCountGap(gaps, "Account",
            source.AccountCount, transformed.AccountCount,
            "accounts lost during mapping.");

        AddCountGap(gaps, "Loan",
            source.LoanCount, transformed.LoanCount,
            "loans lost during mapping.");

        AddCountGap(gaps, "Transaction",
            source.TransactionCount, transformed.TransactionCount,
            "transactions lost during mapping.");

        return gaps;
    }

    private static List<ReconciliationGap> BuildGate2CountGaps(
        DataSnapshot source, DataSnapshot readyForProd)
    {
        var gaps = new List<ReconciliationGap>();
        var totalBlocked = readyForProd.DqBlocked;

        if (totalBlocked > 0)
            gaps.Add(new ReconciliationGap
            {
                EntityType = "All",
                Expected   = source.MemberCount,
                Actual     = readyForProd.MemberCount,
                Message    = $"{totalBlocked} record(s) blocked by DQ consistency checks and excluded from DB write.",
                Severity   = GapSeverity.Warning
            });

        // Any unexplained gap beyond blocked records is an error
        var unexplained = source.MemberCount - readyForProd.MemberCount - readyForProd.DqBlocked;
        if (unexplained > 0)
            gaps.Add(new ReconciliationGap
            {
                EntityType = "Member",
                Expected   = source.MemberCount,
                Actual     = readyForProd.MemberCount,
                Message    = $"{unexplained} member(s) silently dropped between Transformed and ReadyForProd stages.",
                Severity   = GapSeverity.Error
            });

        return gaps;
    }

    private static ValidationGate BuildGate3(DataSnapshot source, DataSnapshot prod)
    {
        var gaps = new List<ReconciliationGap>();

        // Transactions allow a dedup gap (Info, not Error)
        var txnGap = source.TransactionCount - prod.TransactionCount;
        if (txnGap > 0)
            gaps.Add(new ReconciliationGap
            {
                EntityType = "Transaction",
                Expected   = source.TransactionCount,
                Actual     = prod.TransactionCount,
                Message    = $"{txnGap} transaction(s) not in DB. May be expected deduplication of records already present.",
                Severity   = GapSeverity.Info
            });

        // All other entity types must fully match
        AddCountGap(gaps, "Member",     source.MemberCount,      prod.MemberCount,      "members missing from DB after SaveAsync.");
        AddCountGap(gaps, "Account",    source.AccountCount,     prod.AccountCount,     "accounts missing from DB after SaveAsync.");
        AddCountGap(gaps, "Loan",       source.LoanCount,        prod.LoanCount,        "loans missing from DB after SaveAsync.");
        AddCountGap(gaps, "JointOwner", source.JointOwnerCount,  prod.JointOwnerCount,  "joint owner records missing from DB after SaveAsync.");

        return new ValidationGate
        {
            GateId              = "Gate3",
            Description         = "Source → Prod (end-to-end)",
            From                = source,
            To                  = prod,
            CountGaps           = gaps,
            ConsistencyFailures = Array.Empty<CheckResult>()
        };
    }

    private static void AddCountGap(
        List<ReconciliationGap> gaps, string entity,
        int expected, int actual, string lossMessage)
    {
        if (expected == actual) return;

        var missing = expected - actual;
        gaps.Add(new ReconciliationGap
        {
            EntityType = entity,
            Expected   = expected,
            Actual     = actual,
            Message    = missing > 0
                ? $"{missing} {entity} row(s) {lossMessage}"
                : $"{Math.Abs(missing)} unexpected extra {entity} row(s) found.",
            Severity   = missing > 0 ? GapSeverity.Error : GapSeverity.Warning
        });
    }

    // ── Per-entity check runner ───────────────────────────────────────────────

    private static List<CheckResult> RunPerEntityChecks(
        IReadOnlyList<CuMember>      members,
        IReadOnlyList<CuAccount>     accounts,
        IReadOnlyList<CuLoan>        loans,
        IReadOnlyList<CuTransaction> transactions,
        IConsistencyCheck<CuMember>[]      memberChecks,
        IConsistencyCheck<CuAccount>[]     accountChecks,
        IConsistencyCheck<CuLoan>[]        loanChecks,
        IConsistencyCheck<CuTransaction>[] txnChecks)
    {
        var results = new List<CheckResult>();

        foreach (var m in members)
            foreach (var check in memberChecks)
                results.AddRange(check.Validate(m));

        foreach (var a in accounts)
            foreach (var check in accountChecks)
                results.AddRange(check.Validate(a));

        foreach (var l in loans)
            foreach (var check in loanChecks)
                results.AddRange(check.Validate(l));

        foreach (var t in transactions)
            foreach (var check in txnChecks)
                results.AddRange(check.Validate(t));

        return results;
    }
}
