using ApmPackager;
using NUnit.Framework;

namespace ApmPackager.Tests;

[TestFixture]
public class PackageMaterializerTests
{
    private string _tempDir = null!;

    [SetUp]
    public void SetUp()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "ApmMaterializerTests_" + Guid.NewGuid().ToString("N"));
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
    public void Materialize_FromDirectory_CopiesAllFiles()
    {
        var packageDir = CreateExtractedPackage(new Dictionary<string, string>
        {
            ["agents/agent1.md"] = "# Agent 1",
            ["skills/skill1.md"] = "# Skill 1"
        });
        var projectDir = Path.Combine(_tempDir, "project");

        PackageMaterializer.Materialize(new MaterializeOptions(packageDir, projectDir, force: false));

        Assert.Multiple(() =>
        {
            Assert.That(File.Exists(Path.Combine(projectDir, ".github", "agents", "agent1.md")), Is.True);
            Assert.That(File.Exists(Path.Combine(projectDir, ".github", "skills", "skill1.md")), Is.True);
        });
    }

    [Test]
    public void Materialize_FromDirectory_PreservesFileContent()
    {
        var packageDir = CreateExtractedPackage(new Dictionary<string, string>
        {
            ["test.md"] = "# Expected Content"
        });
        var projectDir = Path.Combine(_tempDir, "project");

        PackageMaterializer.Materialize(new MaterializeOptions(packageDir, projectDir, force: false));

        Assert.That(
            File.ReadAllText(Path.Combine(projectDir, ".github", "test.md")),
            Is.EqualTo("# Expected Content"));
    }

    [Test]
    public void Materialize_FromDirectory_CreatesProjectDirectoryWhenMissing()
    {
        var packageDir = CreateExtractedPackage(new Dictionary<string, string>
        {
            ["test.md"] = "# Content"
        });
        var projectDir = Path.Combine(_tempDir, "nonexistent", "project");

        Assert.That(Directory.Exists(projectDir), Is.False, "Precondition");

        PackageMaterializer.Materialize(new MaterializeOptions(packageDir, projectDir, force: false));

        Assert.That(File.Exists(Path.Combine(projectDir, ".github", "test.md")), Is.True);
    }

    [Test]
    public void Materialize_ExistingFileWithoutForce_Throws()
    {
        var packageDir = CreateExtractedPackage(new Dictionary<string, string>
        {
            ["test.md"] = "# Content"
        });
        var projectDir = Path.Combine(_tempDir, "project");
        var destPath = Path.Combine(projectDir, ".github", "test.md");
        Directory.CreateDirectory(Path.GetDirectoryName(destPath)!);
        File.WriteAllText(destPath, "# Existing");

        Assert.That(
            () => PackageMaterializer.Materialize(new MaterializeOptions(packageDir, projectDir, force: false)),
            Throws.TypeOf<InvalidOperationException>());
    }

    [Test]
    public void Materialize_ExistingFileWithForce_OverwritesFile()
    {
        var packageDir = CreateExtractedPackage(new Dictionary<string, string>
        {
            ["test.md"] = "# Updated"
        });
        var projectDir = Path.Combine(_tempDir, "project");
        var destPath = Path.Combine(projectDir, ".github", "test.md");
        Directory.CreateDirectory(Path.GetDirectoryName(destPath)!);
        File.WriteAllText(destPath, "# Old");

        PackageMaterializer.Materialize(new MaterializeOptions(packageDir, projectDir, force: true));

        Assert.That(File.ReadAllText(destPath), Is.EqualTo("# Updated"));
    }

    [Test]
    public void Materialize_FromDirectory_PreservesSubdirectoryStructure()
    {
        var packageDir = CreateExtractedPackage(new Dictionary<string, string>
        {
            ["a/b/c/deep.md"] = "# Deep"
        });
        var projectDir = Path.Combine(_tempDir, "project");

        PackageMaterializer.Materialize(new MaterializeOptions(packageDir, projectDir, force: false));

        Assert.That(File.Exists(Path.Combine(projectDir, ".github", "a", "b", "c", "deep.md")), Is.True);
    }

    [Test]
    public void Materialize_MissingPackageDirectory_Throws()
    {
        var packageDir = Path.Combine(_tempDir, "nonexistent_package");
        var projectDir = Path.Combine(_tempDir, "project");

        Assert.That(
            () => PackageMaterializer.Materialize(new MaterializeOptions(packageDir, projectDir, force: false)),
            Throws.TypeOf<InvalidOperationException>());
    }

    [Test]
    public void Materialize_FromNupkg_CopiesFiles()
    {
        var sourceDir = CreateSourceWithGithub("content.md", "# From Package");
        var nupkgPath = PackageCreator.Create(new PackOptions(sourceDir, Path.Combine(_tempDir, "packages"), force: false));

        var projectDir = Path.Combine(_tempDir, "project");

        PackageMaterializer.Materialize(new MaterializeOptions(nupkgPath, projectDir, force: false));

        Assert.That(File.Exists(Path.Combine(projectDir, ".github", "content.md")), Is.True);
        Assert.That(
            File.ReadAllText(Path.Combine(projectDir, ".github", "content.md")),
            Is.EqualTo("# From Package"));
    }

    [Test]
    public void Materialize_FromNonexistentNupkg_Throws()
    {
        var fakePath = Path.Combine(_tempDir, "fake.nupkg");
        var projectDir = Path.Combine(_tempDir, "project");

        Assert.That(
            () => PackageMaterializer.Materialize(new MaterializeOptions(fakePath, projectDir, force: false)),
            Throws.TypeOf<InvalidOperationException>());
    }

    private string CreateExtractedPackage(Dictionary<string, string> files)
    {
        var packageDir = Path.Combine(_tempDir, "extracted_package");
        var contentDir = PackageLayout.GetContentDirectory(packageDir);
        foreach (var kvp in files)
        {
            var filePath = Path.Combine(contentDir, kvp.Key);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            File.WriteAllText(filePath, kvp.Value);
        }
        return packageDir;
    }

    private string CreateSourceWithGithub(string relativePath, string content)
    {
        var sourceDir = Path.Combine(_tempDir, "source");
        var filePath = Path.Combine(sourceDir, ".github", relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        File.WriteAllText(filePath, content);
        return sourceDir;
    }
}
