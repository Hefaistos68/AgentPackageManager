using System.IO;

namespace ApmVSExtension
{
    [Command(PackageIds.CreateProjectAgentPackageCommand)]
    /// <summary>
    /// Creates a NuGet package from a project-level <c>.github</c> folder.
    /// </summary>
    internal sealed class CreateProjectAgentPackageCommand : BaseCommand<CreateProjectAgentPackageCommand>
    {
        /// <summary>
        /// Executes the command for the selected project.
        /// </summary>
        /// <param name="e">The Visual Studio menu command event arguments.</param>
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            var selectedItems = await VS.Solutions.GetActiveItemsAsync();
            SolutionItem project = null;

            foreach (var item in selectedItems)
            {
                if (item.Type == SolutionItemType.Project || item.Type == SolutionItemType.VirtualProject)
                {
                    project = item;
                    break;
                }
            }

            if (project == null)
            {
                await VS.MessageBox.ShowAsync("No project selected.");
                return;
            }

            var projectDirectory = Path.GetDirectoryName(project.FullPath);
            if (string.IsNullOrEmpty(projectDirectory))
            {
                await VS.MessageBox.ShowAsync("Unable to determine the project directory.");
                return;
            }

            var projectName = Path.GetFileNameWithoutExtension(project.FullPath);
            await AgentPackageHelper.ExecutePackagingAsync(projectDirectory, projectName);
        }
    }
}
