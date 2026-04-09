using System.IO;

namespace ApmVSExtension
{
	[Command(PackageIds.CreateAgentPackageCommand)]
	/// <summary>
	/// Creates a NuGet package from the solution-level <c>.github</c> folder.
	/// </summary>
	internal sealed class CreateAgentPackageCommand : BaseCommand<CreateAgentPackageCommand>
	{
		/// <summary>
		/// Executes the command for the current solution.
		/// </summary>
		/// <param name="e">The Visual Studio menu command event arguments.</param>
		protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
		{
			var solution = await VS.Solutions.GetCurrentSolutionAsync();
			if (solution == null)
			{
				await VS.MessageBox.ShowAsync("No solution is currently open.");
				return;
			}

			var solutionDirectory = Path.GetDirectoryName(solution.FullPath);
			if (string.IsNullOrEmpty(solutionDirectory))
			{
				await VS.MessageBox.ShowAsync("Unable to determine the solution directory.");
				return;
			}

			var solutionName = Path.GetFileNameWithoutExtension(solution.FullPath);
			await AgentPackageHelper.ExecutePackagingAsync(solutionDirectory, solutionName);
		}
	}
}
