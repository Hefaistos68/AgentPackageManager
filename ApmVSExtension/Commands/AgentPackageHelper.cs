using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Versioning;

namespace ApmVSExtension
{
    /// <summary>
    /// Shared logic for creating an agent NuGet package from a <c>.github</c> folder.
    /// </summary>
    internal static class AgentPackageHelper
    {
        private const string ContentRoot = "agentcontent/github";
        private const string ConfigFileName = "agent-package.json";

        /// <summary>
        /// Runs the full packaging workflow for the given base directory.
        /// </summary>
        /// <param name="baseDirectory">The directory that contains the <c>.github</c> folder.</param>
        /// <param name="defaultPackageId">The default package identifier when no config exists.</param>
        public static async Task ExecutePackagingAsync(string baseDirectory, string defaultPackageId)
        {
            var githubDirectory = Path.Combine(baseDirectory, ".github");
            if (!Directory.Exists(githubDirectory))
            {
                await VS.MessageBox.ShowAsync(
                    "No .github folder found",
                    $"No .github folder found in '{baseDirectory}'.\nCreate a .github folder with your agent assets first.");
                return;
            }

            var outputDirectory = Path.Combine(baseDirectory, "bin", "packages");
            var configPath = Path.Combine(githubDirectory, ConfigFileName);

            var config = ReadOrCreateConfig(configPath, defaultPackageId);
            var packageContentHash = ComputePackageContentHash(githubDirectory);
            if (!string.Equals(config.ContentHash, packageContentHash, StringComparison.Ordinal))
            {
                config.Version = IncrementPatchVersion(config.Version);
            }

            await Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var dialog = new PackageConfigDialog(config);
            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            var updatedConfig = dialog.PackageConfig;
            if (!AreEquivalent(config, updatedConfig))
            {
                WriteConfig(configPath, updatedConfig);
            }

            try
            {
                var packagePath = CreatePackage(
                    githubDirectory,
                    outputDirectory,
                    updatedConfig.PackageId,
                    updatedConfig.Version,
                    updatedConfig.Description,
                    updatedConfig.Authors);

                updatedConfig.ContentHash = packageContentHash;
                WriteConfig(configPath, updatedConfig);

                await VS.MessageBox.ShowAsync("Package created", $"Package created successfully:\n{packagePath}");
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is IOException)
            {
                await VS.MessageBox.ShowAsync("Package creation failed", ex.Message);
            }
        }

        /// <summary>
        /// Creates a NuGet package that contains the <c>.github</c> assets and build targets.
        /// </summary>
        private static string CreatePackage(
            string githubDirectory,
            string outputDirectory,
            string packageId,
            string version,
            string description,
            string authors)
        {
            Directory.CreateDirectory(outputDirectory);

            var packagePath = Path.Combine(outputDirectory, $"{packageId}.{version}.nupkg");
            if (File.Exists(packagePath))
            {
                File.Delete(packagePath);
            }

            var builder = new PackageBuilder
            {
                Id = packageId,
                Version = NuGetVersion.Parse(version),
                Description = description
            };
            builder.Authors.Add(authors);

            foreach (var sourceFile in GetPackageSourceFiles(githubDirectory))
            {
                var relativePath = MakeRelativePath(githubDirectory, sourceFile);
                builder.Files.Add(new PhysicalPackageFile
                {
                    SourcePath = sourceFile,
                    TargetPath = ContentRoot + "/" + relativePath
                });
            }

            var tempTargetsPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            try
            {
                File.WriteAllText(tempTargetsPath, GenerateBuildTargets());
                builder.Files.Add(new PhysicalPackageFile
                {
                    SourcePath = tempTargetsPath,
                    TargetPath = $"build/{packageId}.targets"
                });

                using (var stream = File.Create(packagePath))
                {
                    builder.Save(stream);
                }
            }
            finally
            {
                File.Delete(tempTargetsPath);
            }

            return packagePath;
        }

