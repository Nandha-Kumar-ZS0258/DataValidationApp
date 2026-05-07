using Prism.Pipeline.Validation.Models;
using Prism.Pipeline.Core.Models.Canonical;

namespace Prism.Pipeline.Validation.Checks.Transactions;

/// <summary>
/// TXN-001: Transaction Amount must not be zero.
/// A zero-amount transaction is a data anomaly (not a credit or debit).
/// </summary>
public sealed class TransactionAmountCheck : IConsistencyCheck<CuTransaction>
{
    public string CheckId   => "TXN-001";
    public string CheckName => "Transaction Amount Non-Zero";

    public IEnumerable<CheckResult> Validate(CuTransaction t)
    {
        if (t.Amount == 0)
            yield return CheckResult.Error(CheckId, CheckName, "Transaction", t.TransactionId,
                "Transaction Amount is zero. Every transaction must be a credit (positive) or debit (negative).",
                "Amount", "0", "!= 0");
    }
}
