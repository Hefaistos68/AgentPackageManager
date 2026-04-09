namespace ApmVSExtension.Tests
{
	[TestFixture]
	public class PackageIdsTests
	{
		[Test]
		public void SolutionNodeMenuGroup_HasExpectedValue()
		{
			Assert.That(PackageIds.SolutionNodeMenuGroup, Is.EqualTo(0x0001));
		}

		[Test]
		public void ProjectNodeMenuGroup_HasExpectedValue()
		{
			Assert.That(PackageIds.ProjectNodeMenuGroup, Is.EqualTo(0x0002));
		}

		[Test]
		public void CreateAgentPackageCommand_HasExpectedValue()
		{
			Assert.That(PackageIds.CreateAgentPackageCommand, Is.EqualTo(0x0100));
		}

		[Test]
		public void CreateProjectAgentPackageCommand_HasExpectedValue()
		{
			Assert.That(PackageIds.CreateProjectAgentPackageCommand, Is.EqualTo(0x0101));
		}

		[Test]
		public void CommandIds_AreUnique()
		{
			var ids = new[]
			{
				PackageIds.SolutionNodeMenuGroup,
				PackageIds.ProjectNodeMenuGroup,
				PackageIds.CreateAgentPackageCommand,
				PackageIds.CreateProjectAgentPackageCommand
			};

			Assert.That(ids, Is.Unique);
		}
	}
}
