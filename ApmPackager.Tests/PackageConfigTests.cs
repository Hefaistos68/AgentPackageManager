using ApmPackager;
using NUnit.Framework;

namespace ApmPackager.Tests;

[TestFixture]
public class PackageConfigTests
{
	[Test]
	public void Constructor_SetsAllProperties()
	{
		var config = new PackageConfig("MyPkg", "2.0.0", "Desc", "Auth", "hash");

		Assert.Multiple(() =>
		{
			Assert.That(config.PackageId, Is.EqualTo("MyPkg"));
			Assert.That(config.Version, Is.EqualTo("2.0.0"));
			Assert.That(config.Description, Is.EqualTo("Desc"));
			Assert.That(config.Authors, Is.EqualTo("Auth"));
			Assert.That(config.ContentHash, Is.EqualTo("hash"));
		});
	}

	[Test]
	public void CreateDefault_ReturnsExpectedDefaults()
	{
		var config = PackageConfig.CreateDefault("TestProj");

		Assert.Multiple(() =>
		{
			Assert.That(config.PackageId, Is.EqualTo("TestProj"));
			Assert.That(config.Version, Is.EqualTo("1.0.0"));
			Assert.That(config.Description, Is.EqualTo("GitHub Copilot assets package"));
			Assert.That(config.Authors, Is.Not.Null.And.Not.Empty);
			Assert.That(config.ContentHash, Is.EqualTo(string.Empty));
		});
	}

	[TestCase(null)]
	[TestCase("")]
	[TestCase("   ")]
	public void CreateDefault_InvalidId_Throws(string? id)
	{
		Assert.That(() => PackageConfig.CreateDefault(id!), Throws.TypeOf<ArgumentException>());
	}

	[Test]
	public void RecordEquality_SameValues_AreEqual()
	{
		var a = new PackageConfig("Id", "1.0.0", "D", "A", "h");
		var b = new PackageConfig("Id", "1.0.0", "D", "A", "h");

		Assert.That(a, Is.EqualTo(b));
	}

	[Test]
	public void RecordEquality_DifferentHash_NotEqual()
	{
		var a = new PackageConfig("Id", "1.0.0", "D", "A", "h1");
		var b = new PackageConfig("Id", "1.0.0", "D", "A", "h2");

		Assert.That(a, Is.Not.EqualTo(b));
	}

	[Test]
	public void RecordEquality_DifferentPackageId_NotEqual()
	{
		var a = new PackageConfig("Id1", "1.0.0", "D", "A", "h");
		var b = new PackageConfig("Id2", "1.0.0", "D", "A", "h");

		Assert.That(a, Is.Not.EqualTo(b));
	}

	[Test]
	public void RecordEquality_DifferentVersion_NotEqual()
	{
		var a = new PackageConfig("Id", "1.0.0", "D", "A", "h");
		var b = new PackageConfig("Id", "2.0.0", "D", "A", "h");

		Assert.That(a, Is.Not.EqualTo(b));
	}
}