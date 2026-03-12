using TruStage.Adaptor.Validation.Models;
using TruStage.Adaptor.Core.Models.Canonical;

namespace TruStage.Adaptor.Validation.Checks.Loans;

/// <summary>
/// LN-003: ChargeOffDate must only be present when DelinquencyStatus is "ChargedOff".
/// Conversely, if status is "ChargedOff", a ChargeOffDate is expected.
/// </summary>
public sealed class LoanChargeOffCheck : IConsistencyCheck<CuLoan>
{
    public string CheckId   => "LN-003";
    public string CheckName => "Loan Charge-Off Date Consistency";

    public IEnumerable<CheckResult> Validate(CuLoan l)
    {
        var isChargedOff = string.Equals(l.DelinquencyStatus, "ChargedOff",
            StringComparison.OrdinalIgnoreCase);

        if (l.ChargeOffDate.HasValue && !isChargedOff)
            yield return CheckResult.Error(CheckId, CheckName, "Loan", l.LoanId,
                $"ChargeOffDate '{l.ChargeOffDate}' is set but DelinquencyStatus is '{l.DelinquencyStatus}', not 'ChargedOff'.",
                "ChargeOffDate", l.ChargeOffDate.ToString(), "Only present when DelinquencyStatus = ChargedOff");

        if (isChargedOff && !l.ChargeOffDate.HasValue)
            yield return CheckResult.Warning(CheckId, CheckName, "Loan", l.LoanId,
                "DelinquencyStatus is 'ChargedOff' but ChargeOffDate is missing.",
                "ChargeOffDate", null, "Expected date when status = ChargedOff");

        if (l.ChargeOffDate.HasValue && l.OriginationDate != default &&
            l.ChargeOffDate.Value < l.OriginationDate)
            yield return CheckResult.Error(CheckId, CheckName, "Loan", l.LoanId,
                $"ChargeOffDate '{l.ChargeOffDate}' is before OriginationDate '{l.OriginationDate}'.",
                "ChargeOffDate", l.ChargeOffDate.ToString(), $">= OriginationDate ({l.OriginationDate})");
    }
}
