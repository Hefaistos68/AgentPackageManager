namespace ApmVSExtension.Tests
{
	[TestFixture]
	public class VsixTests
	{
		[Test]
		public void Id_IsNotNullOrEmpty()
		{
			Assert.That(Vsix.Id, Is.Not.Null.And.Not.Empty);
		}

		[Test]
		public void Name_IsNotNullOrEmpty()
		{
			Assert.That(Vsix.Name, Is.Not.Null.And.Not.Empty);
		}

		[Test]
		public void Version_IsNotNullOrEmpty()
		{
			Assert.That(Vsix.Version, Is.Not.Null.And.Not.Empty);
		}

		[Test]
		public void Author_IsNotNullOrEmpty()
		{
			Assert.That(Vsix.Author, Is.Not.Null.And.Not.Empty);
		}

		[Test]
		public void Id_ContainsExtensionName()
		{
			Assert.That(Vsix.Id, Does.Contain(Vsix.Name));
		}
	}
}
