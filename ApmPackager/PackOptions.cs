namespace ApmPackager
{
 /// <summary>
	/// Represents the inputs required to create a package from a source directory.
	/// </summary>
	internal sealed record PackOptions
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="PackOptions"/> record.
		/// </summary>
		/// <param name="sourceDirectory">The directory that contains the <c>.github</c> folder to package.</param>
		/// <param name="outputDirectory">The directory where the generated package is written.</param>
		/// <param name="force"><see langword="true"/> to overwrite an existing package file; otherwise, <see langword="false"/>.</param>
		public PackOptions(string sourceDirectory, string outputDirectory, bool force)
		{
			SourceDirectory = sourceDirectory;
			OutputDirectory = outputDirectory;
			Force = force;
		}

		/// <summary>
		/// Gets the directory that contains the <c>.github</c> folder to package.
		/// </summary>
		public string SourceDirectory { get; }

		/// <summary>
		/// Gets the directory where the generated package is written.
		/// </summary>
		public string OutputDirectory { get; }

		/// <summary>
		/// Gets a value indicating whether an existing package file can be overwritten.
		/// </summary>
		public bool Force { get; }
	}
}
