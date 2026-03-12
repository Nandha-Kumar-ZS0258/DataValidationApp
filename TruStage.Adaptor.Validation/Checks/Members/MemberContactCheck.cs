using System.Text.RegularExpressions;
using TruStage.Adaptor.Validation.Models;
using TruStage.Adaptor.Core.Models.Canonical;

namespace TruStage.Adaptor.Validation.Checks.Members;

/// <summary>
/// MBR-006: Contact field format checks (warnings only — contact data is optional).
///   - Email: basic RFC-5322 pattern
///   - State: valid 2-letter US state/territory code
/// </summary>
public sealed class MemberContactCheck : IConsistencyCheck<CuMember>
{
    public string CheckId   => "MBR-006";
    public string CheckName => "Member Contact Format";

    private static readonly Regex EmailRegex =
        new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly HashSet<string> ValidStates = new(StringComparer.OrdinalIgnoreCase)
    {
        "AL","AK","AZ","AR","CA","CO","CT","DE","FL","GA",
        "HI","ID","IL","IN","IA","KS","KY","LA","ME","MD",
        "MA","MI","MN","MS","MO","MT","NE","NV","NH","NJ",
        "NM","NY","NC","ND","OH","OK","OR","PA","RI","SC",
        "SD","TN","TX","UT","VT","VA","WA","WV","WI","WY",
        "DC","PR","VI","GU","AS","MP"
    };

    public IEnumerable<CheckResult> Validate(CuMember m)
    {
        if (!string.IsNullOrWhiteSpace(m.Email) && !EmailRegex.IsMatch(m.Email))
            yield return CheckResult.Warning(CheckId, CheckName, "Member", m.MemberId,
                $"Email '{m.Email}' does not match a valid email format.",
                "Email", m.Email, "valid email address");

        if (!string.IsNullOrWhiteSpace(m.State) && !ValidStates.Contains(m.State))
            yield return CheckResult.Warning(CheckId, CheckName, "Member", m.MemberId,
                $"State '{m.State}' is not a recognised US state/territory code.",
                "State", m.State, "2-letter US state code");
    }
}
