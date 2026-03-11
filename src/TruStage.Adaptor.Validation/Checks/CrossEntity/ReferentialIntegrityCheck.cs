using TruStage.Adaptor.Core.DataValidation.Models;
using TruStage.Adaptor.Core.Models.Canonical;

namespace TruStage.Adaptor.Validation.Checks.CrossEntity;

/// <summary>
/// REF-001: Every Account and Loan must reference a MemberId that exists in
/// the same batch's Members collection.
/// Catches orphan records before they cause FK violations in the DB.
/// </summary>
public sealed class ReferentialIntegrityCheck
{
    public string CheckId   => "REF-001";
    public string CheckName => "Referential Integrity — Account/Loan → Member";

    public IEnumerable<CheckResult> Validate(
        IReadOnlyList<CuMember>  members,
        IReadOnlyList<CuAccount> accounts,
        IReadOnlyList<CuLoan>    loans)
    {
        var memberIds = members.Select(m => m.MemberId).ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var a in accounts)
        {
            if (!memberIds.Contains(a.MemberId))
                yield return CheckResult.Error(CheckId, CheckName, "Account", a.AccountId,
                    $"Account references MemberId '{a.MemberId}' which does not exist in the mapped Members.",
                    "MemberId", a.MemberId, "Must exist in Members");
        }

        foreach (var l in loans)
        {
            if (!memberIds.Contains(l.MemberId))
                yield return CheckResult.Error(CheckId, CheckName, "Loan", l.LoanId,
                    $"Loan references MemberId '{l.MemberId}' which does not exist in the mapped Members.",
                    "MemberId", l.MemberId, "Must exist in Members");
        }
    }
}
