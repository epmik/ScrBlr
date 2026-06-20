namespace Scrblr.Rtx
{
    using System;

    class SimpleWorldWithCameraProgram
    { 

        public void Main()
        {
            // World
            HittableList world = new HittableList();

            world.Add(new SphereNoMaterial(new Vector3d(0, 0, -1), 0.5));
            world.Add(new SphereNoMaterial(new Vector3d(0, -100.5, -1), 100));

            var cam = new CameraSingleSample();

            cam.aspectRatio = 16.0 / 9.0;
            cam.imageWidth = 400;

            cam.Render(world, ".output/SimpleWorldWithCameraProgram.ppm");
        }
    }
}
