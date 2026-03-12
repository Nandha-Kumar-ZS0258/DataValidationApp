namespace TruStage.Adaptor.Validation.Models;

/// <summary>Severity level for a row-count reconciliation gap.</summary>
public enum GapSeverity
{
    /// <summary>Informational — expected deduplication or minor discrepancy.</summary>
    Info,

    /// <summary>Advisory — records lost but not necessarily fatal to the batch.</summary>
    Warning,

    /// <summary>Hard failure — unexpected data loss that should block or alert.</summary>
    Error
}
