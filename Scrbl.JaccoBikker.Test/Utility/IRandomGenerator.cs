namespace Scrbl.JaccoBikker
{
    public interface IRandomGenerator
    {
        Color3f Color3f();
        Color3d Color3d();
        double Double();
        double Double(double max);
        double Double(double min, double max);
        Vector3d HemisphereVector3d(Vector3d normal);
        int Int32();
        int Int32(int max);
        int Int32(int min, int max);
        Vector3d InUnitDiskVector3d();
        Vector3d UnitVector3d();
        Vector3d Vector3d();
        Vector3d Vector3d(double min, double max);
        Vector3f Vector3f();
        Vector3f Vector3f(float min, float max);
        Vector3f Vector3f(double min, double max);
    }
}