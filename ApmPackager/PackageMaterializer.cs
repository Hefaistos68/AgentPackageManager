using System.IO.Compression;

namespace ApmPackager
{
	/// <summary>
	/// Materializes packaged <c>.github</c> content into a target project directory.
	/// </summary>
	internal static class PackageMaterializer
	{
		/// <summary>
		/// Materializes package content into the specified project directory.
		/// </summary>
		/// <param name="options">The materialization options.</param>
		public static void Materialize(MaterializeOptions options)
		{
			var projectDirectory = Path.GetFullPath(options.ProjectDirectory);
			Directory.CreateDirectory(projectDirectory);

			if (options.PackagePath.EndsWith(".nupkg", StringComparison.OrdinalIgnoreCase))
			{
				MaterializeFromPackageFile(Path.GetFullPath(options.PackagePath), projectDirectory, options.Force);
				return;
			}

			MaterializeFromDirectory(Path.GetFullPath(options.PackagePath), projectDirectory, options.Force);
		}

		/// <summary>
		/// Materializes content directly from a package archive.
		/// </summary>
		/// <param name="packagePath">The path to the package archive.</param>
		/// <param name="projectDirectory">The destination project directory.</param>
		/// <param name="force"><see langword="true"/> to overwrite existing files; otherwise, <see langword="false"/>.</param>
		private static void MaterializeFromPackageFile(string packagePath, string projectDirectory, bool force)
		{
			if (!File.Exists(packagePath))
			{
				throw new InvalidOperationException($"Package file '{packagePath}' does not exist.");
			}

			using var archive = ZipFile.OpenRead(packagePath);
			foreach (var entry in archive.Entries)
			{
				if (string.IsNullOrEmpty(entry.Name))
				{
					continue;
				}

				if (!entry.FullName.StartsWith(PackageLayout.ContentRootWithTrailingSlash, StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}

				var relativePath = PathSafety.NormalizeRelativePath(entry.FullName[PackageLayout.ContentRootWithTrailingSlash.Length..]);
				WriteFile(relativePath, projectDirectory, force, destinationStream =>
				{
					using var sourceStream = entry.Open();
					sourceStream.CopyTo(destinationStream);
				});
			}
		}

		/// <summary>
		/// Materializes content from an extracted package directory.
		/// </summary>
		/// <param name="packageDirectory">The extracted package directory.</param>
		/// <param name="projectDirectory">The destination project directory.</param>
		/// <param name="force"><see langword="true"/> to overwrite existing files; otherwise, <see langword="false"/>.</param>
		private static void MaterializeFromDirectory(string packageDirectory, string projectDirectory, bool force)
		{
			var contentDirectory = PackageLayout.GetContentDirectory(packageDirectory);
			if (!Directory.Exists(contentDirectory))
			{
				throw new InvalidOperationException($"Package directory '{packageDirectory}' does not contain '{contentDirectory}'.");
			}

			foreach (var sourceFile in Directory.EnumerateFiles(contentDirectory, "*", SearchOption.AllDirectories))
			{
				var relativePath = PathSafety.GetRelativePath(contentDirectory, sourceFile);
				WriteFile(relativePath, projectDirectory, force, destinationStream =>
				{
					using var sourceStream = File.OpenRead(sourceFile);
					sourceStream.CopyTo(destinationStream);
				});
			}
		}

		/// <summary>
		/// Writes one materialized file into the destination project directory.
		/// </summary>
		/// <param name="relativePath">The relative path under the destination <c>.github</c> folder.</param>
		/// <param name="projectDirectory">The destination project directory.</param>
		/// <param name="force"><see langword="true"/> to overwrite existing files; otherwise, <see langword="false"/>.</param>
		/// <param name="writeAction">The delegate that writes the destination file content.</param>
		private static void WriteFile(string relativePath, string projectDirectory, bool force, Action<FileStream> writeAction)
		{
			var destinationPath = PathSafety.EnsureProjectPath(projectDirectory, Path.Combine(".github", relativePath));
			var destinationDirectory = Path.GetDirectoryName(destinationPath);
			if (!string.IsNullOrEmpty(destinationDirectory))
			{
				Directory.CreateDirectory(destinationDirectory);
			}

			if (File.Exists(destinationPath) && !force)
			{
				throw new InvalidOperationException($"Destination file '{destinationPath}' already exists. Re-run with --force to overwrite it.");
			}

			using var destinationStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None);
			writeAction(destinationStream);
		}
	}
}
