using Autodesk.Revit.DB;

namespace Eneca.Revit.Toolkit.Shared;

internal static class VersionPortExtensions
{
    internal static ElementId ToElementId(this long value)
    {
#if R24 || R25
		return new ElementId(value);
#else
        var id = Convert.ToInt32(value);
        return new ElementId(id);
#endif
    }

    internal static long Retrieve(this ElementId elementId)
    {
#if R24 || R25
		return elementId.Value;
#else
        return elementId.IntegerValue;
#endif
    }
}