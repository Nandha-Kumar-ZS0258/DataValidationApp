using Prism.Pipeline.Validation.Models;
using Prism.Pipeline.Core.Models.Canonical;

namespace Prism.Pipeline.Validation.Checks.Members;

/// <summary>
/// MBR-004: MembershipOpenDate consistency rules.
///   - Must not be in the future
///   - Must be on or after DateOfBirth (member must exist before joining)
///   - Should not predate DateOfBirth by more than 0 years (warning if member was under 16)
/// </summary>
public sealed class MemberMembershipDateCheck : IConsistencyCheck<CuMember>
{
    public string CheckId   => "MBR-004";
    public string CheckName => "Member Membership Open Date Validity";

    public IEnumerable<CheckResult> Validate(CuMember m)
    {
        if (m.MembershipOpenDate is null) yield break;

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var openDate = m.MembershipOpenDate.Value;

        if (openDate > today)
            yield return CheckResult.Error(CheckId, CheckName, "Member", m.MemberId,
                $"MembershipOpenDate '{openDate}' is in the future.",
                "MembershipOpenDate", openDate.ToString(), $"<= {today}");

        if (m.DateOfBirth != default && openDate < m.DateOfBirth)
            yield return CheckResult.Error(CheckId, CheckName, "Member", m.MemberId,
                $"MembershipOpenDate '{openDate}' is before DateOfBirth '{m.DateOfBirth}'.",
                "MembershipOpenDate", openDate.ToString(), $">= {m.DateOfBirth}");

        if (m.DateOfBirth != default && openDate >= m.DateOfBirth)
        {
            var ageAtJoining = openDate.Year - m.DateOfBirth.Year;
            if (openDate < m.DateOfBirth.AddYears(ageAtJoining)) ageAtJoining--;

            if (ageAtJoining < 16)
                yield return CheckResult.Warning(CheckId, CheckName, "Member", m.MemberId,
                    $"Member was {ageAtJoining} years old at membership open date '{openDate}'. Verify if this is a minor account.",
                    "MembershipOpenDate", openDate.ToString(), "Age >= 16 at join date");
        }
    }
}
