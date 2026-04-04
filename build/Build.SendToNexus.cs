using JetBrains.Annotations;
using Nuke.Common;
using Nuke.Common.Tools.DotNet;
using Serilog;

namespace Build;

internal partial class Build
{
	[UsedImplicitly]
	private Target SendToNexus => d => d
		.OnlyWhenStatic(() => ForceNexus || IsServerBuild)
		.Executes(() =>
		{
			Log.Information("Sending Nuget Packages to Nexus process started");

			IEnumerable<string> nugetPackageFiles =
				Directory.GetFiles(ArtifactsDirectory).Where(file => file.Contains(ProjectName));

			foreach (var nugetPackageFile in nugetPackageFiles)
				DotNetTasks.DotNetNuGetPush(s => s
					.SetApiKey(NexusNugetCredentials)
					.SetTargetPath(nugetPackageFile)
					.SetSource(NexusNugetUrl));
		});
}