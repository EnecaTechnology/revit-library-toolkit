using Nuke.Common;

namespace Build;

internal partial class Build : NukeBuild
{
	public static int Main()
	{
		return Execute<Build>(x => x.Clean);
	}
}