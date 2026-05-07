using Prism.Pipeline.Validation.Models;
using Prism.Pipeline.Core.Models.Canonical;

namespace Prism.Pipeline.Validation.Checks.Members;

/// <summary>
/// MBR-005: MemberStatus must be one of the known canonical values.
/// </summary>
public sealed class MemberStatusCheck : IConsistencyCheck<CuMember>
{
    public string CheckId   => "MBR-005";
    public string CheckName => "Member Status Valid Value";

    private static readonly HashSet<string> ValidStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        MemberStatus.Active,
        MemberStatus.Inactive,
        MemberStatus.Deceased,
        MemberStatus.Closed,
        MemberStatus.Suspended
    };

    public IEnumerable<CheckResult> Validate(CuMember m)
    {
        if (!ValidStatuses.Contains(m.MemberStatus))
            yield return CheckResult.Error(CheckId, CheckName, "Member", m.MemberId,
                $"MemberStatus '{m.MemberStatus}' is not a recognised value.",
                "MemberStatus", m.MemberStatus,
                string.Join(" | ", ValidStatuses));
    }
}
