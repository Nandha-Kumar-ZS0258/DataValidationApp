using TruStage.Adaptor.Core.DataValidation.Models;
using TruStage.Adaptor.Core.Models.Canonical;

namespace TruStage.Adaptor.Validation.Checks.Members;

/// <summary>
/// MBR-003: DateOfBirth must be a realistic date.
///   - Not in the future
///   - Not before 1900-01-01
///   - Implies age between 0 and 120 years
/// </summary>
public sealed class MemberDateOfBirthCheck : IConsistencyCheck<CuMember>
{
    public string CheckId   => "MBR-003";
    public string CheckName => "Member Date of Birth Validity";

    private static readonly DateOnly MinDob = new(1900, 1, 1);

    public IEnumerable<CheckResult> Validate(CuMember m)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        if (m.DateOfBirth > today)
            yield return CheckResult.Error(CheckId, CheckName, "Member", m.MemberId,
                $"DateOfBirth '{m.DateOfBirth}' is in the future.",
                "DateOfBirth", m.DateOfBirth.ToString(), $"<= {today}");

        else if (m.DateOfBirth < MinDob)
            yield return CheckResult.Error(CheckId, CheckName, "Member", m.MemberId,
                $"DateOfBirth '{m.DateOfBirth}' is before the minimum allowed date {MinDob}.",
                "DateOfBirth", m.DateOfBirth.ToString(), $">= {MinDob}");

        else
        {
            var age = today.Year - m.DateOfBirth.Year;
            if (m.DateOfBirth > today.AddYears(-age)) age--;

            if (age > 120)
                yield return CheckResult.Warning(CheckId, CheckName, "Member", m.MemberId,
                    $"DateOfBirth '{m.DateOfBirth}' implies an age of {age} years, which is unusually high.",
                    "DateOfBirth", m.DateOfBirth.ToString(), "Age 0–120");
        }
    }
}
