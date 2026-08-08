namespace KappaCopy.Core;

public sealed record CopyResult(
    bool Success,
    bool Cancelled,
    int ExitCode,
    int CompletedItems,
    int TotalItems,
    IReadOnlyList<string> Errors);
