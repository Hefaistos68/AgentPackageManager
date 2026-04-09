using System.Globalization;

namespace ApmPackager
{
	/// <summary>
	/// Parses command-line arguments and dispatches package operations.
	/// </summary>
	internal static class CommandLine
	{
		/// <summary>
		/// Executes the command-line entry point.
		/// </summary>
		/// <param name="args">The command-line arguments.</param>
		/// <returns>The process exit code.</returns>
		public static int Execute(string[] args)
		{
			if (args.Length == 0 || IsHelpToken(args[0]))
			{
				PrintHelp();
				return 0;
			}

			return args[0].ToLowerInvariant() switch
			{
				"pack" => ExecutePack(args[1..]),
				"materialize" => ExecuteMaterialize(args[1..]),
				_ => Fail($"Unknown command '{args[0]}'.")
			};
		}

		/// <summary>
		/// Executes the <c>pack</c> command.
		/// </summary>
		/// <param name="args">The command arguments.</param>
		/// <returns>The process exit code.</returns>
		private static int ExecutePack(string[] args)
		{
			var options = ParseOptions(args);
			if (options.TryGetValue("help", out _))
			{
				PrintPackHelp();
				return 0;
			}

			var sourceDirectory = RequireValue(options, "source");
			var outputDirectory = GetValue(options, "output") ?? Directory.GetCurrentDirectory();
			var force = options.ContainsKey("force");

			var packOptions = new PackOptions(
				sourceDirectory,
				outputDirectory,
				force);

			var packagePath = PackageCreator.Create(packOptions);
			Console.WriteLine($"[+] Created package {packagePath}");
			return 0;
		}

		/// <summary>
		/// Executes the <c>materialize</c> command.
		/// </summary>
		/// <param name="args">The command arguments.</param>
		/// <returns>The process exit code.</returns>
		private static int ExecuteMaterialize(string[] args)
		{
			var options = ParseOptions(args);
			if (options.TryGetValue("help", out _))
			{
				PrintMaterializeHelp();
				return 0;
			}

			var packagePath = RequireValue(options, "package");
			var projectDirectory = RequireValue(options, "project");
			var force = options.ContainsKey("force");

			var materializeOptions = new MaterializeOptions(packagePath, projectDirectory, force);
			PackageMaterializer.Materialize(materializeOptions);
			Console.WriteLine($"[+] Materialized .github into {Path.GetFullPath(projectDirectory, Directory.GetCurrentDirectory())}");
			return 0;
		}

		/// <summary>
		/// Parses command-line options into a key-value lookup.
		/// </summary>
		/// <param name="args">The arguments to parse.</param>
		/// <returns>The parsed options.</returns>
		private static Dictionary<string, string?> ParseOptions(string[] args)
		{
			var options = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

			var index = 0;
			while (index < args.Length)
			{
				var argument = args[index];
				if (!argument.StartsWith("--", StringComparison.Ordinal))
				{
					throw new InvalidOperationException($"Unexpected argument '{argument}'.");
				}

				var optionName = argument[2..];
				if (string.IsNullOrWhiteSpace(optionName))
				{
					throw new InvalidOperationException("Invalid empty option name.");
				}

				if (index + 1 < args.Length && !args[index + 1].StartsWith("--", StringComparison.Ordinal))
				{
					options[optionName] = args[index + 1];
					index += 2;
					continue;
				}

				options[optionName] = null;
				index++;
			}

			return options;
		}

		/// <summary>
		/// Gets a required option value.
		/// </summary>
		/// <param name="options">The parsed options.</param>
		/// <param name="key">The option name.</param>
		/// <returns>The option value.</returns>
		private static string RequireValue(IReadOnlyDictionary<string, string?> options, string key)
		{
			var value = GetValue(options, key);
			if (!string.IsNullOrWhiteSpace(value))
			{
				return value;
			}

			throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Missing required option --{0}.", key));
		}

		/// <summary>
		/// Gets an optional option value.
		/// </summary>
		/// <param name="options">The parsed options.</param>
		/// <param name="key">The option name.</param>
		/// <returns>The option value when present; otherwise, <see langword="null"/>.</returns>
		private static string? GetValue(IReadOnlyDictionary<string, string?> options, string key)
		{
			return options.TryGetValue(key, out var value) ? value : null;
		}

		/// <summary>
		/// Writes an error message and usage hint.
		/// </summary>
		/// <param name="message">The error message.</param>
		/// <returns>The failure exit code.</returns>
		private static int Fail(string message)
		{
			Console.Error.WriteLine($"[x] {message}");
			Console.Error.WriteLine("[i] Run with --help for usage.");
			return 1;
		}

		/// <summary>
		/// Determines whether a token requests help output.
		/// </summary>
		/// <param name="token">The token to evaluate.</param>
		/// <returns><see langword="true"/> when the token is a help token; otherwise, <see langword="false"/>.</returns>
		private static bool IsHelpToken(string token)
		{
			return string.Equals(token, "--help", StringComparison.OrdinalIgnoreCase)
				|| string.Equals(token, "-h", StringComparison.OrdinalIgnoreCase)
				|| string.Equals(token, "help", StringComparison.OrdinalIgnoreCase);
		}

		/// <summary>
		/// Prints the top-level command help.
		/// </summary>
		private static void PrintHelp()
		{
			Console.WriteLine("ApmPackager");
			Console.WriteLine("Builds NuGet packages from a .github folder and materializes package payloads.");
			Console.WriteLine();
			Console.WriteLine("Commands:");
			Console.WriteLine("  pack         Create a .nupkg from a source .github folder");
			Console.WriteLine("  materialize  Copy packaged .github content into a project");
			Console.WriteLine();
			Console.WriteLine("Run 'ApmPackager <command> --help' for command options.");
		}

		/// <summary>
		/// Prints help for the <c>pack</c> command.
		/// </summary>
		private static void PrintPackHelp()
		{
			Console.WriteLine("Usage:");
			Console.WriteLine("  ApmPackager pack --source <dir> [--output <dir>] [--force]");
			Console.WriteLine();
			Console.WriteLine("Package metadata is read from .github/agent-package.json.");
			Console.WriteLine("If the file does not exist, it is created with default values.");
		}

		/// <summary>
		/// Prints help for the <c>materialize</c> command.
		/// </summary>
		private static void PrintMaterializeHelp()
		{
			Console.WriteLine("Usage:");
			Console.WriteLine("  ApmPackager materialize --package <path> --project <dir> [--force]");
			Console.WriteLine();
			Console.WriteLine("The package path can point to a .nupkg file or an extracted package directory.");
		}
	}
}
