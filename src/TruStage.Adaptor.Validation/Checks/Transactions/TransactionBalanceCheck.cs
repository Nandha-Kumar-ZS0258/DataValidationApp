using TruStage.Adaptor.Validation.Models;
using TruStage.Adaptor.Core.Models.Canonical;

namespace TruStage.Adaptor.Validation.Checks.Transactions;

/// <summary>
/// TXN-003: BalanceAfter (if present) should be >= 0 for standard share accounts.
/// A negative balance is a warning — it can legitimately occur with overdraft protection.
/// </summary>
public sealed class TransactionBalanceCheck : IConsistencyCheck<CuTransaction>
{
    public string CheckId   => "TXN-003";
    public string CheckName => "Transaction Balance After Non-Negative";

    public IEnumerable<CheckResult> Validate(CuTransaction t)
    {
        if (t.BalanceAfter.HasValue && t.BalanceAfter.Value < 0)
            yield return CheckResult.Warning(CheckId, CheckName, "Transaction", t.TransactionId,
                $"BalanceAfter {t.BalanceAfter:F2} is negative. This may be valid with overdraft protection, but should be reviewed.",
                "BalanceAfter", t.BalanceAfter.ToString(), ">= 0 (unless overdraft)");
    }
}
