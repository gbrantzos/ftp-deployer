using System.Reflection;

namespace FtpDeployer;

public static class AppOffline
{
    public static string FileName { get; set; } = "App_Offline.htm";
    public static string Default => GetDefault();

    private static readonly Assembly Assembly = typeof(AppOffline).Assembly;
    
    private static string GetDefault()
    {
        var resourceName = $"FtpDeployer.{FileName}";
        using var stream = Assembly.GetManifestResourceStream(resourceName) ??
                           throw new InvalidOperationException($"Could not read resource {resourceName}");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd().Trim();
    }
}