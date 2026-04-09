namespace ApmPackager
{
	/// <summary>
	/// Defines the package paths shared by package creation and materialization.
	/// </summary>
	internal static class PackageLayout
	{
		/// <summary>
		/// The package path that stores the <c>.github</c> content payload.
		/// </summary>
		public const string ContentRoot = "agentcontent/github";

		/// <summary>
		/// The package content root with a trailing slash for prefix comparisons.
		/// </summary>
		public const string ContentRootWithTrailingSlash = ContentRoot + "/";

		/// <summary>
		/// The file name used for persisted package metadata.
		/// </summary>
		public const string ConfigFileName = "agent-package.json";

		/// <summary>
		/// Gets the extracted package directory that contains the <c>.github</c> content.
		/// </summary>
		/// <param name="packageDirectory">The root directory of the extracted package.</param>
		/// <returns>The content directory path.</returns>
		public static string GetContentDirectory(string packageDirectory)
		{
			return Path.Combine(packageDirectory, "agentcontent", "github");
		}

		/// <summary>
		/// Generates the MSBuild targets content that materializes package files into the consuming project.
		/// </summary>
		/// <returns>The <c>.targets</c> file content.</returns>
		public static string GenerateBuildTargets()
		{
			return """
				<Project>
				  <PropertyGroup>
					<_AgentTargetDir Condition="'$(SolutionDir)' != '' And '$(SolutionDir)' != '*Undefined*'">$(SolutionDir)</_AgentTargetDir>
					<_AgentTargetDir Condition="'$(_AgentTargetDir)' == ''">$(MSBuildProjectDirectory)\</_AgentTargetDir>
				  </PropertyGroup>
				  <UsingTask TaskName="_MaterializeAgentContentTask" TaskFactory="RoslynCodeTaskFactory" AssemblyFile="$(MSBuildToolsPath)\Microsoft.Build.Tasks.Core.dll">
					<ParameterGroup>
					  <SourceDir ParameterType="System.String" Required="true" />
					  <TargetDir ParameterType="System.String" Required="true" />
					  <CopiedCount ParameterType="System.Int32" Output="true" />
					  <SkippedCount ParameterType="System.Int32" Output="true" />
					</ParameterGroup>
					<Task>
					  <Code Type="Class" Language="cs"><![CDATA[
				using System;
				using System.Collections.Generic;
				using System.IO;
				using System.Security.Cryptography;
				using Microsoft.Build.Framework;
				using Microsoft.Build.Utilities;

				public class _MaterializeAgentContentTask : Task
				{
					[Required] public string SourceDir { get; set; }
					[Required] public string TargetDir { get; set; }
					[Output] public int CopiedCount { get; set; }
					[Output] public int SkippedCount { get; set; }

					public override bool Execute()
					{
						CopiedCount = 0;
						SkippedCount = 0;
						if (!Directory.Exists(SourceDir)) return true;
						var manifestPath = Path.Combine(TargetDir, ".agent-manifest");
						var manifest = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
						if (File.Exists(manifestPath))
						{
							foreach (var line in File.ReadAllLines(manifestPath))
							{
								var sep = line.IndexOf('|');
								if (sep > 0) manifest[line.Substring(0, sep)] = line.Substring(sep + 1);
							}
						}
						if (!Directory.Exists(TargetDir)) Directory.CreateDirectory(TargetDir);
						var sourceRoot = SourceDir.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
						var updated = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
						foreach (var src in Directory.GetFiles(SourceDir, "*", SearchOption.AllDirectories))
						{
							var rel = src.Substring(sourceRoot.Length + 1);
							var dst = Path.Combine(TargetDir, rel);
							var srcHash = ComputeHash(src);
							updated[rel] = srcHash;
							if (File.Exists(dst))
							{
								var dstHash = ComputeHash(dst);
								if (manifest.TryGetValue(rel, out var prevHash) && !string.Equals(dstHash, prevHash, StringComparison.Ordinal))
								{
									SkippedCount++;
									continue;
								}
								if (string.Equals(dstHash, srcHash, StringComparison.Ordinal)) continue;
							}
							var dstDir = Path.GetDirectoryName(dst);
							if (!Directory.Exists(dstDir)) Directory.CreateDirectory(dstDir);
							File.Copy(src, dst, true);
							CopiedCount++;
						}
						var lines = new List<string>(updated.Count);
						foreach (var kv in updated) lines.Add(kv.Key + "|" + kv.Value);
						File.WriteAllLines(manifestPath, lines.ToArray());
						return true;
					}

					private static string ComputeHash(string filePath)
					{
						using (var sha = SHA256.Create())
						using (var stream = File.OpenRead(filePath))
						{
							return Convert.ToBase64String(sha.ComputeHash(stream));
						}
					}
				}
				]]></Code>
					</Task>
				  </UsingTask>
				  <Target Name="_MaterializeAgentContent" BeforeTargets="BeforeBuild" Condition="'$(DesignTimeBuild)' != 'true'">
					<_MaterializeAgentContentTask SourceDir="$(MSBuildThisFileDirectory)..\agentcontent\github\" TargetDir="$(_AgentTargetDir).github\">
					  <Output TaskParameter="CopiedCount" PropertyName="_AgentCopiedCount" />
					  <Output TaskParameter="SkippedCount" PropertyName="_AgentSkippedCount" />
					</_MaterializeAgentContentTask>
					<Message Text="Agent content: $(_AgentCopiedCount) file(s) updated, $(_AgentSkippedCount) file(s) preserved (user-modified) in $(_AgentTargetDir).github" Importance="normal" Condition="$(_AgentCopiedCount) > 0 Or $(_AgentSkippedCount) > 0" />
				  </Target>
				</Project>
				""";
		}
	}
}
