using System;
using System.Windows.Forms;

namespace ApmVSExtension
{
	/// <summary>
	/// Provides a modal dialog for editing package metadata persisted in <c>.github\agent-package.json</c>.
	/// </summary>
	internal sealed partial class PackageConfigDialog : Form
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="PackageConfigDialog"/> class.
		/// </summary>
		/// <param name="config">The initial package configuration values.</param>
		public PackageConfigDialog(PackageConfig config)
		{
			if (config == null)
			{
				throw new ArgumentNullException(nameof(config));
			}

			InitializeComponent();

			packageIdTextBox.Text = config.PackageId ?? string.Empty;
			versionTextBox.Text = config.Version ?? string.Empty;
			descriptionTextBox.Text = config.Description ?? string.Empty;
			authorsTextBox.Text = config.Authors ?? string.Empty;
			PackageConfig = config;
		}

		/// <summary>
		/// Gets the package configuration represented by the current dialog values.
		/// </summary>
		public PackageConfig PackageConfig { get; private set; }

		/// <inheritdoc/>
		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			if (DialogResult == DialogResult.OK)
			{
				PackageConfig = new PackageConfig
				{
					PackageId = packageIdTextBox.Text.Trim(),
					Version = versionTextBox.Text.Trim(),
					Description = descriptionTextBox.Text.Trim(),
					Authors = authorsTextBox.Text.Trim()
				};
			}

			base.OnFormClosing(e);
		}

		private void PackageConfigDialog_Load(object sender, EventArgs e)
		{

		}
	}
}
