namespace KappaCopy.Core;

public sealed record CopyProgress(
    int OverallPercent,
    int CurrentItemPercent,
    int CompletedItems,
    int TotalItems,
    string? CurrentPath,
    string? Message);
