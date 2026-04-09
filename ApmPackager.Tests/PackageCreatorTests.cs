using ApmPackager;
using NuGet.Packaging;
using NUnit.Framework;

namespace ApmPackager.Tests;

[TestFixture]
public class PackageCreatorTests
{
	private string _tempDir = null!;

	[SetUp]
	public void SetUp()
	{
		_tempDir = Path.Combine(Path.GetTempPath(), "ApmCreatorTests_" + Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(_tempDir);
	}

	[TearDown]
	public void TearDown()
	{
		if (Directory.Exists(_tempDir))
		{
			Directory.Delete(_tempDir, recursive: true);
		}
	}

	[Test]
	public void Create_ValidSource_CreatesNupkgFile()
	{
		var sourceDir = CreateSource("content.md", "# Hello");

		var packagePath = PackageCreator.Create(new PackOptions(sourceDir, Path.Combine(_tempDir, "out"), force: false));

		Assert.Multiple(() =>
		{
			Assert.That(File.Exists(packagePath), Is.True);
			Assert.That(packagePath, Does.EndWith(".nupkg"));
		});
	}

	[Test]
	public void Create_NoGithubDirectory_Throws()
	{
		var sourceDir = Path.Combine(_tempDir, "empty");
		Directory.CreateDirectory(sourceDir);

		Assert.That(
			() => PackageCreator.Create(new PackOptions(sourceDir, Path.Combine(_tempDir, "out"), force: false)),
			Throws.TypeOf<InvalidOperationException>());
	}

	[Test]
	public void Create_ExistingPackageWithoutForce_Throws()
	{
		var sourceDir = CreateSource("c.md", "# C");
		var outputDir = Path.Combine(_tempDir, "out");
		var opts = new PackOptions(sourceDir, outputDir, force: false);

		PackageCreator.Create(opts);

		Assert.That(() => PackageCreator.Create(opts), Throws.TypeOf<InvalidOperationException>());
	}

	[Test]
	public void Create_ExistingPackageWithForce_Succeeds()
	{
		var sourceDir = CreateSource("c.md", "# C");
		var outputDir = Path.Combine(_tempDir, "out");

		PackageCreator.Create(new PackOptions(sourceDir, outputDir, force: false));

		Assert.That(File.Exists(PackageCreator.Create(new PackOptions(sourceDir, outputDir, force: true))), Is.True);
	}

	[Test]
	public void Create_ExcludesConfigFile()
	{
		var sourceDir = CreateSource("c.md", "# C");
		File.WriteAllText(
			Path.Combine(sourceDir, ".github", PackageLayout.ConfigFileName),
			"""{"packageId":"t","version":"1.0.0","description":"d","authors":"a","contentHash":""}""");

		var packagePath = PackageCreator.Create(new PackOptions(sourceDir, Path.Combine(_tempDir, "out"), force: false));

		using var stream = File.OpenRead(packagePath);
		using var reader = new PackageArchiveReader(stream);

		Assert.That(reader.GetFiles().ToList(), Has.None.Contain(PackageLayout.ConfigFileName));
	}

	[Test]
	public void Create_PlacesContentUnderAgentContentPath()
	{
		var sourceDir = CreateSource("agents/test.agent.md", "# Agent");

		var packagePath = PackageCreator.Create(new PackOptions(sourceDir, Path.Combine(_tempDir, "out"), force: false));

		using var stream = File.OpenRead(packagePath);
		using var reader = new PackageArchiveReader(stream);

		Assert.That(reader.GetFiles().ToList(), Has.Some.Contain("agentcontent/github/agents/test.agent.md"));
	}

	[Test]
	public void Create_IncludesBuildTargets()
	{
		var sourceDir = CreateSource("c.md", "# C");

		var packagePath = PackageCreator.Create(new PackOptions(sourceDir, Path.Combine(_tempDir, "out"), force: false));

		using var stream = File.OpenRead(packagePath);
		using var reader = new PackageArchiveReader(stream);

		Assert.That(reader.GetFiles().ToList(),
			Has.Some.Matches<string>(f => f.StartsWith("build/") && f.EndsWith(".targets")));
	}

	[Test]
	public void Create_CreatesConfigFileOnFirstRun()
	{
		var sourceDir = CreateSource("c.md", "# C");
		var configPath = Path.Combine(sourceDir, ".github", PackageLayout.ConfigFileName);

		Assert.That(File.Exists(configPath), Is.False, "Precondition");

		PackageCreator.Create(new PackOptions(sourceDir, Path.Combine(_tempDir, "out"), force: false));

		Assert.That(File.Exists(configPath), Is.True);
	}

	[Test]
	public void Create_MultipleFiles_AllPackaged()
	{
		var sourceDir = Path.Combine(_tempDir, "source");
		var ghDir = Path.Combine(sourceDir, ".github");
		Directory.CreateDirectory(Path.Combine(ghDir, "agents"));
		Directory.CreateDirectory(Path.Combine(ghDir, "skills"));
		File.WriteAllText(Path.Combine(ghDir, "agents", "a1.md"), "# A1");
		File.WriteAllText(Path.Combine(ghDir, "agents", "a2.md"), "# A2");
		File.WriteAllText(Path.Combine(ghDir, "skills", "s1.md"), "# S1");

		var packagePath = PackageCreator.Create(new PackOptions(sourceDir, Path.Combine(_tempDir, "out"), force: false));

		using var stream = File.OpenRead(packagePath);
		using var reader = new PackageArchiveReader(stream);

		Assert.That(
			reader.GetFiles().Where(f => f.StartsWith("agentcontent/", StringComparison.OrdinalIgnoreCase)).ToList(),
			Has.Count.EqualTo(3));
	}

	[Test]
	public void Create_CreatesOutputDirectoryWhenMissing()
	{
		var sourceDir = CreateSource("c.md", "# C");
		var outputDir = Path.Combine(_tempDir, "deep", "nested", "out");

		Assert.That(Directory.Exists(outputDir), Is.False, "Precondition");

		Assert.That(
			File.Exists(PackageCreator.Create(new PackOptions(sourceDir, outputDir, force: false))),
			Is.True);
	}

	[Test]
	public void Create_EmptyGithubDir_NoContentFiles()
	{
		var sourceDir = Path.Combine(_tempDir, "source");
		Directory.CreateDirectory(Path.Combine(sourceDir, ".github"));

		var packagePath = PackageCreator.Create(new PackOptions(sourceDir, Path.Combine(_tempDir, "out"), force: false));

		using var stream = File.OpenRead(packagePath);
		using var reader = new PackageArchiveReader(stream);

		Assert.That(
			reader.GetFiles().Where(f => f.StartsWith("agentcontent/", StringComparison.OrdinalIgnoreCase)).ToList(),
			Is.Empty);
	}

	private string CreateSource(string relativePath, string content)
	{
		var sourceDir = Path.Combine(_tempDir, "source");
		var filePath = Path.Combine(sourceDir, ".github", relativePath);
		Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
		File.WriteAllText(filePath, content);
		return sourceDir;
	}
}