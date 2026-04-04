using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Serilog;

// ReSharper disable InconsistentNaming

namespace Build;

internal partial class Build
{
	private readonly AbsolutePath ArtifactsDirectory = RootDirectory / "output" / "nuget";
	[Parameter] private readonly bool ForceNexus;
	[Parameter] [Secret] private readonly string NexusCredentials;
	[Parameter] [Secret] private readonly string NexusNugetCredentials;
	[Parameter] private readonly string NexusNugetUrl;
	[Parameter] private readonly string NexusUrl;
	[Parameter] private readonly string ReleaseTag;

	[Parameter] private readonly bool SkipTagCheck;
	[Parameter] [Secret] private readonly string Token;

	private string[] Configurations { get; } =
	{
		"Release R21",
		"Release R22",
		"Release R23",
		"Release R24",
		"Release R25",
	};


	[Solution(GenerateProjects = true)] private Solution Solution { get; set; }

	private string ProjectName => Solution.Eneca_Revit_Toolkit.Name;
	private string ProjectDirectory => Solution.Eneca_Revit_Toolkit.Directory;

	protected override void OnBuildInitialized()
	{
		Log.Information("ArtifactsDirectory: {ArtifactsDirectory}", ArtifactsDirectory);
		Log.Information("Artifact directory: {RootDirectory}", RootDirectory);
	}
}