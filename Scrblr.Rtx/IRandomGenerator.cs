namespace Scrblr.Rtx
{
    public interface IRandomGenerator
    {
        Color Color();
        double Double();
        double Double(double max);
        double Double(double min, double max);
        Color HemisphereVector3d(Color normal);
        int Int32();
        int Int32(int max);
        int Int32(int min, int max);
        Color InUnitDiskVector3d();
        Color UnitVector3d();
        Color Vector3d();
        Color Vector3d(double min, double max);
    }
}