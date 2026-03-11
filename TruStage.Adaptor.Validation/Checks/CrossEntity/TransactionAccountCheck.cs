using TruStage.Adaptor.Core.DataValidation.Models;
using TruStage.Adaptor.Core.Models.Canonical;

namespace TruStage.Adaptor.Validation.Checks.CrossEntity;

/// <summary>
/// REF-002: Every Transaction must reference an AccountId that exists in the
/// same batch's Accounts collection.
/// </summary>
public sealed class TransactionAccountCheck
{
    public string CheckId   => "REF-002";
    public string CheckName => "Referential Integrity — Transaction → Account";

    public IEnumerable<CheckResult> Validate(
        IReadOnlyList<CuAccount>     accounts,
        IReadOnlyList<CuTransaction> transactions)
    {
        var accountIds = accounts.Select(a => a.AccountId).ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var t in transactions)
        {
            if (!accountIds.Contains(t.AccountId))
                yield return CheckResult.Error(CheckId, CheckName, "Transaction", t.TransactionId,
                    $"Transaction references AccountId '{t.AccountId}' which does not exist in the mapped Accounts.",
                    "AccountId", t.AccountId, "Must exist in Accounts");
        }
    }
}
