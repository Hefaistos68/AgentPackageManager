using ApmPackager;
using NUnit.Framework;
using System.Xml;

namespace ApmPackager.Tests;

[TestFixture]
public class PackageLayoutTests
{
	[Test]
	public void ContentRoot_IsExpectedPath()
	{
		Assert.That(PackageLayout.ContentRoot, Is.EqualTo("agentcontent/github"));
	}

	[Test]
	public void ContentRootWithTrailingSlash_EndsWithSlash()
	{
		Assert.That(PackageLayout.ContentRootWithTrailingSlash, Is.EqualTo("agentcontent/github/"));
	}

	[Test]
	public void ConfigFileName_IsExpectedValue()
	{
		Assert.That(PackageLayout.ConfigFileName, Is.EqualTo("agent-package.json"));
	}

	[Test]
	public void GetContentDirectory_CombinesPathCorrectly()
	{
		var packageDir = Path.Combine("C:", "packages", "mypackage");

		var result = PackageLayout.GetContentDirectory(packageDir);

		Assert.That(result, Is.EqualTo(Path.Combine(packageDir, "agentcontent", "github")));
	}

	[Test]
	public void GenerateBuildTargets_IsValidXml()
	{
		var targets = PackageLayout.GenerateBuildTargets();
		var doc = new XmlDocument();

		Assert.DoesNotThrow(() => doc.LoadXml(targets.Trim()));
	}

	[Test]
	public void GenerateBuildTargets_ContainsProjectElement()
	{
		var targets = PackageLayout.GenerateBuildTargets();

		Assert.Multiple(() =>
		{
			Assert.That(targets, Does.Contain("<Project>"));
			Assert.That(targets, Does.Contain("</Project>"));
		});
	}

	[Test]
	public void GenerateBuildTargets_ContainsTargetName()
	{
		var targets = PackageLayout.GenerateBuildTargets();

		Assert.That(targets, Does.Contain("_MaterializeAgentContent"));
	}

	[Test]
	public void GenerateBuildTargets_ContainsSolutionDirFallback()
	{
		var targets = PackageLayout.GenerateBuildTargets();

		Assert.Multiple(() =>
		{
			Assert.That(targets, Does.Contain("$(SolutionDir)"));
			Assert.That(targets, Does.Contain("$(MSBuildProjectDirectory)"));
			Assert.That(targets, Does.Contain("_AgentTargetDir"));
		});
	}

	[Test]
	public void GenerateBuildTargets_ContainsAgentContentSourceGlob()
	{
		var targets = PackageLayout.GenerateBuildTargets();

		Assert.That(targets, Does.Contain(@"agentcontent\github\"));
	}

	[Test]
	public void GenerateBuildTargets_CopiesToGithubDestination()
	{
		var targets = PackageLayout.GenerateBuildTargets();

		Assert.That(targets, Does.Contain(".github"));
	}

	[Test]
	public void GenerateBuildTargets_SkipsDesignTimeBuild()
	{
		var targets = PackageLayout.GenerateBuildTargets();

		Assert.That(targets, Does.Contain("DesignTimeBuild"));
	}

	[Test]
	public void GenerateBuildTargets_UsesRoslynCodeTaskFactory()
	{
		var targets = PackageLayout.GenerateBuildTargets();

		Assert.That(targets, Does.Contain("RoslynCodeTaskFactory"));
	}

	[Test]
	public void GenerateBuildTargets_ContainsManifestTracking()
	{
		var targets = PackageLayout.GenerateBuildTargets();

		Assert.That(targets, Does.Contain(".agent-manifest"));
	}

	[Test]
	public void GenerateBuildTargets_ContainsCopiedAndSkippedOutputs()
	{
		var targets = PackageLayout.GenerateBuildTargets();

		Assert.Multiple(() =>
		{
			Assert.That(targets, Does.Contain("SkippedCount"));
			Assert.That(targets, Does.Contain("CopiedCount"));
		});
	}

	[Test]
	public void GenerateBuildTargets_ContainsHashComputation()
	{
		var targets = PackageLayout.GenerateBuildTargets();

		Assert.That(targets, Does.Contain("ComputeHash"));
	}

	[Test]
	public void GenerateBuildTargets_ContainsBuildMessage()
	{
		var targets = PackageLayout.GenerateBuildTargets();

		Assert.Multiple(() =>
		{
			Assert.That(targets, Does.Contain("<Message"));
			Assert.That(targets, Does.Contain("user-modified"));
		});
	}
}