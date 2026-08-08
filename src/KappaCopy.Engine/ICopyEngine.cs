using KappaCopy.Core;

namespace KappaCopy.Engine;

public interface ICopyEngine
{
    bool IsSupported { get; }
    string DisplayName { get; }

    Task<CopyResult> CopyAsync(
        CopyJob job,
        IProgress<CopyProgress>? progress = null,
        Action<string>? log = null,
        CancellationToken cancellationToken = default);
}
