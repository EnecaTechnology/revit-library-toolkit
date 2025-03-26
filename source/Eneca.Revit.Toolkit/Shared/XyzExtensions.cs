namespace Eneca.Revit.Toolkit.Shared;

public static class XyzExtensions
{
    public static XYZ TransformPoint(this XYZ point, Transform transform)
    {
        var x = point.X;
        var y = point.Y;
        var z = point.Z;

        //transform basis of the old coordinate system in the new coordinate // system
        var b0 = transform.get_Basis(0);
        var b1 = transform.get_Basis(1);
        var b2 = transform.get_Basis(2);
        var origin = transform.Origin;

        //transform the origin of the old coordinate system in the new 
        //coordinate system
        var xTemp = x * b0.X + y * b1.X + z * b2.X + origin.X;
        var yTemp = x * b0.Y + y * b1.Y + z * b2.Y + origin.Y;
        var zTemp = x * b0.Z + y * b1.Z + z * b2.Z + origin.Z;

        return new XYZ(xTemp, yTemp, zTemp);
    }
}