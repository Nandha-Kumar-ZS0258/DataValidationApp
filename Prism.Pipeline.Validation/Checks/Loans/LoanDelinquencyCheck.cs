using Prism.Pipeline.Validation.Models;
using Prism.Pipeline.Core.Models.Canonical;

namespace Prism.Pipeline.Validation.Checks.Loans;

/// <summary>
/// LN-002: DelinquencyStatus must be consistent with DaysPastDue.
///   - DaysPastDue = 0  → status must be "Current"
///   - DaysPastDue 1–29 → status must be "Current" (not yet bucketed)
///   - DaysPastDue 30–59→ status must be "Delinquent30"
///   - DaysPastDue 60–89→ status must be "Delinquent60"
///   - DaysPastDue >= 90 → status must be "Delinquent90Plus" (unless ChargedOff)
/// </summary>
public sealed class LoanDelinquencyCheck : IConsistencyCheck<CuLoan>
{
    public string CheckId   => "LN-002";
    public string CheckName => "Loan Delinquency Status Consistency";

    public IEnumerable<CheckResult> Validate(CuLoan l)
    {
        if (l.DaysPastDue is null) yield break;

        var dpd = l.DaysPastDue.Value;
        var status = l.DelinquencyStatus;

        var expectedStatus = dpd switch
        {
            0               => "Current",
            >= 1 and < 30   => "Current",
            >= 30 and < 60  => "Delinquent30",
            >= 60 and < 90  => "Delinquent60",
            >= 90           => "Delinquent90Plus",
            _               => null
        };

        // ChargedOff is valid regardless of DPD
        if (status == "ChargedOff" || status == "Paid") yield break;

        if (expectedStatus is not null &&
            !string.Equals(status, expectedStatus, StringComparison.OrdinalIgnoreCase))
            yield return CheckResult.Error(CheckId, CheckName, "Loan", l.LoanId,
                $"DelinquencyStatus '{status}' is inconsistent with DaysPastDue={dpd}. " +
                $"Expected '{expectedStatus}'.",
                "DelinquencyStatus", status, expectedStatus);
    }
}
