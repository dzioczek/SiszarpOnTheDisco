using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using SiszarpOnTheDisco.Signal.Models.Github;
using System.Formats.Tar;
using System.Linq;

namespace SiszarpOnTheDisco.Signal;

public class SignalUpdateService
{
    public string ClientPath { get; set; }
    private string? SignalDirectory { get; set; }
    private readonly ILogger _logger;

    public SignalUpdateService(ILogger logger)
    {
        _logger = logger;
        SignalDirectory = Environment.GetEnvironmentVariable("SIGNAL_DIRECTORY");
        _logger.Information("App path: {SignalDirectory}", SignalDirectory);
    }

    public async Task CheckUpdates(CancellationToken cancellationToken)
    {

        HttpRequestMessage request = new()
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri("https://api.github.com/repos/AsamK/signal-cli/releases/latest")
        };

        request.Headers.Accept.Clear();
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        request.Headers.Add("X-GitHub-Api-Version", "2022-11-28");
        request.Headers.UserAgent.TryParseAdd("HttpClientFactory-Sample");

        using HttpClient client = new();
        using HttpResponseMessage response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        Release? release = await response.Content.ReadFromJsonAsync<Release>(cancellationToken);
        _logger.Information("Got latest release number: {Release}", release!.Name);

        DirectoryInfo signalDir = new(SignalDirectory!);
        if (!Directory.Exists(signalDir.FullName)) Directory.CreateDirectory(signalDir.FullName);

        ClientPath = Path.Combine(signalDir.FullName, "signal-cli");

        string versionFilePath = Path.Combine(signalDir.FullName, "version");
        await using FileStream versionFile =  File.Open(versionFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite,FileShare.Read);

        _logger.Information("Reading version file");

        StreamReader reader = new(versionFile);
        string localVersion = await reader.ReadToEndAsync(cancellationToken);

        _logger.Information("Local version: {LocalVersion}, remote version {RemoteVersion}", localVersion, release!.Name);

        if (localVersion.Length > 0 && localVersion == release.Name) return;

        _logger.Information("Deleting old signal-cli Version");
        File.Delete(ClientPath);

        _logger.Information("Downloading latest release");
        Asset? asset = release.Assets.FirstOrDefault(asset =>
            asset.Name.Contains("Linux-native") && !asset.Name.Contains(".tar.gz.asc"));

        string signalLink = asset!.BrowserDownloadUrl;

        await using Stream responseStream = await client.GetStreamAsync(signalLink, cancellationToken);

        await using GZipStream decompressor = new(responseStream, CompressionMode.Decompress, false);

        string tarPath = Path.Combine(signalDir.FullName, $"signal{release.Name}.tar");
        await using Stream tarFile = File.Create(tarPath);
        await decompressor.CopyToAsync(tarFile, cancellationToken);
        tarFile.Close();

        await ExtractTar(signalDir.FullName, tarPath);

        File.Delete(tarPath);

        versionFile.SetLength(0);
        await using StreamWriter streamWriter = new(versionFile);
        await streamWriter.WriteAsync(new StringBuilder(release.Name), cancellationToken);
    }

    private async Task ExtractTar(string outputDirectory, string tarPath)
    {
        await using FileStream tarStream = new(tarPath,
            new FileStreamOptions
                { Mode = FileMode.Open, Access = FileAccess.Read, Options = FileOptions.Asynchronous });
        await using TarReader tarReader = new(tarStream);
        TarEntry? entry;
        while ((entry = await tarReader.GetNextEntryAsync()) != null)
        {
            if (entry.EntryType is TarEntryType.SymbolicLink or TarEntryType.HardLink
                or TarEntryType.GlobalExtendedAttributes) continue;

            _logger.Information("Extracting {EntryName}", entry.Name);
            await entry.ExtractToFileAsync(Path.Combine(outputDirectory, entry.Name), true);
        }
    }
}