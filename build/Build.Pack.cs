using System.Text.RegularExpressions;
using Nuke.Common;
using Nuke.Common.Tools.DotNet;
using Serilog;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

namespace Build;

internal partial class Build
{
	private Target Pack => target => target
		.Executes(() =>
		{
			foreach (var configuration in Configurations)
			{
				if (string.IsNullOrEmpty(configuration)) throw new Exception("Configuration is not specified");

				Log.Information("Crating package {Configuration} of {Solution} solution", configuration, Solution);

				DotNetTasks.DotNetRestore(settings => settings
					.SetProjectFile(Solution.Path)
					.SetProperty("Configuration", configuration));
				
				DotNetPack(settings => settings
					.SetProject(Solution.Eneca_Revit_Toolkit)
					.SetConfiguration(configuration)
					.SetDeterministic(false)
					.SetVersion(ReadVersionFromConfiguration(configuration))
					.SetOutputDirectory(ArtifactsDirectory)
					.SetNoBuild(true)
					.SetVerbosity(DotNetVerbosity.quiet));
			}
		})
		.Triggers(SendToNexus);

	private string ReadVersionFromConfiguration(string configuration)
	{
		var year = ParseRevitYear(configuration);
		var assemblyVersion = GetAssemblyVersion();
		var version = year == 0 
			? assemblyVersion.ToString() 
			: $"{year}.{assemblyVersion.Major}.{assemblyVersion.Minor}.{assemblyVersion.Build}";

		Log.Information("Parsed version {Version} from configuration '{Configuration}'", version, configuration);

		return version;
	}

	private int ParseRevitYear(string configurationName)
	{
		var match = Regex.Match(configurationName, @"R(\d+)");
		if (!match.Success) return 0;

		var versionNumber = int.Parse(match.Groups[1].Value);
		var year = 2000 + versionNumber;

		Log.Information("Parsed Revit year {Year} from configuration '{Configuration}'", year, configurationName);
		return year;
	}
}