using Newtonsoft.Json;

namespace ApmVSExtension
{
	/// <summary>
	/// Represents the package metadata persisted in <c>.github\agent-package.json</c>.
	/// </summary>
	internal sealed class PackageConfig
	{
		/// <summary>
		/// Gets or sets the package identifier.
		/// </summary>
		[JsonProperty("packageId")]
		public string PackageId { get; set; }

		/// <summary>
		/// Gets or sets the package version.
		/// </summary>
		[JsonProperty("version")]
		public string Version { get; set; }

		/// <summary>
		/// Gets or sets the package description.
		/// </summary>
		[JsonProperty("description")]
		public string Description { get; set; }

		/// <summary>
		/// Gets or sets the package authors.
		/// </summary>
		[JsonProperty("authors")]
		public string Authors { get; set; }

		/// <summary>
		/// Gets or sets the hash of the packaged <c>.github</c> content.
		/// </summary>
		[JsonProperty("contentHash")]
		public string ContentHash { get; set; }
	}
}
