using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace RefSwitch
{
	class Program
	{
		static int Main(string[] args)
		{
			try
			{
				if (args.Length != 1 && args.Length != 2)
				{
					Console.WriteLine(Messages.Usage);
					return 1;
				}

				var type = args.ElementAt(0).ToLower();
				var package = type == "package";
				var project = type == "project";

				if (!package && !project)
				{
					Console.WriteLine();
				}

				var target = args.ElementAtOrDefault(1) ?? Environment.CurrentDirectory;
				var files = new string[] { };
				if (File.Exists(target))
				{
					if (Path.GetExtension(target)?.ToLower() != ".csproj")
					{
						Console.WriteLine(Messages.InvalidTargetFileType);
						return 1;
					}

					files = new[] { target };
				}
				else if (Directory.Exists(target))
				{
					files = Directory.GetFiles(target, "*.csproj", SearchOption.AllDirectories);
				}
				else
				{
					Console.WriteLine(Messages.TargetNotFound);
					return 1;
				}

				foreach (var file in files)
				{
					var modified = false;
					var lines = File.ReadLines(file).ToList();
					if (package)
					{
						Enable("PackageReferences", lines, ref modified);
						Disable("ProjectReferences", lines, ref modified);
					}
					else
					{
						Enable("ProjectReferences", lines, ref modified);
						Disable("PackageReferences", lines, ref modified);
					}

					if (modified)
					{
						File.WriteAllLines(file, lines);
					}
				}

				return 0;
			}
			catch (Exception exception)
			{
				var color = Console.ForegroundColor;
				Console.ForegroundColor = ConsoleColor.Red;
				Console.Error.WriteLine(exception.Message);
				Console.ForegroundColor = color;
				return -1;
			}
		}

		private static void Disable(string type, IList<string> lines, ref bool modified)
		{
			var startTag = $"<!-- {type} BEGIN -->";
			var endTag = $"<!-- {type} END -->";

			var process = false;

			for (var i = 0; i < lines.Count; i++)
			{
				var line = lines[i];

				if (string.IsNullOrWhiteSpace(line))
				{
					continue;
				}

				if (Regex.IsMatch(line, startTag))
				{
					process = true;
					continue;
				}

				if (Regex.IsMatch(line, endTag))
				{
					process = false;
					continue;
				}

				if (process)
				{
					var whitespaces = GetLeadingWhitespaces(line);
					var content = line.Trim();

					if (!content.StartsWith("<!--"))
					{
						content = "<!-- " + content;
					}

					if (!content.EndsWith("-->"))
					{
						content += " -->";
					}

					modified = true;
					lines[i] = whitespaces + content;
				}
			}
		}

		private static void Enable(string type, IList<string> lines, ref bool modified)
		{
			var startTag = $"<!-- {type} BEGIN -->";
			var endTag = $"<!-- {type} END -->";

			var process = false;

			for (var i = 0; i < lines.Count; i++)
			{
				var line = lines[i];

				if (Regex.IsMatch(line, startTag))
				{
					process = true;
					continue;
				}

				if (Regex.IsMatch(line, endTag))
				{
					process = false;
					continue;
				}

				if (process)
				{
					var whitespaces = GetLeadingWhitespaces(line);
					var content = line.Trim();

					if (content.StartsWith("<!--"))
					{
						content = content.Substring(4).TrimStart();
					}

					if (content.EndsWith("-->"))
					{
						content = content.Substring(0, content.Length - 3).TrimEnd();
					}

					modified = true;
					lines[i] = whitespaces + content;
				}
			}
		}

		private static string GetLeadingWhitespaces(string s)
		{
			var index = 0;
			for (; index < s.Length && char.IsWhiteSpace(s[index]); index++) { }
			return s.Substring(0, index);
		}

		static class Messages
		{
			public static string TargetNotFound => "Target not found.";
			public static string InvalidTargetFileType => "Target file must be a *.csproj.";
			public static string MissingReferenceType => "Specify reference type (package / project).";
			public static string UnknownReferenceType(string s) => $"Unknown reference type '{s}'.";
			public static string Usage => "refswitch.exe <ref-type> <target>";
		}
	}
}
