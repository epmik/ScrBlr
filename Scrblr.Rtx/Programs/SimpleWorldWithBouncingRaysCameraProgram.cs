namespace Scrblr.Rtx
{
    using System;

    class SimpleWorldWithBouncingRaysCameraProgram
    { 

        public void Main()
        {
            // World
            HittableList world = new HittableList();

            world.Add(new SphereNoMaterial(new Vector3d(0, 0, -1), 0.5));
            world.Add(new SphereNoMaterial(new Vector3d(0, -100.5, -1), 100));

            var cam = new CameraBouncingRays();

            cam.aspectRatio = 16.0 / 9.0;
            cam.imageWidth = 400;
            cam.samples_per_pixel = 100;
            cam.max_depth = 50;
            cam.UseLambertianReflection = false;

            cam.Render(world, ".output/SimpleWorldWithBouncingRaysCameraProgram.ppm");

            cam.UseLambertianReflection = true;
            cam.OutputLinearColorSpace = false;

            cam.Render(world, ".output/SimpleWorldWithBouncingRaysCameraProgram-LambertianReflection.ppm");

            cam.UseLambertianReflection = true;
            cam.OutputLinearColorSpace = true;

            cam.Render(world, ".output/SimpleWorldWithBouncingRaysCameraProgram-LambertianReflection-OutputLinearColorSpace.ppm");
        }
    }
}
