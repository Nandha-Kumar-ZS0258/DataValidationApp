using TruStage.Adaptor.Validation.Models;
using TruStage.Adaptor.Core.Models.Canonical;

namespace TruStage.Adaptor.Validation.Checks.Accounts;

/// <summary>
/// ACC-002: InterestRate (dividend rate) must be stored as a decimal fraction (0–1.0).
/// A value > 1.0 suggests the rate was not divided by 100 during mapping.
/// </summary>
public sealed class AccountInterestRateCheck : IConsistencyCheck<CuAccount>
{
    public string CheckId   => "ACC-002";
    public string CheckName => "Account Interest Rate Range";

    public IEnumerable<CheckResult> Validate(CuAccount a)
    {
        if (a.InterestRate is null) yield break;

        if (a.InterestRate < 0)
            yield return CheckResult.Error(CheckId, CheckName, "Account", a.AccountId,
                $"InterestRate {a.InterestRate} is negative.",
                "InterestRate", a.InterestRate.ToString(), ">= 0");

        else if (a.InterestRate > 1.0m)
            yield return CheckResult.Warning(CheckId, CheckName, "Account", a.AccountId,
                $"InterestRate {a.InterestRate} is > 1.0. Rate should be stored as decimal fraction (e.g. 0.05 for 5%), not percentage.",
                "InterestRate", a.InterestRate.ToString(), "0–1.0 (decimal fraction)");
    }
}
