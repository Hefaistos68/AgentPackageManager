namespace ApmPackager
{
  /// <summary>
	/// Represents the inputs required to materialize package content into a project.
	/// </summary>
	internal sealed record MaterializeOptions
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MaterializeOptions"/> record.
		/// </summary>
		/// <param name="packagePath">The path to the package file or extracted package directory.</param>
		/// <param name="projectDirectory">The directory that receives the materialized <c>.github</c> content.</param>
		/// <param name="force"><see langword="true"/> to overwrite existing files; otherwise, <see langword="false"/>.</param>
		public MaterializeOptions(string packagePath, string projectDirectory, bool force)
		{
			PackagePath = packagePath;
			ProjectDirectory = projectDirectory;
			Force = force;
		}

		/// <summary>
		/// Gets the path to the package file or extracted package directory.
		/// </summary>
		public string PackagePath { get; }

		/// <summary>
		/// Gets the directory that receives the materialized <c>.github</c> content.
		/// </summary>
		public string ProjectDirectory { get; }

		/// <summary>
		/// Gets a value indicating whether existing files can be overwritten.
		/// </summary>
		public bool Force { get; }
	}
}
