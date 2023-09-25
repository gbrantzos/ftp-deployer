using System.CommandLine;
using System.Diagnostics;
using FtpDeployer;
using Spectre.Console;

// Prepare arguments parsing
var settingsArgument = new Argument<string>("settings", "Settings file path.");
var noColorsOption = new Option<bool>("--no-colors", "Do not use colors (for proper logging).");
var cmd = new RootCommand();
cmd.AddArgument(settingsArgument);
cmd.AddOption(noColorsOption);

cmd.SetHandler((string settingsPath, bool skipColors) =>
    {
        // Get deployment config
        var settings = Settings.FromYaml(settingsPath);

        // Create the appropriate console
        var console = skipColors
            ? AnsiConsole.Create(new AnsiConsoleSettings()
            {
                ColorSystem = ColorSystemSupport.NoColors,
                Interactive = InteractionSupport.No
            })
            : AnsiConsole.Console;
        if (skipColors)
        {
            console.Profile.Width = 120;
        }

        return DoWork(console, settings);
    },
    settingsArgument,
    noColorsOption);

AnsiConsole.WriteLine("");
AnsiConsole.Write(new FigletText("FTP Deployer").LeftJustified().Color(Color.Red));
AnsiConsole.WriteLine("");

return await cmd.InvokeAsync(args);

Task<int> DoWork(IAnsiConsole console, Settings settings)
{
    try
    {
        // TODO Get this from assembly
        var version = "v0.1.0-preview";
        var sw = new Stopwatch();
        sw.Start();

        // Logging info
        console.WriteLine("");
        console.MarkupLine($"FTP deployer, version [blue]{version}[/], running at [blue]{DateTime.Now:s}[/]");
        console.MarkupLine($"Executing [blue]{settings.Title}[/] (file {settings.FilePath})");

        // Get deployment sources
        console.MarkupLine("Searching for files to deploy...");
        var sourceDetails = SourceAggregator.GetFileDetails(settings);
        var totalVolume = sourceDetails.Sum(d => d.FileSize);
        console.MarkupLine($"Found [green]{sourceDetails.Count}[/] files, total [green]{totalVolume}[/] bytes");

        // Transfer files
        var deployer = new Deployer(settings, console.MarkupLine);
        console.MarkupLine(
            $"Deploying to FTP [blue]{settings.FtpHost}:{settings.FtpPort}[/] on directory [blue]{settings.RemoteDirectory}[/]");
        console.WriteLine("");

        var totalFiles = sourceDetails.Count;
        console.Status()
            .Start("Deploying to FTP...", ctx =>
            {
                ctx.Status("Transferring files ...");
                ctx.Spinner(Spinner.Known.Dots);
                ctx.SpinnerStyle(Style.Parse("yellow"));

                var results = deployer.StartDeployment(
                    sourceDetails,
                    (details, index, skipped) =>
                    {
                        var tmp = $"({index}/{totalFiles})";
                        ctx.Status($"Transferring files ... {tmp}");
                        console.MarkupLine(skipped
                            ? $"Skipping  [olive]{details.RelativePath}[/] (same size)"
                            : $"Uploading [olive]{details.RelativePath}[/] ({details.FileSize} bytes) ... ");
                    });

                console.MarkupLine($"Transferred [green]{results.FilesTransferred}[/] files, [green]{results.BytesTransferred}[/] bytes");
            });

        console.WriteLine("");
        sw.Stop();
        console.MarkupLine($"Deployment finished, elapsed [green]{sw.ElapsedMilliseconds}[/] ms");
    }
    catch (Exception x)
    {
        console.WriteLine("");
        console.MarkupLine($"Unhandled exception: [red]{x.Message}[/]");
        console.WriteLine("");
        console.WriteLine("Exception details:");
        console.WriteException(x);

        return Task.FromResult(1);
    }

    return Task.FromResult(0);
}