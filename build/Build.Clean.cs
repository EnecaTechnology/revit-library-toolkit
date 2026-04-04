using Nuke.Common;
using Nuke.Common.IO;
using Serilog;

namespace Build;

internal partial class Build
{
	private Target Clean => d => d
		.OnlyWhenStatic(() => IsLocalBuild)
		.WhenSkipped(DependencyBehavior.Execute)
		.Executes(() =>
		{
			CleanDirectory(ArtifactsDirectory);
			foreach (var project in Solution.AllProjects.Where(p => p.Name != "Build").ToList())
			{
				CleanDirectory(project.Directory / "bin");
				CleanDirectory(project.Directory / "obj");
			}
		})
		.Triggers(Compile);

	private static void CleanDirectory(AbsolutePath path)
	{
		Log.Information("Cleaning directory: {Directory}", path);
		path.CreateOrCleanDirectory();
	}
}