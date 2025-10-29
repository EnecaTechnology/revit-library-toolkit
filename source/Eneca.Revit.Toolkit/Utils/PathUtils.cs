using System.IO;

namespace Eneca.Revit.Toolkit.Utils;

public static class PathUtils
{
    public static bool TryExtractModelInfoFromPath(string path, out string project, out string model)
    {
        project = null;
        model = null;
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        var parts = path.Split(['/', '\\'], StringSplitOptions.RemoveEmptyEntries);
        try
        {
            int modelIndex = parts.Length - 1;
            if (parts.Length > 0)
            {
                if (parts[0] is "srv-revit")
                {
                    project = parts[3];
                    model = parts[modelIndex];
                    return true;
                }
                if (parts[0] is "R:" or "RSN:")
                {
                    project = parts[2];
                    model = parts[modelIndex];
                    return true;
                }
            }
        }
        catch (Exception)
        {
            //just ignore because we should not break execution of program, this is a helper method
        }

        return false;
    }
}