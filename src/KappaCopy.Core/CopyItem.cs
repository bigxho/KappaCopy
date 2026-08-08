namespace KappaCopy.Core;

public sealed record CopyItem(string SourcePath, bool IsDirectory)
{
    public string Name => Path.GetFileName(SourcePath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
}
