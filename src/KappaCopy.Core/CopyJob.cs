namespace KappaCopy.Core;

public sealed class CopyJob
{
    public required IReadOnlyList<CopyItem> Items { get; init; }
    public required string DestinationPath { get; init; }
    public CopyProfile Profile { get; init; } = CopyProfile.Automatic;
}
