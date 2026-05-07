namespace Prism.Pipeline.Validation.Models;

/// <summary>
/// A single finding produced by a consistency check on one entity.
/// </summary>
public sealed class CheckResult
{
    public required string        CheckId       { get; init; }
    public required string        CheckName     { get; init; }
    public required string        EntityType    { get; init; }
    public required string        EntityKey     { get; init; }
    public required string        Message       { get; init; }
    public          CheckSeverity Severity      { get; init; }
    public          string?       FieldName     { get; init; }
    public          string?       ActualValue   { get; init; }
    public          string?       ExpectedRange { get; init; }

    public static CheckResult Error(
        string checkId, string checkName,
        string entityType, string entityKey,
        string message,
        string? fieldName = null, string? actualValue = null, string? expectedRange = null)
        => new()
        {
            CheckId       = checkId,
            CheckName     = checkName,
            EntityType    = entityType,
            EntityKey     = entityKey,
            Message       = message,
            Severity      = CheckSeverity.Error,
            FieldName     = fieldName,
            ActualValue   = actualValue,
            ExpectedRange = expectedRange
        };

    public static CheckResult Warning(
        string checkId, string checkName,
        string entityType, string entityKey,
        string message,
        string? fieldName = null, string? actualValue = null, string? expectedRange = null)
        => new()
        {
            CheckId       = checkId,
            CheckName     = checkName,
            EntityType    = entityType,
            EntityKey     = entityKey,
            Message       = message,
            Severity      = CheckSeverity.Warning,
            FieldName     = fieldName,
            ActualValue   = actualValue,
            ExpectedRange = expectedRange
        };
}
