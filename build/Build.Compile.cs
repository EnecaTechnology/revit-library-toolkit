using System.Reflection;
using Nuke.Common;
using Nuke.Common.Tools.DotNet;
using Serilog;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

namespace Build;

internal partial class Build
{
	private Target Compile => d => d
		.Triggers(Pack)
		.Executes(() =>
		{
			foreach (var configuration in Configurations)
			{
				if (string.IsNullOrEmpty(configuration)) throw new Exception("Configuration is not specified");

				Log.Information("Compiling {Configuration} of {Solution} solution", configuration, Solution);
				DotNetBuild(settings => settings
					.SetConfiguration(configuration)
					.SetDeterministic(false)
					.SetProjectFile(Solution.Path)
					.SetVerbosity(DotNetVerbosity.quiet));
			}

			if (!IsServerBuild) return;
			
			ValidateVersion();
		});

	private void ValidateVersion()
	{
		if (!Version.TryParse(ReleaseTag, out var tagVersion))
			throw new Exception($"Invalid version in tag: {ReleaseTag}");

		Log.Information("Release tag: {Version}", tagVersion);

		var productVersion = GetAssemblyVersion();;
			
		Log.Information("Product Version: {Version}", tagVersion);
			
		var majorEquals = productVersion!.Major == tagVersion.Major;
		var minorEquals = productVersion.Minor == tagVersion.Minor;
		var buildEquals = productVersion.Build == tagVersion.Build;
		if (!(majorEquals && minorEquals && buildEquals))
			throw new Exception("Tag and Product version should be the same");
	}

	private Version GetAssemblyVersion()
	{
		var assembly = Directory
			.GetFiles(ProjectDirectory, "*.dll", SearchOption.AllDirectories)
			.First(x => x.Contains($"{ProjectName}") && x.EndsWith(".dll"));
		
		 return Assembly.LoadFile(assembly).GetName().Version;
	}
}