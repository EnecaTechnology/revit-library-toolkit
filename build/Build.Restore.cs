using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tools.DotNet;
using Serilog;

namespace Build;

internal partial class Build
{
	private Target Restore => d => d
		.Triggers(Compile)
		.Executes(() =>
	{
		foreach (var configuration in Configurations)
		{
			DotNetTasks.DotNetRestore(settings => settings
				.SetProjectFile(Solution.Path)
				.SetProperty("Configuration", configuration));
		}
	});
}