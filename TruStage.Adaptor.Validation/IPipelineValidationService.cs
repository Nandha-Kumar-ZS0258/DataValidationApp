using TruStage.Adaptor.Validation.Models;
using TruStage.Adaptor.Core.Models;
using TruStage.Adaptor.Validation.Models;

namespace TruStage.Adaptor.Validation;

/// <summary>
/// Orchestrates all three pipeline validation gates for one ingestion run.
///
///   Gate 1 — Source → Transformed  : row-count reconciliation + field consistency on mapped data
///   Gate 2 — Source → ReadyForProd : deeper business-rule checks; determines which records are clean
///   Gate 3 — Source → Prod         : post-save DB query; verifies actual persisted counts match source
///
/// Gate 1 and Gate 2 are run inside the adapter (before SaveAsync).
/// Gate 3 is run in the Runner after SaveAsync, using the DB snapshot returned by the repository.
/// </summary>
public interface IPipelineValidationService
{
    /// <summary>
    /// Runs Gate 1 and Gate 2 checks on the in-memory canonical entities.
    /// Returns the validated result (with any DQ-blocked records removed from clean lists)
    /// plus the two gate snapshots.
    /// </summary>
    Gate1And2Result RunGate1And2(
        DataSnapshot            sourceSnapshot,
        CanonicalAdapterResult  transformedResult);

    /// <summary>
    /// Runs Gate 3 — compares the source snapshot to the DB snapshot captured
    /// after SaveAsync, and assembles the full <see cref="PipelineValidationReport"/>.
    /// </summary>
    PipelineValidationReport RunGate3AndBuildReport(
        DataSnapshot       sourceSnapshot,
        DataSnapshot       transformedSnapshot,
        DataSnapshot       readyForProdSnapshot,
        DataSnapshot       prodSnapshot,
        ValidationGate     gate1,
        ValidationGate     gate2,
        string             ingestionBatchId,
        string             cuId,
        string             sourceFileName);
}

/// <summary>
/// Carries the outputs of Gate 1 and Gate 2 back to the adapter/runner.
/// </summary>
public sealed class Gate1And2Result
{
    public required DataSnapshot    TransformedSnapshot  { get; init; }
    public required DataSnapshot    ReadyForProdSnapshot { get; init; }
    public required ValidationGate  Gate1                { get; init; }
    public required ValidationGate  Gate2                { get; init; }

    /// <summary>
    /// The subset of mapped entities that passed all Gate 2 consistency checks.
    /// These are the records that will actually be written to the DB.
    /// </summary>
    public required CanonicalAdapterResult CleanResult { get; init; }
}
