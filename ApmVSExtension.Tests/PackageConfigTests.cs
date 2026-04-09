using Newtonsoft.Json;

namespace ApmVSExtension.Tests
{
	[TestFixture]
	public class PackageConfigTests
	{
		[Test]
		public void Properties_CanBeSetAndRetrieved()
		{
			var config = new PackageConfig
			{
				PackageId = "TestId",
				Version = "1.2.3",
				Description = "Test description",
				Authors = "TestAuthor",
				ContentHash = "abc123"
			};

			Assert.Multiple(() =>
			{
				Assert.That(config.PackageId, Is.EqualTo("TestId"));
				Assert.That(config.Version, Is.EqualTo("1.2.3"));
				Assert.That(config.Description, Is.EqualTo("Test description"));
				Assert.That(config.Authors, Is.EqualTo("TestAuthor"));
				Assert.That(config.ContentHash, Is.EqualTo("abc123"));
			});
		}

		[Test]
		public void Serialization_ProducesExpectedJsonPropertyNames()
		{
			var config = new PackageConfig
			{
				PackageId = "MyPkg",
				Version = "1.0.0",
				Description = "Desc",
				Authors = "Auth",
				ContentHash = "hash"
			};

			var json = JsonConvert.SerializeObject(config);

			Assert.Multiple(() =>
			{
				Assert.That(json, Does.Contain("\"packageId\""));
				Assert.That(json, Does.Contain("\"version\""));
				Assert.That(json, Does.Contain("\"description\""));
				Assert.That(json, Does.Contain("\"authors\""));
				Assert.That(json, Does.Contain("\"contentHash\""));
			});
		}

		[Test]
		public void Serialization_DoesNotUsePascalCasePropertyNames()
		{
			var config = new PackageConfig
			{
				PackageId = "MyPkg",
				Version = "1.0.0",
				Description = "Desc",
				Authors = "Auth",
				ContentHash = "hash"
			};

			var json = JsonConvert.SerializeObject(config);

			Assert.Multiple(() =>
			{
				Assert.That(json, Does.Not.Contain("\"PackageId\""));
				Assert.That(json, Does.Not.Contain("\"Version\""));
				Assert.That(json, Does.Not.Contain("\"Description\""));
				Assert.That(json, Does.Not.Contain("\"Authors\""));
				Assert.That(json, Does.Not.Contain("\"ContentHash\""));
			});
		}

		[Test]
		public void Deserialization_RestoresAllProperties()
		{
			var json = """
				{
					"packageId": "RestoredPkg",
					"version": "3.2.1",
					"description": "Restored desc",
					"authors": "RestoredAuth",
					"contentHash": "restoredHash"
				}
				""";

			var config = JsonConvert.DeserializeObject<PackageConfig>(json);

			Assert.Multiple(() =>
			{
				Assert.That(config!.PackageId, Is.EqualTo("RestoredPkg"));
				Assert.That(config.Version, Is.EqualTo("3.2.1"));
				Assert.That(config.Description, Is.EqualTo("Restored desc"));
				Assert.That(config.Authors, Is.EqualTo("RestoredAuth"));
				Assert.That(config.ContentHash, Is.EqualTo("restoredHash"));
			});
		}

		[Test]
		public void Deserialization_MissingFields_DefaultsToNull()
		{
			var json = "{}";

			var config = JsonConvert.DeserializeObject<PackageConfig>(json);

			Assert.Multiple(() =>
			{
				Assert.That(config!.PackageId, Is.Null);
				Assert.That(config.Version, Is.Null);
				Assert.That(config.Description, Is.Null);
				Assert.That(config.Authors, Is.Null);
				Assert.That(config.ContentHash, Is.Null);
			});
		}

		[Test]
		public void RoundTrip_SerializeDeserialize_PreservesValues()
		{
			var original = new PackageConfig
			{
				PackageId = "RoundTrip",
				Version = "5.0.0",
				Description = "Round trip test",
				Authors = "Tester",
				ContentHash = "rtHash"
			};

			var json = JsonConvert.SerializeObject(original, Formatting.Indented);
			var restored = JsonConvert.DeserializeObject<PackageConfig>(json);

			Assert.Multiple(() =>
			{
				Assert.That(restored!.PackageId, Is.EqualTo(original.PackageId));
				Assert.That(restored.Version, Is.EqualTo(original.Version));
				Assert.That(restored.Description, Is.EqualTo(original.Description));
				Assert.That(restored.Authors, Is.EqualTo(original.Authors));
				Assert.That(restored.ContentHash, Is.EqualTo(original.ContentHash));
			});
		}
	}
}
