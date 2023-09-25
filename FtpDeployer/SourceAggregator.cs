using GlobExpressions;

namespace FtpDeployer;

public static class SourceAggregator
{
    public record FileDetails(
        string FullPath, 
        string RelativePath,
        string RelativeFolder,
        string RelativeFileName,
        long FileSize);

    public static IReadOnlyList<FileDetails> GetFileDetails(Settings deployment)
    {
        // Get all files inside root path
        var files = Directory.EnumerateFiles(deployment.LocalDirectory, "*.*", SearchOption.AllDirectories);

        // Inclusions - Exclusions
        var include = deployment.Include.Select(m => new Glob(m)).ToArray();
        var exclude = deployment.Exclude.Select(m => new Glob(m)).ToArray();

        // Get relative paths for all files
        var details = files
            .Where(f => include.Length == 0 || include.Any(x => x.IsMatch(f)))
            .Where(f => exclude.Length == 0 || exclude.All(x => !x.IsMatch(f)))
            .Select(fullPath =>
            {
                var relativePath = Path.GetRelativePath(deployment.LocalDirectory, fullPath);
                return new FileDetails(
                    fullPath,
                    relativePath,
                    Path.GetDirectoryName(relativePath) ?? "",
                    Path.GetFileName(relativePath),
                    new FileInfo(fullPath).Length
                );
            })
            .ToList();

        return details;
    }
}