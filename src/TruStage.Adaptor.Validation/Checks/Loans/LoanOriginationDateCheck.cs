using TruStage.Adaptor.Validation.Models;
using TruStage.Adaptor.Core.Models.Canonical;

namespace TruStage.Adaptor.Validation.Checks.Loans;

/// <summary>
/// LN-005: OriginationDate must be a realistic date.
///   - Must not be in the future
///   - Must not be before 1900-01-01
///   - PaymentDueDate (if present) must be >= OriginationDate
/// </summary>
public sealed class LoanOriginationDateCheck : IConsistencyCheck<CuLoan>
{
    public string CheckId   => "LN-005";
    public string CheckName => "Loan Origination Date Validity";

    private static readonly DateOnly MinDate = new(1900, 1, 1);

    public IEnumerable<CheckResult> Validate(CuLoan l)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        if (l.OriginationDate > today)
            yield return CheckResult.Error(CheckId, CheckName, "Loan", l.LoanId,
                $"OriginationDate '{l.OriginationDate}' is in the future.",
                "OriginationDate", l.OriginationDate.ToString(), $"<= {today}");

        if (l.OriginationDate < MinDate)
            yield return CheckResult.Error(CheckId, CheckName, "Loan", l.LoanId,
                $"OriginationDate '{l.OriginationDate}' is before the minimum allowed date {MinDate}.",
                "OriginationDate", l.OriginationDate.ToString(), $">= {MinDate}");

        if (l.PaymentDueDate.HasValue && l.PaymentDueDate.Value < l.OriginationDate)
            yield return CheckResult.Error(CheckId, CheckName, "Loan", l.LoanId,
                $"PaymentDueDate '{l.PaymentDueDate}' is before OriginationDate '{l.OriginationDate}'.",
                "PaymentDueDate", l.PaymentDueDate.ToString(), $">= OriginationDate ({l.OriginationDate})");
    }
}
