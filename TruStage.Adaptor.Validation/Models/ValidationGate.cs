namespace TruStage.Adaptor.Validation.Models;

/// <summary>
/// The outcome of a single validation gate — count reconciliation + consistency checks
/// between two pipeline stages.
/// </summary>
public sealed class ValidationGate
{
    public string                     GateId              { get; set; } = string.Empty;
    public string                     Description         { get; set; } = string.Empty;
    public DataSnapshot               From                { get; set; } = new();
    public DataSnapshot               To                  { get; set; } = new();
    public IReadOnlyList<ReconciliationGap> CountGaps     { get; set; } = Array.Empty<ReconciliationGap>();
    public IReadOnlyList<CheckResult> ConsistencyFailures { get; set; } = Array.Empty<CheckResult>();

    /// <summary>True when no Error-severity count gaps or consistency failures exist.</summary>
    public bool Passed =>
        !CountGaps.Any(g => g.Severity == GapSeverity.Error) &&
        !ConsistencyFailures.Any(f => f.Severity == CheckSeverity.Error);

    public int ErrorCount =>
        CountGaps.Count(g => g.Severity == GapSeverity.Error) +
        ConsistencyFailures.Count(f => f.Severity == CheckSeverity.Error);

    public int WarningCount =>
        CountGaps.Count(g => g.Severity == GapSeverity.Warning) +
        ConsistencyFailures.Count(f => f.Severity == CheckSeverity.Warning);
}
