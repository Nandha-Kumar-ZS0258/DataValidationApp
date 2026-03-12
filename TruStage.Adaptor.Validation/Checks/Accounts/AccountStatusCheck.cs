using TruStage.Adaptor.Validation.Models;
using TruStage.Adaptor.Core.Models.Canonical;

namespace TruStage.Adaptor.Validation.Checks.Accounts;

/// <summary>
/// ACC-004: AccountStatus and AccountType must be known canonical values.
/// Also verifies OpenDate is not DateOnly.MinValue (mapper parse-failure fallback).
/// </summary>
public sealed class AccountStatusCheck : IConsistencyCheck<CuAccount>
{
    public string CheckId   => "ACC-004";
    public string CheckName => "Account Status and Type Valid Values";

    private static readonly HashSet<string> ValidStatuses = new(StringComparer.OrdinalIgnoreCase)
        { "Active", "Dormant", "Closed", "ChargedOff", "Frozen" };

    private static readonly HashSet<string> ValidTypes = new(StringComparer.OrdinalIgnoreCase)
        { "Share", "Draft/Checking", "MoneyMarket", "Certificate", "IRA", "HSA", "Other" };

    public IEnumerable<CheckResult> Validate(CuAccount a)
    {
        if (!ValidStatuses.Contains(a.AccountStatus))
            yield return CheckResult.Error(CheckId, CheckName, "Account", a.AccountId,
                $"AccountStatus '{a.AccountStatus}' is not a recognised value.",
                "AccountStatus", a.AccountStatus,
                string.Join(" | ", ValidStatuses));

        if (!ValidTypes.Contains(a.AccountType))
            yield return CheckResult.Warning(CheckId, CheckName, "Account", a.AccountId,
                $"AccountType '{a.AccountType}' is not a recognised value.",
                "AccountType", a.AccountType,
                string.Join(" | ", ValidTypes));

        if (a.OpenDate == DateOnly.MinValue)
            yield return CheckResult.Error(CheckId, CheckName, "Account", a.AccountId,
                "OpenDate is DateOnly.MinValue — this indicates a date parse failure in the mapper.",
                "OpenDate", a.OpenDate.ToString(), "Valid date > 1900-01-01");
    }
}
