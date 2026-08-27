using Scrbl.JaccoBikker.Bvh;

namespace Scrbl.JaccoBikker
{
    internal class Program
    {
        static void Main(string[] args)
        {
            const string ImageSavePath = @"C:\Steven\Atelier\Scrbl\Scrbl.JaccoBikker\.output";

            const int TriangleCount = 512;

            //new HowToBuildABvh_Part01_Basics_Step01().Run(
            //    new HowToBuildABvh_Part01_Basics_Step01.RayTraceSettings 
            //    { 
            //        TriangleCount = TriangleCount,
            //        ImageSavePath = System.IO.Path.Combine(ImageSavePath, "HowToBuildABvh_Part01_Basics_Step01.png") 
            //    });

            new HowToBuildABvh_Part01_Basics_Step02().Run(
                new HowToBuildABvh_Part01_Basics_Step01.RayTraceSettings
                {
                    TriangleCount = TriangleCount,
                    ImageSavePath = System.IO.Path.Combine(ImageSavePath, "HowToBuildABvh_Part01_Basics_Step02.png")
                });

            new HowToBuildABvh_Part01_Basics_Step03_Struct_BvhNode().Run(
                new HowToBuildABvh_Part01_Basics_Step01.RayTraceSettings
                {
                    TriangleCount = TriangleCount,
                    ImageSavePath = System.IO.Path.Combine(ImageSavePath, "HowToBuildABvh_Part01_Basics_Step03_Struct_BvhNode.png")
                });

            new HowToBuildABvh_Part01_Basics_Step04_Struct_BvhNode_Vector3f().Run(
                new HowToBuildABvh_Part01_Basics_Step01.RayTraceSettings
                {
                    TriangleCount = TriangleCount,
                    ImageSavePath = System.IO.Path.Combine(ImageSavePath, "HowToBuildABvh_Part01_Basics_Step04_Struct_BvhNode_Vector3f.png")
                });

            Console.WriteLine($"Press a key to exit...");

            Console.ReadKey();
        }
    }
}