        /// <summary>
        /// Generates the MSBuild targets file that materializes package content into the consuming project.
        /// </summary>
        private static string GenerateBuildTargets()
        {
            return @"<Project>
  <PropertyGroup>
    <_AgentTargetDir Condition=""'$(SolutionDir)' != '' And '$(SolutionDir)' != '*Undefined*'"">$(SolutionDir)</_AgentTargetDir>
    <_AgentTargetDir Condition=""'$(_AgentTargetDir)' == ''"">$(MSBuildProjectDirectory)\</_AgentTargetDir>
  </PropertyGroup>
  <UsingTask TaskName=""_MaterializeAgentContentTask"" TaskFactory=""RoslynCodeTaskFactory"" AssemblyFile=""$(MSBuildToolsPath)\Microsoft.Build.Tasks.Core.dll"">
    <ParameterGroup>
      <SourceDir ParameterType=""System.String"" Required=""true"" />
      <TargetDir ParameterType=""System.String"" Required=""true"" />
      <CopiedCount ParameterType=""System.Int32"" Output=""true"" />
      <SkippedCount ParameterType=""System.Int32"" Output=""true"" />
    </ParameterGroup>
    <Task>
      <Code Type=""Class"" Language=""cs""><![CDATA[
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

public class _MaterializeAgentContentTask : Task
{
    [Required] public string SourceDir { get; set; }
    [Required] public string TargetDir { get; set; }
    [Output] public int CopiedCount { get; set; }
    [Output] public int SkippedCount { get; set; }

    public override bool Execute()
    {
        CopiedCount = 0;
        SkippedCount = 0;
        if (!Directory.Exists(SourceDir)) return true;
        var manifestPath = Path.Combine(TargetDir, "".agent-manifest"");
        var manifest = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (File.Exists(manifestPath))
        {
            foreach (var line in File.ReadAllLines(manifestPath))
            {
                var sep = line.IndexOf('|');
                if (sep > 0) manifest[line.Substring(0, sep)] = line.Substring(sep + 1);
            }
        }
        if (!Directory.Exists(TargetDir)) Directory.CreateDirectory(TargetDir);
        var sourceRoot = SourceDir.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var updated = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var src in Directory.GetFiles(SourceDir, ""*"", SearchOption.AllDirectories))
        {
            var rel = src.Substring(sourceRoot.Length + 1);
            var dst = Path.Combine(TargetDir, rel);
            var srcHash = ComputeHash(src);
            updated[rel] = srcHash;
            if (File.Exists(dst))
            {
                var dstHash = ComputeHash(dst);
                if (manifest.TryGetValue(rel, out var prevHash) && !string.Equals(dstHash, prevHash, StringComparison.Ordinal))
                {
                    SkippedCount++;
                    continue;
                }
                if (string.Equals(dstHash, srcHash, StringComparison.Ordinal)) continue;
            }
            var dstDir = Path.GetDirectoryName(dst);
            if (!Directory.Exists(dstDir)) Directory.CreateDirectory(dstDir);
            File.Copy(src, dst, true);
            CopiedCount++;
        }
        var lines = new List<string>(updated.Count);
        foreach (var kv in updated) lines.Add(kv.Key + ""|"" + kv.Value);
        File.WriteAllLines(manifestPath, lines.ToArray());
        return true;
    }

    private static string ComputeHash(string filePath)
    {
        using (var sha = SHA256.Create())
        using (var stream = File.OpenRead(filePath))
        {
            return Convert.ToBase64String(sha.ComputeHash(stream));
        }
    }
}
]]></Code>
    </Task>
  </UsingTask>
  <Target Name=""_MaterializeAgentContent"" BeforeTargets=""BeforeBuild"" Condition=""'$(DesignTimeBuild)' != 'true'"">
    <_MaterializeAgentContentTask SourceDir=""$(MSBuildThisFileDirectory)..\agentcontent\github\"" TargetDir=""$(_AgentTargetDir).github\"">
      <Output TaskParameter=""CopiedCount"" PropertyName=""_AgentCopiedCount"" />
      <Output TaskParameter=""SkippedCount"" PropertyName=""_AgentSkippedCount"" />
    </_MaterializeAgentContentTask>
    <Message Text=""Agent content: $(_AgentCopiedCount) file(s) updated, $(_AgentSkippedCount) file(s) preserved (user-modified) in $(_AgentTargetDir).github"" Importance=""normal"" Condition=""$(_AgentCopiedCount) > 0 Or $(_AgentSkippedCount) > 0"" />
  </Target>
</Project>";
        }

