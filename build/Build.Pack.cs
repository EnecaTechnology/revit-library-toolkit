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

				DotNetPack(settings => settings
					.SetProject(Solution.Eneca_Revit_Toolkit)
					.SetConfiguration(configuration)
					.SetDeterministic(false)
					.SetOutputDirectory(ArtifactsDirectory)
					.SetNoBuild(true)
					.SetVerbosity(DotNetVerbosity.quiet));
			}
		}).Triggers(SendToNexus);
}