using Autodesk.Revit.DB;

namespace Eneca.Revit.Toolkit.Shared;

public static class VersionPortExtensions
{
    public static ElementId ToElementId(this long value)
    {
#if R24 || R25
		return new ElementId(value);
#else
        var id = Convert.ToInt32(value);
        return new ElementId(id);
#endif
    }

    public static long Retrieve(this ElementId elementId)
    {
#if R24 || R25
		return elementId.Value;
#else
        return elementId.IntegerValue;
#endif
    }
}