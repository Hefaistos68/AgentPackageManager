global using Community.VisualStudio.Toolkit;

global using Microsoft.VisualStudio.Shell;

global using System;

global using Task = System.Threading.Tasks.Task;

using System.Runtime.InteropServices;
using System.Threading;

namespace ApmVSExtension
{
	[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
	[InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
	[ProvideMenuResource("Menus.ctmenu", 1)]
	[Guid(PackageGuids.ApmVSExtensionString)]
	/// <summary>
	/// Registers the Visual Studio package and its commands.
	/// </summary>
	public sealed class ApmVSExtensionPackage : ToolkitPackage
	{
		/// <summary>
		/// Initializes the package and registers its commands.
		/// </summary>
		/// <param name="cancellationToken">The token that cancels package initialization.</param>
		/// <param name="progress">The Visual Studio progress reporter.</param>
		protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
		{
			await this.RegisterCommandsAsync();
		}
	}
}