        /// <summary>
        /// Reads the persisted package configuration or creates a default configuration when the file is missing.
        /// </summary>
        private static PackageConfig ReadOrCreateConfig(string configPath, string defaultPackageId)
        {
            if (File.Exists(configPath))
            {
                var json = File.ReadAllText(configPath);
                return JsonConvert.DeserializeObject<PackageConfig>(json);
            }

            var config = new PackageConfig
            {
                PackageId = defaultPackageId + " Agent Configuration",
                Version = "1.0.0",
                Description = "GitHub Copilot assets package",
                Authors = Environment.UserName,
                ContentHash = string.Empty
            };

            File.WriteAllText(configPath, JsonConvert.SerializeObject(config, Formatting.Indented));

            return config;
        }

        /// <summary>
        /// Determines whether two package configuration instances contain the same values.
        /// </summary>
        private static bool AreEquivalent(PackageConfig left, PackageConfig right)
        {
            if (left == null)
            {
                return right == null;
            }

            if (right == null)
            {
                return false;
            }

            return string.Equals(left.PackageId, right.PackageId, StringComparison.Ordinal)
                && string.Equals(left.Version, right.Version, StringComparison.Ordinal)
                && string.Equals(left.Description, right.Description, StringComparison.Ordinal)
                && string.Equals(left.Authors, right.Authors, StringComparison.Ordinal)
                && string.Equals(left.ContentHash, right.ContentHash, StringComparison.Ordinal);
        }

        /// <summary>
        /// Computes a stable hash for the files that are included in the package.
        /// </summary>
        private static string ComputePackageContentHash(string githubDirectory)
        {
            using (var algorithm = SHA256.Create())
            {
                foreach (var sourceFile in GetPackageSourceFiles(githubDirectory))
                {
                    HashBytes(algorithm, Encoding.UTF8.GetBytes(MakeRelativePath(githubDirectory, sourceFile)));
                    HashBytes(algorithm, new byte[] { 0 });

                    using (var stream = File.OpenRead(sourceFile))
                    {
                        var buffer = new byte[81920];
                        int bytesRead;
                        while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            algorithm.TransformBlock(buffer, 0, bytesRead, null, 0);
                        }
                    }

                    HashBytes(algorithm, new byte[] { 0 });
                }

                algorithm.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
                return Convert.ToBase64String(algorithm.Hash);
            }
        }

        /// <summary>
        /// Returns the source files that should be included in the package.
        /// </summary>
        private static IEnumerable<string> GetPackageSourceFiles(string githubDirectory)
        {
            return Directory.EnumerateFiles(githubDirectory, "*", SearchOption.AllDirectories)
                .Where(path => !Path.GetFileName(path).Equals(ConfigFileName, StringComparison.OrdinalIgnoreCase))
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Increments the patch component of a semantic version.
        /// </summary>
        private static string IncrementPatchVersion(string version)
        {
            var parsedVersion = NuGetVersion.Parse(version);
            return new NuGetVersion(parsedVersion.Major, parsedVersion.Minor, parsedVersion.Patch + 1).ToNormalizedString();
        }

        private static void HashBytes(HashAlgorithm algorithm, byte[] bytes)
        {
            algorithm.TransformBlock(bytes, 0, bytes.Length, null, 0);
        }

        private static void WriteConfig(string configPath, PackageConfig config)
        {
            File.WriteAllText(configPath, JsonConvert.SerializeObject(config, Formatting.Indented));
        }

        private static string MakeRelativePath(string basePath, string targetPath)
        {
            var baseUri = new Uri(basePath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar);
            var targetUri = new Uri(targetPath);
            var relativeUri = baseUri.MakeRelativeUri(targetUri);
            return Uri.UnescapeDataString(relativeUri.ToString());
        }
    }
}
