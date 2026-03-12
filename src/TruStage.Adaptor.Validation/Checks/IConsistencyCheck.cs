using TruStage.Adaptor.Validation.Models;

namespace TruStage.Adaptor.Validation.Checks;

/// <summary>
/// A single consistency rule that validates one entity type.
/// Returns an empty collection when the entity passes, or one or more
/// <see cref="CheckResult"/> entries when it fails.
/// </summary>
public interface IConsistencyCheck<in TEntity>
{
    /// <summary>Unique rule identifier, e.g. "MBR-001".</summary>
    string CheckId { get; }

    /// <summary>Short description of what this check validates.</summary>
    string CheckName { get; }

    IEnumerable<CheckResult> Validate(TEntity entity);
}
