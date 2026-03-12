namespace TruStage.Adaptor.Validation.Models;

/// <summary>
/// Row counts captured at a specific pipeline stage.
/// </summary>
public sealed record DataSnapshot
{
    public static class Stages
    {
        public const string Source       = "Source";
        public const string Transformed  = "Transformed";
        public const string ReadyForProd = "ReadyForProd";
        public const string Prod         = "Prod";
    }

    public string Stage            { get; init; } = string.Empty;
    public int    MemberCount      { get; init; }
    public int    AccountCount     { get; init; }
    public int    LoanCount        { get; init; }
    public int    TransactionCount { get; init; }
    public int    JointOwnerCount  { get; init; }
    public int    MappingErrors    { get; init; }
    public int    DqBlocked        { get; init; }
    public int    DqWarnings       { get; init; }
    public int?   DeclaredMemberCount { get; init; }
}
