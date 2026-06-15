#!/usr/bin/env dotnet
#:package SharpCompress@0.49.1
// Sadly until .net 11 (hopefully), we have to use sharpcompress since .net10 doesn't support LZMA2/XZ compression.
using System.Formats.Tar;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using SharpCompress.Readers;


var json = await File.ReadAllTextAsync(Path.Combine("..", "external-assembly-version.json"));
var externalItems = JsonSerializer.Deserialize<ExternalAssemblyVersions[]>(json, ExternalAssemblyVersionsJsonSourceGenerationContext.Default.ExternalAssemblyVersionsArray);
var httpClient = new HttpClient();

if (externalItems is null)
{
    throw new Exception($"Unable to deserialize {Path.Combine("..", "external-assembly-version.json")}");
}

Console.WriteLine($"Found {externalItems.Length} external items to fetch.");

foreach (var externalAssembly in externalItems)
{
    Console.WriteLine($"Fetching version {externalAssembly.Version} of {externalAssembly.Name} ({externalAssembly.Repo})");
    foreach (var externalAssemblyDownloadPath in externalAssembly.Paths)
    {
        Console.WriteLine($"Fetching {externalAssemblyDownloadPath.Url}");
        var result = await httpClient.GetAsync(externalAssemblyDownloadPath.Url);
        var bytes = await result.Content.ReadAsByteArrayAsync();
        var name = externalAssemblyDownloadPath.Url.Segments.Last();


        var outputPath = Path.Combine("..", "external", name);

        if (File.Exists(outputPath))
        {
            File.Delete(outputPath);
        }

        await File.WriteAllBytesAsync(outputPath, bytes);
        
        Console.WriteLine("Verifying checksum");
        string checksum;
        await using (var stream = File.OpenRead(outputPath))
        {
            byte[] hashBytes = SHA256.HashData(stream);
            checksum = Convert.ToHexString(hashBytes).ToLowerInvariant();
        }


        if (checksum != externalAssemblyDownloadPath.Sha256)
        {
            File.Delete(outputPath);
            Console.WriteLine($"Checksum was expected to be {externalAssemblyDownloadPath.Sha256} but was {checksum}. Did you update the version in external-assembly-version.json without updating it's Sha256?  If not, then it's possible this release has been hijacked and should not be trusted.  Exiting");
            return;
        }


        Console.WriteLine($"Extracting {externalAssemblyDownloadPath.FileToExtract} from {outputPath}");

        await ExtractSingleFile(outputPath, externalAssemblyDownloadPath.FileToExtract, Path.Combine("..", "external", externalAssemblyDownloadPath.FileToExtract));
        Thread.Sleep(1000);
        File.Delete(outputPath);
    }
}

static async Task ExtractSingleFile(string path, string targetEntryName, string destinationPath)
{
    await using Stream stream = File.OpenRead(path);
    await using var reader = await ReaderFactory.OpenAsyncReader(stream);
    while (await reader.MoveToNextEntryAsync())
    {
        if (reader.Entry.Key is null) continue;

        var keyWithoutPath = reader.Entry.Key.Split('/').Last();
        if (keyWithoutPath == targetEntryName)
        {
            await using var outputStream = File.Create(destinationPath);
            await reader.WriteEntryToAsync(outputStream);
        }
    }
}

[JsonSourceGenerationOptions(WriteIndented = true, PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(ExternalAssemblyVersions[]))]
partial class ExternalAssemblyVersionsJsonSourceGenerationContext : JsonSerializerContext;

#pragma warning disable CS8618
class ExternalAssemblyVersions {
    
    public string Name { get; set; }
    public string Repo { get; set; }
    public string Version { get; set; }

    public ExternalAssemblyDownloadPath[] Paths { get; set; }
    
    public class ExternalAssemblyDownloadPath
    {
        public Uri Url { get;set; }
        public string Sha256 { get; set; }
        public string FileToExtract { get; set; }

    }
    
}