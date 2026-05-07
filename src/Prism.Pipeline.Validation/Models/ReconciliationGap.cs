namespace Prism.Pipeline.Validation.Models;

/// <summary>
/// A count discrepancy found between two pipeline stages.
/// </summary>
public sealed class ReconciliationGap
{
    public string      EntityType { get; set; } = string.Empty;
    public int         Expected   { get; set; }
    public int         Actual     { get; set; }
    public string      Message    { get; set; } = string.Empty;
    public GapSeverity Severity   { get; set; }
}
