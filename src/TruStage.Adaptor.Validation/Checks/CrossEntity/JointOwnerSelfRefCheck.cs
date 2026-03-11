using TruStage.Adaptor.Core.DataValidation.Models;
using TruStage.Adaptor.Core.Models.Canonical;

namespace TruStage.Adaptor.Validation.Checks.CrossEntity;

/// <summary>
/// REF-003: A JointOwner record must not reference the same member as both
/// primary and joint owner. Also verifies both members exist in the batch.
/// </summary>
public sealed class JointOwnerSelfRefCheck
{
    public string CheckId   => "REF-003";
    public string CheckName => "Joint Owner Self-Reference and Existence";

    public IEnumerable<CheckResult> Validate(
        IReadOnlyList<CuMember>    members,
        IReadOnlyList<CuJointOwner> jointOwners)
    {
        var memberIds = members.Select(m => m.MemberId).ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var jo in jointOwners)
        {
            var key = $"{jo.PrimaryMemberId}|{jo.JointMemberId}|{jo.AccountId}";

            if (string.Equals(jo.PrimaryMemberId, jo.JointMemberId, StringComparison.OrdinalIgnoreCase))
                yield return CheckResult.Error(CheckId, CheckName, "JointOwner", key,
                    $"PrimaryMemberId and JointMemberId are both '{jo.PrimaryMemberId}'. A member cannot be their own joint owner.",
                    "JointMemberId", jo.JointMemberId, "Must differ from PrimaryMemberId");

            if (!memberIds.Contains(jo.PrimaryMemberId))
                yield return CheckResult.Error(CheckId, CheckName, "JointOwner", key,
                    $"PrimaryMemberId '{jo.PrimaryMemberId}' does not exist in the mapped Members.",
                    "PrimaryMemberId", jo.PrimaryMemberId, "Must exist in Members");

            if (!memberIds.Contains(jo.JointMemberId))
                yield return CheckResult.Warning(CheckId, CheckName, "JointOwner", key,
                    $"JointMemberId '{jo.JointMemberId}' does not exist in the mapped Members. " +
                    "This may be a member from a different batch.",
                    "JointMemberId", jo.JointMemberId, "Should exist in Members");
        }
    }
}
