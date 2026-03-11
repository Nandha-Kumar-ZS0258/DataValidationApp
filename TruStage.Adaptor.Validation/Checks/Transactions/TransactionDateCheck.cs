using TruStage.Adaptor.Core.DataValidation.Models;
using TruStage.Adaptor.Core.Models.Canonical;

namespace TruStage.Adaptor.Validation.Checks.Transactions;

/// <summary>
/// TXN-002: TransactionDate must not be in the future and not before 1900-01-01.
/// </summary>
public sealed class TransactionDateCheck : IConsistencyCheck<CuTransaction>
{
    public string CheckId   => "TXN-002";
    public string CheckName => "Transaction Date Validity";

    private static readonly DateTime MinDate = new(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public IEnumerable<CheckResult> Validate(CuTransaction t)
    {
        var now = DateTime.UtcNow;

        if (t.TransactionDate > now)
            yield return CheckResult.Error(CheckId, CheckName, "Transaction", t.TransactionId,
                $"TransactionDate '{t.TransactionDate:yyyy-MM-dd}' is in the future.",
                "TransactionDate", t.TransactionDate.ToString("yyyy-MM-dd"), $"<= {now:yyyy-MM-dd}");

        if (t.TransactionDate < MinDate)
            yield return CheckResult.Error(CheckId, CheckName, "Transaction", t.TransactionId,
                $"TransactionDate '{t.TransactionDate:yyyy-MM-dd}' is before the minimum allowed date {MinDate:yyyy-MM-dd}.",
                "TransactionDate", t.TransactionDate.ToString("yyyy-MM-dd"), $">= {MinDate:yyyy-MM-dd}");
    }
}
