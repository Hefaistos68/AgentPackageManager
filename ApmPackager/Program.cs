namespace ApmPackager
{
	/// <summary>
	/// Provides the application entry point for the package command-line tool.
	/// </summary>
	internal static class Program
	{
		/// <summary>
		/// Executes the command-line tool.
		/// </summary>
		/// <param name="args">The command-line arguments.</param>
		/// <returns>The process exit code.</returns>
		private static int Main(string[] args)
		{
			try
			{
				return CommandLine.Execute(args);
			}
			catch (Exception exception)
			{
				Console.Error.WriteLine($"[x] {exception.Message}");
				return 1;
			}
		}
	}
}
