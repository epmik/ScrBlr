namespace Scrblr.Rtx
{
    using Scrblr.Rtx.Materials;
    using System;
    using System.Text.RegularExpressions;

    class SimpleWorldWithUltimateCameraProgram
    { 

        public void Main()
        {
            Scene01(".output/SimpleWorldWithUltimateCameraProgram-scene01.png");

            Scene02(".output/SimpleWorldWithUltimateCameraProgram-scene02.png");

            Scene03(".output/SimpleWorldWithUltimateCameraProgram-scene03.png");
        }

        public void Scene01(string output)
        {
            HittableList world = new HittableList();

            var R = Math.Cos(Math.PI / 4);

            var material_left = new Lambertian(new Vector3d(0, 0, 1));
            var material_right = new Lambertian(new Vector3d(1, 0, 0));

            world.Add(new Sphere(new Vector3d(-R, 0, -1), R, material_left));
            world.Add(new Sphere(new Vector3d(R, 0, -1), R, material_right));

            var cam = new Camera();

            cam.aspectRatio = 16.0 / 9.0;
            cam.imageWidth = 400;
            cam.samples_per_pixel = 100;
            cam.max_depth = 50;

            cam.vfov = 90;

            cam.Render(world, output);
        }

        public void Scene02(string output)
        {
            HittableList world = new HittableList();

            var material_ground = new Lambertian(new Vector3d(0.8, 0.8, 0.0));
            var material_center = new Lambertian(new Vector3d(0.1, 0.2, 0.5));
            var material_left = new Dielectric(1.50);
            var material_bubble = new Dielectric(1.00 / 1.50);
            var material_right = new Metal(new Vector3d(0.8, 0.6, 0.2), 1.0);

            world.Add(new Sphere(new Vector3d(0.0, -100.5, -1.0), 100.0, material_ground));
            world.Add(new Sphere(new Vector3d(0.0, 0.0, -1.2), 0.5, material_center));
            world.Add(new Sphere(new Vector3d(-1.0, 0.0, -1.0), 0.5, material_left));
            world.Add(new Sphere(new Vector3d(-1.0, 0.0, -1.0), 0.4, material_bubble));
            world.Add(new Sphere(new Vector3d(1.0, 0.0, -1.0), 0.5, material_right));

            var cam = new Camera();

            cam.aspectRatio = 16.0 / 9.0;
            cam.imageWidth = 400;
            cam.samples_per_pixel = 100;
            cam.max_depth = 50;

            cam.vfov = 90;
            cam.lookfrom = new Vector3d(-2, 2, 1);
            cam.lookat = new Vector3d(0, 0, -1);
            cam.vup = new Vector3d(0, 1, 0);

            cam.Render(world, output);
        }

        public void Scene03(string output)
        {
            HittableList world = new HittableList();

            var material_ground = new Lambertian(new Vector3d(0.8, 0.8, 0.0));
            var material_center = new Lambertian(new Vector3d(0.1, 0.2, 0.5));
            var material_left = new Dielectric(1.50);
            var material_bubble = new Dielectric(1.00 / 1.50);
            var material_right = new Metal(new Vector3d(0.8, 0.6, 0.2), 1.0);

            world.Add(new Sphere(new Vector3d(0.0, -100.5, -1.0), 100.0, material_ground));
            world.Add(new Sphere(new Vector3d(0.0, 0.0, -1.2), 0.5, material_center));
            world.Add(new Sphere(new Vector3d(-1.0, 0.0, -1.0), 0.5, material_left));
            world.Add(new Sphere(new Vector3d(-1.0, 0.0, -1.0), 0.4, material_bubble));
            world.Add(new Sphere(new Vector3d(1.0, 0.0, -1.0), 0.5, material_right));

            var cam = new Camera();

            cam.aspectRatio = 16.0 / 9.0;
            cam.imageWidth = 400;
            cam.samples_per_pixel = 100;
            cam.max_depth = 50;

            cam.vfov = 20;
            cam.lookfrom = new Vector3d(-2, 2, 1);
            cam.lookat = new Vector3d(0, 0, -1);
            cam.vup = new Vector3d(0, 1, 0);

            cam.Render(world, output);
        }
    }
}
