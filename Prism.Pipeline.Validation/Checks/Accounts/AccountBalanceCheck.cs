using Prism.Pipeline.Validation.Models;
using Prism.Pipeline.Core.Models.Canonical;

namespace Prism.Pipeline.Validation.Checks.Accounts;

/// <summary>
/// ACC-001: Balance consistency rules.
///   - Balance must be >= 0
///   - AvailableBalance must be <= Balance (if present)
/// </summary>
public sealed class AccountBalanceCheck : IConsistencyCheck<CuAccount>
{
    public string CheckId   => "ACC-001";
    public string CheckName => "Account Balance Consistency";

    public IEnumerable<CheckResult> Validate(CuAccount a)
    {
        if (a.Balance < 0)
            yield return CheckResult.Error(CheckId, CheckName, "Account", a.AccountId,
                $"Balance {a.Balance:F2} is negative.",
                "Balance", a.Balance.ToString("F2"), ">= 0");

        if (a.AvailableBalance.HasValue && a.AvailableBalance.Value > a.Balance)
            yield return CheckResult.Error(CheckId, CheckName, "Account", a.AccountId,
                $"AvailableBalance {a.AvailableBalance:F2} exceeds Balance {a.Balance:F2}.",
                "AvailableBalance", a.AvailableBalance.ToString(), $"<= Balance ({a.Balance:F2})");
    }
}
