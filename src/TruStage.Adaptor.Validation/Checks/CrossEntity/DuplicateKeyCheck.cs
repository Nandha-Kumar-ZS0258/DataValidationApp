using TruStage.Adaptor.Validation.Models;
using TruStage.Adaptor.Core.Models.Canonical;

namespace TruStage.Adaptor.Validation.Checks.CrossEntity;

/// <summary>
/// REF-004: Detects duplicate primary keys within the same batch.
/// Duplicate MemberIds or AccountIds in a single file indicate a source data issue.
/// </summary>
public sealed class DuplicateKeyCheck
{
    public string CheckId   => "REF-004";
    public string CheckName => "Duplicate Key Detection";

    public IEnumerable<CheckResult> Validate(
        IReadOnlyList<CuMember>  members,
        IReadOnlyList<CuAccount> accounts,
        IReadOnlyList<CuLoan>    loans)
    {
        // Duplicate MemberIds
        foreach (var dup in FindDuplicates(members.Select(m => m.MemberId)))
            yield return CheckResult.Error(CheckId, CheckName, "Member", dup,
                $"MemberId '{dup}' appears more than once in this batch.",
                "MemberId", dup, "Unique per CU per batch");

        // Duplicate AccountIds
        foreach (var dup in FindDuplicates(accounts.Select(a => a.AccountId)))
            yield return CheckResult.Error(CheckId, CheckName, "Account", dup,
                $"AccountId '{dup}' appears more than once in this batch.",
                "AccountId", dup, "Unique per CU per batch");

        // Duplicate LoanIds
        foreach (var dup in FindDuplicates(loans.Select(l => l.LoanId)))
            yield return CheckResult.Error(CheckId, CheckName, "Loan", dup,
                $"LoanId '{dup}' appears more than once in this batch.",
                "LoanId", dup, "Unique per CU per batch");
    }

    private static IEnumerable<string> FindDuplicates(IEnumerable<string> keys)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        return keys.Where(k => !seen.Add(k)).Distinct(StringComparer.OrdinalIgnoreCase);
    }
}
