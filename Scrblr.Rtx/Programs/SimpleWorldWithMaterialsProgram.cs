namespace Scrblr.Rtx
{
    using Scrblr.Rtx.Materials;
    using System;

    class SimpleWorldWithMaterialsProgram
    { 

        public void Main()
        {
            // World
            HittableList world = new HittableList();

            {
                var material_ground = new Lambertian(new Vector3d(0.8, 0.8, 0.0));
                var material_center = new Lambertian(new Vector3d(0.1, 0.2, 0.5));
                var material_left = new Metal(new Vector3d(0.8, 0.8, 0.8), 0.3);
                var material_right = new Metal(new Vector3d(0.8, 0.6, 0.2), 1.0);

                world.Add(new Sphere(new Vector3d(0.0, -100.5, -1.0), 100.0, material_ground));
                world.Add(new Sphere(new Vector3d(0.0, 0.0, -1.2), 0.5, material_center));
                world.Add(new Sphere(new Vector3d(-1.0, 0.0, -1.0), 0.5, material_left));
                world.Add(new Sphere(new Vector3d(1.0, 0.0, -1.0), 0.5, material_right));
            }

            var cam = new CameraScatter();

            cam.aspectRatio = 16.0 / 9.0;
            cam.imageWidth = 400;
            cam.samples_per_pixel = 100;
            cam.max_depth = 50;

            //cam.Render(world, ".output/SimpleWorldWithMaterialsProgram.ppm");

            {
                var material_ground = new Lambertian(new Vector3d(0.8, 0.8, 0.0));
                var material_center = new Lambertian(new Vector3d(0.1, 0.2, 0.5));
                var material_left = new DielectricSimple(1.50);
                var material_bubble = new Dielectric(1.00 / 1.50);
                var material_right = new Metal(new Vector3d(0.8, 0.6, 0.2), 0.0);

                world.Clear();
                world.Add(new Sphere(new Vector3d(0.0, -100.5, -1.0), 100.0, material_ground));
                world.Add(new Sphere(new Vector3d(0.0, 0.0, -1.2), 0.5, material_center));
                world.Add(new Sphere(new Vector3d(-1.0, 0.0, -1.0), 0.5, material_left));
                world.Add(new Sphere(new Vector3d(-1.0, 0.0, -1.0), 0.4, material_bubble));
                world.Add(new Sphere(new Vector3d(1.0, 0.0, -1.0), 0.5, material_right));
            }

            cam.Render(world, ".output/SimpleWorldWithMaterialsProgram-Dielectric.ppm");
        }
    }
}
