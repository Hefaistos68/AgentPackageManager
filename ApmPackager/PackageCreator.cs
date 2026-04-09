using System.Text.Json;
using NuGet.Packaging;
using NuGet.Versioning;

namespace ApmPackager
{
	/// <summary>
	/// Creates NuGet packages from <c>.github</c> content.
	/// </summary>
	internal static class PackageCreator
	{
		/// <summary>
		/// Controls JSON serialization for persisted package configuration files.
		/// </summary>
		private static readonly JsonSerializerOptions JsonSerializerOptions = new()
		{
			WriteIndented = true
		};

		/// <summary>
		/// Creates a NuGet package from the configured source directory.
		/// </summary>
		/// <param name="options">The pack operation options.</param>
		/// <returns>The full path to the generated package file.</returns>
		public static string Create(PackOptions options)
		{
			var sourceDirectory = Path.GetFullPath(options.SourceDirectory);
			var githubDirectory = Path.Combine(sourceDirectory, ".github");
			if (!Directory.Exists(githubDirectory))
			{
				throw new InvalidOperationException($"Source directory '{sourceDirectory}' does not contain a .github folder.");
			}

			var outputDirectory = Path.GetFullPath(options.OutputDirectory);
			Directory.CreateDirectory(outputDirectory);

			var config = ReadOrCreateConfig(githubDirectory, Path.GetFileName(sourceDirectory));

			var packagePath = Path.Combine(outputDirectory, $"{config.PackageId}.{config.Version}.nupkg");
			if (File.Exists(packagePath))
			{
				if (!options.Force)
				{
					throw new InvalidOperationException($"Package '{packagePath}' already exists. Re-run with --force to overwrite it.");
				}

				File.Delete(packagePath);
			}

			var builder = new PackageBuilder();
			builder.Id = config.PackageId;
			builder.Version = NuGetVersion.Parse(config.Version);
			builder.Description = config.Description;
			builder.Authors.Add(config.Authors);

			foreach (var sourceFile in EnumerateFiles(githubDirectory))
			{
				var relativePath = PathSafety.GetRelativePath(githubDirectory, sourceFile);
				builder.Files.Add(new PhysicalPackageFile
				{
					SourcePath = sourceFile,
					TargetPath = $"{PackageLayout.ContentRoot}/{relativePath}"
				});
			}

			var tempTargetsPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			try
			{
				File.WriteAllText(tempTargetsPath, PackageLayout.GenerateBuildTargets());
				builder.Files.Add(new PhysicalPackageFile
				{
					SourcePath = tempTargetsPath,
					TargetPath = $"build/{config.PackageId}.targets"
				});

				using var stream = File.Create(packagePath);
				builder.Save(stream);
			}
			finally
			{
				File.Delete(tempTargetsPath);
			}

			return packagePath;
		}

		/// <summary>
		/// Enumerates the source files that should be included in the package payload.
		/// </summary>
		/// <param name="rootDirectory">The root <c>.github</c> directory.</param>
		/// <returns>The ordered package payload files.</returns>
		private static IReadOnlyList<string> EnumerateFiles(string rootDirectory)
		{
			return Directory
				.EnumerateFiles(rootDirectory, "*", SearchOption.AllDirectories)
			  .Where(path => !IsExcludedPackageContent(path))
				.OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
				.ToArray();
		}

		/// <summary>
		/// Determines whether a file should be excluded from the package payload.
		/// </summary>
		/// <param name="fullPath">The file path to inspect.</param>
		/// <returns><see langword="true"/> when the file should be excluded; otherwise, <see langword="false"/>.</returns>
		private static bool IsExcludedPackageContent(string fullPath)
		{
			return string.Equals(Path.GetFileName(fullPath), PackageLayout.ConfigFileName, StringComparison.OrdinalIgnoreCase);
		}

		/// <summary>
		/// Reads the package configuration from disk or creates a default configuration when none exists.
		/// </summary>
		/// <param name="githubDirectory">The source <c>.github</c> directory.</param>
		/// <param name="defaultPackageId">The default package identifier to use.</param>
		/// <returns>The resolved package configuration.</returns>
		private static PackageConfig ReadOrCreateConfig(string githubDirectory, string defaultPackageId)
		{
			var configPath = Path.Combine(githubDirectory, PackageLayout.ConfigFileName);
			var config = File.Exists(configPath)
				? ReadConfig(configPath, defaultPackageId)
				: PackageConfig.CreateDefault(defaultPackageId);

			File.WriteAllText(configPath, JsonSerializer.Serialize(config, JsonSerializerOptions));
			return config;
		}

		/// <summary>
		/// Reads and normalizes a package configuration file.
		/// </summary>
		/// <param name="configPath">The path to the configuration file.</param>
		/// <param name="defaultPackageId">The default package identifier to use for missing values.</param>
		/// <returns>The normalized package configuration.</returns>
		private static PackageConfig ReadConfig(string configPath, string defaultPackageId)
		{
			var json = File.ReadAllText(configPath);
			var config = JsonSerializer.Deserialize<PackageConfig>(json);
			if (config is null)
			{
				throw new InvalidOperationException($"Package config '{configPath}' is invalid.");
			}

			var packageId = string.IsNullOrWhiteSpace(config.PackageId) ? defaultPackageId : config.PackageId;
			var version = string.IsNullOrWhiteSpace(config.Version) ? "1.0.0" : config.Version;
			var description = string.IsNullOrWhiteSpace(config.Description) ? "GitHub Copilot assets package" : config.Description;
			var authors = string.IsNullOrWhiteSpace(config.Authors) ? Environment.UserName : config.Authors;
			var contentHash = config.ContentHash ?? string.Empty;

			return new PackageConfig(packageId, version, description, authors, contentHash);
		}
	}
}
