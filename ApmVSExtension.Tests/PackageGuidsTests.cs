namespace ApmVSExtension.Tests
{
	[TestFixture]
	public class PackageGuidsTests
	{
		[Test]
		public void ApmVSExtensionString_IsValidGuidFormat()
		{
			Assert.That(Guid.TryParse(PackageGuids.ApmVSExtensionString, out _), Is.True);
		}

		[Test]
		public void ApmVSExtension_MatchesStringConstant()
		{
			Assert.That(PackageGuids.ApmVSExtension, Is.EqualTo(new Guid(PackageGuids.ApmVSExtensionString)));
		}

		[Test]
		public void ApmVSExtension_IsNotEmptyGuid()
		{
			Assert.That(PackageGuids.ApmVSExtension, Is.Not.EqualTo(Guid.Empty));
		}

		[Test]
		public void ApmVSExtensionString_IsLowercase()
		{
			Assert.That(PackageGuids.ApmVSExtensionString, Is.EqualTo(PackageGuids.ApmVSExtensionString.ToLowerInvariant()));
		}
	}
}
