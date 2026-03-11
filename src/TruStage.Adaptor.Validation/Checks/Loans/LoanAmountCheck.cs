using TruStage.Adaptor.Core.DataValidation.Models;
using TruStage.Adaptor.Core.Models.Canonical;

namespace TruStage.Adaptor.Validation.Checks.Loans;

/// <summary>
/// LN-001: LoanAmount must be > 0. CurrentBalance must be >= 0 and not exceed LoanAmount.
/// </summary>
public sealed class LoanAmountCheck : IConsistencyCheck<CuLoan>
{
    public string CheckId   => "LN-001";
    public string CheckName => "Loan Amount Validity";

    public IEnumerable<CheckResult> Validate(CuLoan l)
    {
        if (l.LoanAmount <= 0)
            yield return CheckResult.Error(CheckId, CheckName, "Loan", l.LoanId,
                $"LoanAmount {l.LoanAmount:F2} must be greater than zero.",
                "LoanAmount", l.LoanAmount.ToString("F2"), "> 0");

        if (l.CurrentBalance < 0)
            yield return CheckResult.Error(CheckId, CheckName, "Loan", l.LoanId,
                $"CurrentBalance {l.CurrentBalance:F2} is negative.",
                "CurrentBalance", l.CurrentBalance.ToString("F2"), ">= 0");

        if (l.LoanAmount > 0 && l.CurrentBalance > l.LoanAmount)
            yield return CheckResult.Warning(CheckId, CheckName, "Loan", l.LoanId,
                $"CurrentBalance {l.CurrentBalance:F2} exceeds original LoanAmount {l.LoanAmount:F2}. " +
                "This may indicate accrued interest or fees.",
                "CurrentBalance", l.CurrentBalance.ToString("F2"), $"<= LoanAmount ({l.LoanAmount:F2})");
    }
}
