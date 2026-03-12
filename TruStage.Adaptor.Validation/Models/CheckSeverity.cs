namespace TruStage.Adaptor.Validation.Models;

/// <summary>Severity level for a single consistency-check finding.</summary>
public enum CheckSeverity
{
    /// <summary>Advisory finding — record passes through but is flagged.</summary>
    Warning,

    /// <summary>Hard failure — record is blocked from the clean result set.</summary>
    Error
}
