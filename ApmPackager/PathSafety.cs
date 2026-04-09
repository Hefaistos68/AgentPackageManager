namespace ApmPackager
{
	/// <summary>
	/// Provides helpers for validating and normalizing relative paths used by the package workflow.
	/// </summary>
	internal static class PathSafety
	{
		/// <summary>
		/// Normalizes a relative path and rejects navigation segments.
		/// </summary>
		/// <param name="value">The relative path to normalize.</param>
		/// <returns>The normalized relative path using forward slashes.</returns>
		public static string NormalizeRelativePath(string value)
		{
			var normalized = value.Replace('\\', '/');
			var segments = normalized.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
			foreach (var segment in segments)
			{
				if (segment is "." or "..")
				{
					throw new InvalidOperationException($"Unsafe path segment '{segment}' in '{value}'.");
				}
			}

			return string.Join('/', segments);
		}

		/// <summary>
		/// Gets a safe relative path from a root directory to a file system path.
		/// </summary>
		/// <param name="rootDirectory">The base directory.</param>
		/// <param name="fullPath">The full path to convert.</param>
		/// <returns>A normalized relative path.</returns>
		public static string GetRelativePath(string rootDirectory, string fullPath)
		{
			var relativePath = Path.GetRelativePath(rootDirectory, fullPath);
			return NormalizeRelativePath(relativePath);
		}

		/// <summary>
		/// Combines a base directory with a relative path and ensures the result stays within the base directory.
		/// </summary>
		/// <param name="baseDirectory">The allowed base directory.</param>
		/// <param name="relativePath">The relative path to append.</param>
		/// <returns>The validated full path.</returns>
		public static string EnsureProjectPath(string baseDirectory, string relativePath)
		{
			var normalized = NormalizeRelativePath(relativePath);
			var fullPath = Path.GetFullPath(Path.Combine(baseDirectory, normalized));
			var fullBaseDirectory = Path.GetFullPath(baseDirectory);

			if (!fullPath.StartsWith(fullBaseDirectory, StringComparison.OrdinalIgnoreCase))
			{
				throw new InvalidOperationException($"Path '{relativePath}' escapes base directory '{baseDirectory}'.");
			}

			return fullPath;
		}
	}
}
