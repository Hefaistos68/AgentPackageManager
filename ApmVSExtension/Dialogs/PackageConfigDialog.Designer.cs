using System.ComponentModel;
using System.Windows.Forms;

namespace ApmVSExtension
{
    partial class PackageConfigDialog
    {
        private IContainer components;
        private FlowLayoutPanel buttonsPanel;
        private Label authorsLabel;
        private TextBox authorsTextBox;
        private Button cancelButton;
        private Label descriptionLabel;
        private TextBox descriptionTextBox;
        private TableLayoutPanel layoutPanel;
        private Button okButton;
        private Label packageIdLabel;
        private TextBox packageIdTextBox;
        private Label versionLabel;
        private TextBox versionTextBox;

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
			this.layoutPanel = new System.Windows.Forms.TableLayoutPanel();
			this.packageIdLabel = new System.Windows.Forms.Label();
			this.packageIdTextBox = new System.Windows.Forms.TextBox();
			this.versionLabel = new System.Windows.Forms.Label();
			this.versionTextBox = new System.Windows.Forms.TextBox();
			this.descriptionLabel = new System.Windows.Forms.Label();
			this.descriptionTextBox = new System.Windows.Forms.TextBox();
			this.authorsLabel = new System.Windows.Forms.Label();
			this.authorsTextBox = new System.Windows.Forms.TextBox();
			this.buttonsPanel = new System.Windows.Forms.FlowLayoutPanel();
			this.cancelButton = new System.Windows.Forms.Button();
			this.okButton = new System.Windows.Forms.Button();
			this.layoutPanel.SuspendLayout();
			this.buttonsPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// layoutPanel
			// 
			this.layoutPanel.AutoSize = true;
			this.layoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.layoutPanel.ColumnCount = 2;
			this.layoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.layoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.layoutPanel.Controls.Add(this.packageIdLabel, 0, 0);
			this.layoutPanel.Controls.Add(this.packageIdTextBox, 1, 0);
			this.layoutPanel.Controls.Add(this.versionLabel, 0, 1);
			this.layoutPanel.Controls.Add(this.versionTextBox, 1, 1);
			this.layoutPanel.Controls.Add(this.descriptionLabel, 0, 2);
			this.layoutPanel.Controls.Add(this.descriptionTextBox, 1, 2);
			this.layoutPanel.Controls.Add(this.authorsLabel, 0, 3);
			this.layoutPanel.Controls.Add(this.authorsTextBox, 1, 3);
			this.layoutPanel.Controls.Add(this.buttonsPanel, 0, 4);
			this.layoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.layoutPanel.Location = new System.Drawing.Point(10, 10);
			this.layoutPanel.Margin = new System.Windows.Forms.Padding(0);
			this.layoutPanel.Name = "layoutPanel";
			this.layoutPanel.RowCount = 5;
			this.layoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.layoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.layoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.layoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.layoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.layoutPanel.Size = new System.Drawing.Size(345, 188);
			this.layoutPanel.TabIndex = 0;
			// 
			// packageIdLabel
			// 
			this.packageIdLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.packageIdLabel.AutoSize = true;
			this.packageIdLabel.Location = new System.Drawing.Point(0, 6);
			this.packageIdLabel.Margin = new System.Windows.Forms.Padding(0, 5, 7, 5);
			this.packageIdLabel.Name = "packageIdLabel";
			this.packageIdLabel.Size = new System.Drawing.Size(67, 13);
			this.packageIdLabel.TabIndex = 0;
			this.packageIdLabel.Text = "Package Name:";
			// 
			// packageIdTextBox
			// 
			this.packageIdTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.packageIdTextBox.Location = new System.Drawing.Point(74, 3);
			this.packageIdTextBox.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
			this.packageIdTextBox.Name = "packageIdTextBox";
			this.packageIdTextBox.Size = new System.Drawing.Size(271, 20);
			this.packageIdTextBox.TabIndex = 1;
			// 
			// versionLabel
			// 
			this.versionLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.versionLabel.AutoSize = true;
			this.versionLabel.Location = new System.Drawing.Point(0, 32);
			this.versionLabel.Margin = new System.Windows.Forms.Padding(0, 5, 7, 5);
			this.versionLabel.Name = "versionLabel";
			this.versionLabel.Size = new System.Drawing.Size(45, 13);
			this.versionLabel.TabIndex = 2;
			this.versionLabel.Text = "Version:";
			// 
			// versionTextBox
			// 
			this.versionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.versionTextBox.Location = new System.Drawing.Point(74, 29);
			this.versionTextBox.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
			this.versionTextBox.Name = "versionTextBox";
			this.versionTextBox.Size = new System.Drawing.Size(271, 20);
			this.versionTextBox.TabIndex = 3;
			// 
			// descriptionLabel
			// 
			this.descriptionLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.descriptionLabel.AutoSize = true;
			this.descriptionLabel.Location = new System.Drawing.Point(0, 80);
			this.descriptionLabel.Margin = new System.Windows.Forms.Padding(0, 5, 7, 5);
			this.descriptionLabel.Name = "descriptionLabel";
			this.descriptionLabel.Size = new System.Drawing.Size(63, 13);
			this.descriptionLabel.TabIndex = 4;
			this.descriptionLabel.Text = "Description:";
			// 
			// descriptionTextBox
			// 
			this.descriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.descriptionTextBox.Location = new System.Drawing.Point(74, 55);
			this.descriptionTextBox.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
			this.descriptionTextBox.Multiline = true;
			this.descriptionTextBox.Name = "descriptionTextBox";
			this.descriptionTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.descriptionTextBox.Size = new System.Drawing.Size(271, 63);
			this.descriptionTextBox.TabIndex = 5;
			// 
			// authorsLabel
			// 
			this.authorsLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.authorsLabel.AutoSize = true;
			this.authorsLabel.Location = new System.Drawing.Point(0, 127);
			this.authorsLabel.Margin = new System.Windows.Forms.Padding(0, 5, 7, 5);
			this.authorsLabel.Name = "authorsLabel";
			this.authorsLabel.Size = new System.Drawing.Size(46, 13);
			this.authorsLabel.TabIndex = 6;
			this.authorsLabel.Text = "Authors:";
			// 
			// authorsTextBox
			// 
			this.authorsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.authorsTextBox.Location = new System.Drawing.Point(74, 124);
			this.authorsTextBox.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
			this.authorsTextBox.Name = "authorsTextBox";
			this.authorsTextBox.Size = new System.Drawing.Size(271, 20);
			this.authorsTextBox.TabIndex = 7;
			// 
			// buttonsPanel
			// 
			this.buttonsPanel.AutoSize = true;
			this.buttonsPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.layoutPanel.SetColumnSpan(this.buttonsPanel, 2);
			this.buttonsPanel.Controls.Add(this.cancelButton);
			this.buttonsPanel.Controls.Add(this.okButton);
			this.buttonsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.buttonsPanel.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
			this.buttonsPanel.Location = new System.Drawing.Point(0, 157);
			this.buttonsPanel.Margin = new System.Windows.Forms.Padding(0, 10, 0, 0);
			this.buttonsPanel.Name = "buttonsPanel";
			this.buttonsPanel.Size = new System.Drawing.Size(345, 31);
			this.buttonsPanel.TabIndex = 8;
			// 
			// cancelButton
			// 
			this.cancelButton.AutoSize = true;
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(278, 3);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(64, 23);
			this.cancelButton.TabIndex = 1;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// okButton
			// 
			this.okButton.AutoSize = true;
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Location = new System.Drawing.Point(208, 3);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(64, 23);
			this.okButton.TabIndex = 0;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			// 
			// PackageConfigDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(365, 208);
			this.Controls.Add(this.layoutPanel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "PackageConfigDialog";
			this.Padding = new System.Windows.Forms.Padding(10);
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Agent Package Configuration";
			this.Load += new System.EventHandler(this.PackageConfigDialog_Load);
			this.layoutPanel.ResumeLayout(false);
			this.layoutPanel.PerformLayout();
			this.buttonsPanel.ResumeLayout(false);
			this.buttonsPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }
    }
}
