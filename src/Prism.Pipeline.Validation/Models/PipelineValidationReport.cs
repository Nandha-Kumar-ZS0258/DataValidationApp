using Prism.Pipeline.Validation.Models;

namespace Prism.Pipeline.Validation.Models;

/// <summary>
/// The complete end-to-end pipeline validation report for one ingestion run.
/// Contains all three gates:
///   Gate 1 — Source → Transformed  (mapping preserved all rows?)
///   Gate 2 — Source → ReadyForProd (all rows passed consistency checks?)
///   Gate 3 — Source → Prod         (all rows actually landed in DB?)
/// </summary>
public sealed class PipelineValidationReport
{
    // ── Run identity ──────────────────────────────────────────────────────────
    public required string IngestionBatchId { get; init; }
    public required string CuId { get; init; }
    public required string SourceFileName { get; init; }
    public DateTime ReportedAt { get; init; } = DateTime.UtcNow;

    // ── Snapshots ─────────────────────────────────────────────────────────────
    public required DataSnapshot SourceSnapshot { get; init; }
    public required DataSnapshot TransformedSnapshot { get; init; }
    public required DataSnapshot ReadyForProdSnapshot { get; init; }
    public required DataSnapshot ProdSnapshot { get; init; }

    // ── Gates ─────────────────────────────────────────────────────────────────
    /// <summary>Gate 1: Source → Transformed. Did mapping preserve all rows?</summary>
    public required ValidationGate Gate1_SourceToTransformed { get; init; }

    /// <summary>Gate 2: Source → ReadyForProd. Did all rows pass consistency checks?</summary>
    public required ValidationGate Gate2_SourceToReadyForProd { get; init; }

    /// <summary>Gate 3: Source → Prod. Did all rows actually land in the DB?</summary>
    public required ValidationGate Gate3_SourceToProd { get; init; }

    // ── Overall verdict ───────────────────────────────────────────────────────
    public bool AllGatesPassed =>
        Gate1_SourceToTransformed.Passed &&
        Gate2_SourceToReadyForProd.Passed &&
        Gate3_SourceToProd.Passed;

    public PipelineValidationStatus OverallStatus
    {
        get
        {
            if (AllGatesPassed) return PipelineValidationStatus.Passed;
            if (Gate1_SourceToTransformed.Passed && Gate2_SourceToReadyForProd.Passed)
                return PipelineValidationStatus.PartiallyPassed; // only Gate 3 failed
            return PipelineValidationStatus.Failed;
        }
    }

    public int TotalErrors =>
        Gate1_SourceToTransformed.ErrorCount +
        Gate2_SourceToReadyForProd.ErrorCount +
        Gate3_SourceToProd.ErrorCount;

    public int TotalWarnings =>
        Gate1_SourceToTransformed.WarningCount +
        Gate2_SourceToReadyForProd.WarningCount +
        Gate3_SourceToProd.WarningCount;
}

public enum PipelineValidationStatus { Passed, PartiallyPassed, Failed }
