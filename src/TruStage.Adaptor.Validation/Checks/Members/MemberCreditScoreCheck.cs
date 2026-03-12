using TruStage.Adaptor.Validation.Models;
using TruStage.Adaptor.Core.Models.Canonical;

namespace TruStage.Adaptor.Validation.Checks.Members;

/// <summary>
/// MBR-002: CreditScore must be in the 300–850 range (FICO standard).
/// DB has a CHECK constraint for this but we catch it earlier with a clear message.
/// Null is allowed — not all members have a score on file.
/// </summary>
public sealed class MemberCreditScoreCheck : IConsistencyCheck<CuMember>
{
    public string CheckId   => "MBR-002";
    public string CheckName => "Member Credit Score Range";

    public IEnumerable<CheckResult> Validate(CuMember m)
    {
        if (m.CreditScore is null) yield break;

        if (m.CreditScore < 300 || m.CreditScore > 850)
            yield return CheckResult.Error(CheckId, CheckName, "Member", m.MemberId,
                $"CreditScore {m.CreditScore} is outside the valid FICO range 300–850.",
                "CreditScore", m.CreditScore.ToString(), "300–850");
    }
}
