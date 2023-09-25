using FluentFTP;

namespace FtpDeployer;

public class Deployer
{
    private readonly Settings _settings;
    private readonly Action<string>? _logger;

    public record DeploymentResults(int FilesTransferred, long BytesTransferred);

    public Deployer(Settings settings, Action<string>? logger = null)
    {
        _settings = settings;
        _logger = logger;
    }

    public DeploymentResults StartDeployment(IEnumerable<SourceAggregator.FileDetails> sources,
        Action<SourceAggregator.FileDetails, int, bool>? callbackBefore = null,
        Action<SourceAggregator.FileDetails, int, FtpStatus>? callbackAfter = null)
    {
        // Connect to FTP target
        var client = new FtpClient(
            _settings.FtpHost,
            _settings.Username,
            _settings.Password,
            _settings.FtpPort);
        client.AutoConnect();
        client.SetWorkingDirectory(_settings.RemoteDirectory);

        // Handle actions before
        if (_settings.UploadAppOffline)
        {
            if (File.Exists(AppOffline.FileName))
                File.Delete(AppOffline.FileName);
            File.WriteAllText(AppOffline.FileName, AppOffline.Default);
            _logger?.Invoke("Uploading [olive]App_Offline.html[/] ...");
            var result = client.UploadFile(AppOffline.FileName, AppOffline.FileName);
            if (result == FtpStatus.Failed)
                throw new InvalidOperationException($"Could not upload {AppOffline.FileName}!");
            Thread.Sleep(300);
        }

        if (_settings.EmptyRemoteDirectory)
        {
            _logger?.Invoke("Empty remote directory...");
            client.EmptyDirectory(_settings.RemoteDirectory);
        }

        // Copy all files
        var index = 0;
        var files = 0;
        var bytes = 0L;
        foreach (var detail in sources)
        {
            index++;
            if (!String.IsNullOrEmpty(detail.RelativeFolder))
            {
                client.CreateDirectory(detail.RelativeFolder);
            }

            var comparison = client
                .CompareFile(detail.FullPath, detail.RelativePath, FtpCompareOption.Size);
            if (comparison == FtpCompareResult.Equal)
            {
                callbackBefore?.Invoke(detail, index, true);
                continue;
            }

            callbackBefore?.Invoke(detail, index, false);
            var uploadStatus = client.UploadFile(detail.FullPath, detail.RelativePath);
            callbackAfter?.Invoke(detail, index, uploadStatus);

            files++;
            bytes += detail.FileSize;
        }

        if (_settings.UploadAppOffline)
        {
            client.DeleteFile(AppOffline.FileName);
            if (File.Exists(AppOffline.FileName))
                File.Delete(AppOffline.FileName);
        }

        client.Disconnect();
        return new DeploymentResults(files, bytes);
    }
}