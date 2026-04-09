using ApmPackager;
using NUnit.Framework;

namespace ApmPackager.Tests;

[TestFixture]
public class PathSafetyTests
{
	[Test]
	public void GetRelativePath_ChildFile_ReturnsRelative()
	{
		var basePath = Path.Combine(Path.GetTempPath(), "project", ".github");
		var targetPath = Path.Combine(basePath, "agents", "test.md");

		var result = PathSafety.GetRelativePath(basePath, targetPath);

		Assert.That(result, Is.EqualTo("agents/test.md"));
	}

	[Test]
	public void GetRelativePath_DirectChild_ReturnsFileName()
	{
		var basePath = Path.Combine(Path.GetTempPath(), "project", ".github");
		var targetPath = Path.Combine(basePath, "readme.md");

		var result = PathSafety.GetRelativePath(basePath, targetPath);

		Assert.That(result, Is.EqualTo("readme.md"));
	}

	[Test]
	public void GetRelativePath_DeeplyNested_ReturnsFullRelative()
	{
		var basePath = Path.Combine(Path.GetTempPath(), "project", ".github");
		var targetPath = Path.Combine(basePath, "skills", "csharp", "nunit", "SKILL.md");

		var result = PathSafety.GetRelativePath(basePath, targetPath);

		Assert.That(result, Is.EqualTo("skills/csharp/nunit/SKILL.md"));
	}

	[Test]
	public void GetRelativePath_BaseWithTrailingSlash_WorksCorrectly()
	{
		var basePath = Path.Combine(Path.GetTempPath(), "project", ".github") + Path.DirectorySeparatorChar;
		var targetPath = Path.Combine(Path.GetTempPath(), "project", ".github", "agents", "test.md");

		var result = PathSafety.GetRelativePath(basePath, targetPath);

		Assert.That(result, Is.EqualTo("agents/test.md"));
	}

	[Test]
	public void GetRelativePath_BaseWithoutTrailingSlash_WorksCorrectly()
	{
		var basePath = Path.Combine(Path.GetTempPath(), "project", ".github");
		var targetPath = Path.Combine(basePath, "instructions", "copilot.md");

		var result = PathSafety.GetRelativePath(basePath, targetPath);

		Assert.That(result, Is.EqualTo("instructions/copilot.md"));
	}
}