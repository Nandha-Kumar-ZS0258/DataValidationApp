using TruStage.Adaptor.Core.DataValidation.Models;
using TruStage.Adaptor.Core.Models.Canonical;

namespace TruStage.Adaptor.Validation.Checks.Accounts;

/// <summary>
/// ACC-003: MaturityDate must be after OpenDate for term-based account types
/// (Certificate, IRA). For other types, MaturityDate should be absent.
/// </summary>
public sealed class AccountMaturityDateCheck : IConsistencyCheck<CuAccount>
{
    public string CheckId   => "ACC-003";
    public string CheckName => "Account Maturity Date Validity";

    private static readonly HashSet<string> TermAccountTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Certificate", "IRA"
    };

    public IEnumerable<CheckResult> Validate(CuAccount a)
    {
        if (a.MaturityDate is null) yield break;

        if (a.MaturityDate.Value <= a.OpenDate)
            yield return CheckResult.Error(CheckId, CheckName, "Account", a.AccountId,
                $"MaturityDate '{a.MaturityDate}' must be after OpenDate '{a.OpenDate}'.",
                "MaturityDate", a.MaturityDate.ToString(), $"> OpenDate ({a.OpenDate})");

        if (!TermAccountTypes.Contains(a.AccountType))
            yield return CheckResult.Warning(CheckId, CheckName, "Account", a.AccountId,
                $"MaturityDate is set on a '{a.AccountType}' account, which is not a term-based account type.",
                "MaturityDate", a.MaturityDate.ToString(), "Only expected for Certificate or IRA accounts");
    }
}
