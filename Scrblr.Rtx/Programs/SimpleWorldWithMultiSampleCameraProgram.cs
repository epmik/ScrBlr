namespace Scrblr.Rtx
{
    using System;

    class SimpleWorldWithMultiSampleCameraProgram
    { 

        public void Main()
        {
            // World
            HittableList world = new HittableList();

            world.Add(new SphereNoMaterial(new Vector3d(0, 0, -1), 0.5));
            world.Add(new SphereNoMaterial(new Vector3d(0, -100.5, -1), 100));

            var cam = new CameraMultiSample();

            cam.aspectRatio = 16.0 / 9.0;
            cam.imageWidth = 400;
            cam.samples_per_pixel = 100;

            cam.Render(world, ".output/SimpleWorldWithMultiSampleCameraProgram.ppm");
        }
    }
}
