using Prism.Pipeline.Validation.Models;
using Prism.Pipeline.Core.Models.Canonical;

namespace Prism.Pipeline.Validation.Checks.Members;

/// <summary>
/// MBR-001: Required fields must not be null, empty, or the "Unknown" fallback
/// that the mapper inserts when name parsing fails.
/// </summary>
public sealed class MemberRequiredFieldsCheck : IConsistencyCheck<CuMember>
{
    public string CheckId   => "MBR-001";
    public string CheckName => "Member Required Fields";

    public IEnumerable<CheckResult> Validate(CuMember m)
    {
        if (string.IsNullOrWhiteSpace(m.MemberId))
            yield return CheckResult.Error(CheckId, CheckName, "Member", m.MemberId ?? "(null)",
                "MemberId is null or empty.", "MemberId");

        if (string.IsNullOrWhiteSpace(m.FirstName) || m.FirstName == "Unknown")
            yield return CheckResult.Error(CheckId, CheckName, "Member", m.MemberId ?? "(null)",
                $"FirstName is missing or could not be parsed (value: '{m.FirstName}').",
                "FirstName", m.FirstName);

        if (string.IsNullOrWhiteSpace(m.LastName) || m.LastName == "Unknown")
            yield return CheckResult.Error(CheckId, CheckName, "Member", m.MemberId ?? "(null)",
                $"LastName is missing or could not be parsed (value: '{m.LastName}').",
                "LastName", m.LastName);

        if (m.DateOfBirth == DateOnly.MinValue || m.DateOfBirth == default)
            yield return CheckResult.Error(CheckId, CheckName, "Member", m.MemberId ?? "(null)",
                $"DateOfBirth is missing or defaulted to MinValue (value: '{m.DateOfBirth}').",
                "DateOfBirth", m.DateOfBirth.ToString());
    }
}
