using Prism.Pipeline.Validation.Models;
using Prism.Pipeline.Core.Models.Canonical;

namespace Prism.Pipeline.Validation.Checks.Loans;

/// <summary>
/// LN-004: InterestRate and DebtToIncomeRatio range checks.
///   - InterestRate stored as decimal fraction (0–0.30 = 0%–30%)
///   - DebtToIncomeRatio range 0–2.0 (0%–200%)
/// </summary>
public sealed class LoanRateCheck : IConsistencyCheck<CuLoan>
{
    public string CheckId   => "LN-004";
    public string CheckName => "Loan Rate and DTI Range";

    public IEnumerable<CheckResult> Validate(CuLoan l)
    {
        if (l.InterestRate < 0)
            yield return CheckResult.Error(CheckId, CheckName, "Loan", l.LoanId,
                $"InterestRate {l.InterestRate} is negative.",
                "InterestRate", l.InterestRate.ToString(), ">= 0");

        else if (l.InterestRate > 0.30m)
            yield return CheckResult.Warning(CheckId, CheckName, "Loan", l.LoanId,
                $"InterestRate {l.InterestRate:P2} exceeds 30%, which is unusually high. " +
                "Verify the rate was correctly divided by 100 during mapping.",
                "InterestRate", l.InterestRate.ToString(), "0–0.30 (decimal fraction, i.e. 0–30%)");

        if (l.DebtToIncomeRatio.HasValue)
        {
            if (l.DebtToIncomeRatio < 0)
                yield return CheckResult.Error(CheckId, CheckName, "Loan", l.LoanId,
                    $"DebtToIncomeRatio {l.DebtToIncomeRatio} is negative.",
                    "DebtToIncomeRatio", l.DebtToIncomeRatio.ToString(), ">= 0");

            else if (l.DebtToIncomeRatio > 2.0m)
                yield return CheckResult.Warning(CheckId, CheckName, "Loan", l.LoanId,
                    $"DebtToIncomeRatio {l.DebtToIncomeRatio:P0} exceeds 200%, which is unusually high.",
                    "DebtToIncomeRatio", l.DebtToIncomeRatio.ToString(), "0–2.0 (0–200%)");
        }
    }
}
