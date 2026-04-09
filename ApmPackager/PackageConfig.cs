using System.Text.Json.Serialization;

namespace ApmPackager
{
   /// <summary>
    /// Represents the persisted package metadata stored in <c>.github/agent-package.json</c>.
    /// </summary>
    internal sealed record PackageConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PackageConfig"/> record.
        /// </summary>
        /// <param name="packageId">The package identifier.</param>
        /// <param name="version">The package version.</param>
        /// <param name="description">The package description.</param>
        /// <param name="authors">The package authors.</param>
        /// <param name="contentHash">The hash of the packaged <c>.github</c> content.</param>
        public PackageConfig(string packageId, string version, string description, string authors, string contentHash)
        {
            PackageId = packageId;
            Version = version;
            Description = description;
            Authors = authors;
            ContentHash = contentHash;
        }

        /// <summary>
        /// Gets the package identifier.
        /// </summary>
        [JsonPropertyName("packageId")]
        public string PackageId { get; }

        /// <summary>
        /// Gets the package version.
        /// </summary>
        [JsonPropertyName("version")]
        public string Version { get; }

        /// <summary>
        /// Gets the package description.
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; }

        /// <summary>
        /// Gets the package authors.
        /// </summary>
        [JsonPropertyName("authors")]
        public string Authors { get; }

        /// <summary>
        /// Gets the hash of the packaged <c>.github</c> content.
        /// </summary>
        [JsonPropertyName("contentHash")]
        public string ContentHash { get; }

        /// <summary>
        /// Creates a default configuration for a package source directory.
        /// </summary>
        /// <param name="defaultPackageId">The default package identifier to use when no configuration exists.</param>
        /// <returns>A new <see cref="PackageConfig"/> instance populated with default values.</returns>
        public static PackageConfig CreateDefault(string defaultPackageId)
        {
            if (string.IsNullOrWhiteSpace(defaultPackageId))
            {
                throw new ArgumentException("Default package id must be provided.", nameof(defaultPackageId));
            }

            return new PackageConfig(
                defaultPackageId,
                "1.0.0",
                "GitHub Copilot assets package",
                Environment.UserName,
                string.Empty);
        }
    }
}
