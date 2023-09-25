using YamlDotNet.Serialization;

namespace FtpDeployer;

public sealed class Settings
{
    public string Title { get; set; } = String.Empty;
    public string FilePath { get; set; } = String.Empty;
    public string FtpHost { get; set; } = String.Empty;
    public int FtpPort { get; set; } = 21;
    public string Username { get; set; } = String.Empty;
    public string Password { get; set; } = String.Empty;
    public string LocalDirectory { get; set; } = String.Empty;
    public string[] Include { get; set; } = { };
    public string[] Exclude { get; set; } = { };
    public string RemoteDirectory { get; set; } = String.Empty;
    public bool EmptyRemoteDirectory { get; set; }
    public bool UploadAppOffline { get; set; }

    public static Settings FromYaml(string filePath)
    {
        var path = Path.GetFullPath(filePath);
        if (!File.Exists(path))
            throw new ArgumentException($"File does not exist: {path}");

        var deserializer = new DeserializerBuilder().Build();
        var deployment = deserializer.Deserialize<Settings>(File.ReadAllText(path));
        if (String.IsNullOrEmpty(deployment.Title))
            deployment.Title = Path.GetFileNameWithoutExtension(path);
        deployment.FilePath = path;

        return deployment;
    }
}