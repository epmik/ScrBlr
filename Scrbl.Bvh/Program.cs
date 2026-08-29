global using Color3f = Scrbl.Bvh.float3;
global using Color4f = Scrbl.Bvh.float4;
global using Point3f = Scrbl.Bvh.float3;
global using Point4f = Scrbl.Bvh.float4;
using Scrbl.Bvh;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.ColorProfiles;
using SixLabors.ImageSharp.PixelFormats;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Timers;
using System.Xml.Linq;
using static System.Net.WebRequestMethods;

namespace Scrbl.Bvh
{

    struct float3
	{

		public float X;
		public float Y;
		public float Z;
        public float3(float x, float y, float z) => (X, Y, Z) = (x, y, z);
		public float3(float v) => (X, Y, Z) = (v, v, v);
		public float this[in int i] => i switch
		{
			0 => X,
			1 => Y,
			2 => Z,
			_ => throw new ArgumentOutOfRangeException(nameof(i), "Vector index out of bounds!")
		};
		public static implicit operator float3(float value)
		{
			return new float3(value);
		}
		public static float3 operator -(in float3 v) => new(-v.X, -v.Y, -v.Z);
		public static float3 operator +(in float3 u, in float3 v) => new(u.X + v.X, u.Y + v.Y, u.Z + v.Z);
		public static float3 operator -(in float3 u, in float3 v) => new(u.X - v.X, u.Y - v.Y, u.Z - v.Z);
		public static float3 operator *(in float3 u, in float3 v) => new(u.X * v.X, u.Y * v.Y, u.Z * v.Z);
		public static float3 operator *(float t, in float3 v) => new(t * v.X, t * v.Y, t * v.Z);
		public static float3 operator *(in float3 v, float t) => t * v;
		public static float3 operator /(in float3 v, float t) => (1.0f / t) * v;
		public static float3 Min(in float3 a, in float3 b)
		{
			return new float3(MathF.Min(a.X, b.X), MathF.Min(a.Y, b.Y), MathF.Min(a.Z, b.Z));
		}
		public static float3 Max(in float3 a, in float3 b)
		{
			return new float3(MathF.Max(a.X, b.X), MathF.Max(a.Y, b.Y), MathF.Max(a.Z, b.Z));
		}
		public static float3 cross(in float3 a, in float3 b) { return new float3(a.Y * b.Z - a.Z * b.Y, a.Z * b.X - a.X * b.Z, a.X * b.Y - a.Y * b.X); }

		public static float dot(in float3 a, in float3 b ) { return a.X * b.X + a.Y * b.Y + a.Z * b.Z; }

		public static float3 normalize( in float3 v ) { float invLen = 1.0f / MathF.Sqrt(dot(v, v)); return v* invLen; }

        public override string ToString() => $"{X} {Y} {Z}";
    };
    struct float4
    {

        public float X;
        public float Y;
        public float Z;
        public float W;

        public float4(float x, float y, float z) => (X, Y, Z, W) = (x, y, z, 1f);
        public float4(float x, float y, float z, float w) => (X, Y, Z, W) = (x, y, z, w);
        public float4(float3 v) => (X, Y, Z, W) = (v.X, v.Y, v.Z, 1f);
        public float4(float4 v) => (X, Y, Z, W) = (v.X, v.Y, v.Z, v.W);
        public float this[in int i] => i switch
        {
            0 => X,
            1 => Y,
            2 => Z,
            3 => W,
            _ => throw new ArgumentOutOfRangeException(nameof(i), "Vector index out of bounds!")
        };
        public static implicit operator float4(float3 value)
        {
            return new float4(value);
        }
        public static implicit operator float4(float value)
        {
            return new float4(value);
        }

        public override string ToString() => $"{X} {Y} {Z}";
    };

    struct Tri 
	{
		public float3 vertex0;
		public float3 vertex1;
		public float3 vertex2; 
		public float3 centroid; 
	};

	struct BVHNode
	{
		public float3 aabbMin;
		public int leftFirst;
		public float3 aabbMax; 
		public int triCount;

		public bool isLeaf() { return triCount > 0; }
	};

	struct aabb
	{
		public float3 bmin = new float3(1e30f);
		public float3 bmax = new float3(-1e30f);

		public aabb()
		{
		}

		void grow(float3 p) 
		{ 
			bmin = float3.Min(bmin, p); 
			bmax = float3.Max(bmax, p); 
		}

		float area()
		{
			float3 e = bmax - bmin; // box extent
			return e.X * e.Y + e.Y * e.Z + e.Z * e.X;
		}
	};

	[StructLayout(LayoutKind.Sequential, Pack = 64)]
	struct Ray
	{
		public float3 O;
		public float3 D;
		public float3 rD;
		public float t = 1e30f;

		public Ray()
		{
		}
	};

	class Sample
	{
		const int TriangleCount = 12582;

		Tri[] tri = new Tri[TriangleCount];

		int[] triIdx = new int[TriangleCount];

		BVHNode[] bvhNode = new BVHNode[TriangleCount * 2];

		//BVHNode* bvhNode = 0;
		int rootNodeIdx = 0, nodesUsed = 2;

		public void IntersectTri(ref Ray ray, in Tri tri)
		{
			float3 edge1 = tri.vertex1 - tri.vertex0;
			float3 edge2 = tri.vertex2 - tri.vertex0;
			float3 h = float3.cross(ray.D, edge2);
			float a = float3.dot(edge1, h);
			if (a > -0.0001f && a < 0.0001f) return; // ray parallel to triangle
			float f = 1 / a;
			float3 s = ray.O - tri.vertex0;
			float u = f * float3.dot(s, h);
			if (u < 0 || u > 1) return;
			float3 q = float3.cross(s, edge1);
			float v = f * float3.dot(ray.D, q);
			if (v < 0 || u + v > 1) return;
			float t = f * float3.dot(edge2, q);
			if (t > 0.0001f) ray.t = MathF.Min(ray.t, t);
		}

        float IntersectAABB(in Ray ray, in float3 bmin, in float3 bmax)
		{

			float tx1 = (bmin.X - ray.O.X) / ray.D.X, tx2 = (bmax.X - ray.O.X) / ray.D.X;
			float tmin = MathF.Min(tx1, tx2);
			float tmax = MathF.Max(tx1, tx2);
			float ty1 = (bmin.Y - ray.O.Y) / ray.D.Y, ty2 = (bmax.Y - ray.O.Y) / ray.D.Y;
			tmin = MathF.Max(tmin, MathF.Min(ty1, ty2));
			tmax = MathF.Min(tmax, MathF.Max(ty1, ty2) );
			float tz1 = (bmin.Z - ray.O.Z) / ray.D.Z, tz2 = (bmax.Z - ray.O.Z) / ray.D.Z;
			tmin = MathF.Max(tmin, MathF.Min(tz1, tz2));
			tmax = MathF.Min(tmax, MathF.Max(tz1, tz2) );

			return tmax >= tmin && tmin<ray.t && tmax> 0 ? tmin : 1e30f;
		}

		//public float IntersectAABB(in Ray ray, in float3 bmin, in float3 bmax)
		//{
		//	float tx1 = (bmin.X - ray.O.X) * ray.rD.X, tx2 = (bmax.X - ray.O.X) * ray.rD.X;
		//	float tmin = MathF.Min(tx1, tx2), tmax = MathF.Max(tx1, tx2);
		//	float ty1 = (bmin.Y - ray.O.Y) * ray.rD.Y, ty2 = (bmax.Y - ray.O.Y) * ray.rD.Y;
		//	tmin = MathF.Max(tmin, MathF.Min(ty1, ty2));
		//	tmax = MathF.Min(tmax, MathF.Max(ty1, ty2));
		//	float tz1 = (bmin.Z - ray.O.Z) * ray.rD.Z, tz2 = (bmax.Z - ray.O.Z) * ray.rD.Z;
		//	tmin = MathF.Max(tmin, MathF.Min(tz1, tz2));
		//	tmax = MathF.Min(tmax, MathF.Max(tz1, tz2));
		//	if (tmax >= tmin && tmin < ray.t && tmax > 0) return tmin; else return 1e30f;
		//}

		void IntersectBVH(ref Ray ray, int nodeIdx)
		{

			ref BVHNode node = ref bvhNode[nodeIdx];

			if (1e30f == IntersectAABB(ray, node.aabbMin, node.aabbMax )) return;

			if (node.isLeaf())
			{
				for (uint i = 0; i<node.triCount; i++ )
					IntersectTri(ref ray, tri[triIdx[node.leftFirst + i]] );
			}
			else
			{
				IntersectBVH(ref ray, node.leftFirst );
				IntersectBVH(ref ray, node.leftFirst + 1 );
			}
		}

		void BuildBVH()
		{
			// populate triangle index array
			for (int i = 0; i < TriangleCount; i++) triIdx[i] = i;
			// calculate triangle centroids for partitioning
			for (int i = 0; i < TriangleCount; i++)
			{
                //ref Tri t = ref tri[i];
                tri[i].centroid = (tri[i].vertex0 + tri[i].vertex1 + tri[i].vertex2) * 0.3333f;
            }
			// assign all triangles to root node
			ref BVHNode  root = ref bvhNode[rootNodeIdx];
			root.leftFirst = 0;
			root.triCount = TriangleCount;
			UpdateNodeBounds(rootNodeIdx);
			// subdivide recursively
			Subdivide(rootNodeIdx);
		}

		void UpdateNodeBounds(int nodeIdx)
		{
			ref BVHNode node = ref bvhNode[nodeIdx];
			node.aabbMin = 1e30f;
			node.aabbMax = -1e30f;
			for (int first = node.leftFirst, i = 0; i < node.triCount; i++)
			{
				int leafTriIdx = triIdx[first + i];
				ref Tri leafTri = ref tri[leafTriIdx];
				node.aabbMin = float3.Min(node.aabbMin, leafTri.vertex0);
				node.aabbMin = float3.Min(node.aabbMin, leafTri.vertex1);
				node.aabbMin = float3.Min(node.aabbMin, leafTri.vertex2);
				node.aabbMax = float3.Max(node.aabbMax, leafTri.vertex0);
				node.aabbMax = float3.Max(node.aabbMax, leafTri.vertex1);
				node.aabbMax = float3.Max(node.aabbMax, leafTri.vertex2);
			}
		}

		void Subdivide(int nodeIdx)
		{
			// terminate recursion
			ref BVHNode node = ref bvhNode[nodeIdx];
			if (node.triCount <= 2) return;
			// determine split axis and position
			float3 extent = node.aabbMax - node.aabbMin;
			int axis = 0;
			if (extent.Y > extent.X) axis = 1;
			if (extent.Z > extent[axis]) axis = 2;
			float splitPos = node.aabbMin[axis] + extent[axis] * 0.5f;
			// in-place partition
			int i = node.leftFirst;
			int j = i + node.triCount - 1;
			while (i <= j)
			{
				if (tri[triIdx[i]].centroid[axis] < splitPos)
					i++;
				else
				{
					// https://stackoverflow.com/questions/804706/swap-two-variables-without-using-a-temporary-variable
					(triIdx[i], triIdx[j]) = (triIdx[j], triIdx[i]);
					j--;
				}
			}
			// abort split if one of the sides is empty
			int leftCount = i - node.leftFirst;
			if (leftCount == 0 || leftCount == node.triCount) return;
			// create child nodes
			int leftChildIdx = nodesUsed++;
			int rightChildIdx = nodesUsed++;
			bvhNode[leftChildIdx].leftFirst = node.leftFirst;
			bvhNode[leftChildIdx].triCount = leftCount;
			bvhNode[rightChildIdx].leftFirst = i;
			bvhNode[rightChildIdx].triCount = node.triCount - leftCount;
			node.leftFirst = leftChildIdx;
			node.triCount = 0;
			UpdateNodeBounds(leftChildIdx);
			UpdateNodeBounds(rightChildIdx);
			// recurse
			Subdivide(leftChildIdx);
			Subdivide(rightChildIdx);
		}

		void Init()
		{
			char[] SplitOptions = new[] { ' ', '\t', '\n', '\r' };

			using (var reader = new StreamReader(@"C:\Steven\Atelier\Scrbl\Scrbl.JaccoBikker\.assets\unity.tri"))
			{
				string line = reader.ReadLine();

				string[] tokens = line.Split(SplitOptions, StringSplitOptions.RemoveEmptyEntries);

				var triangleCount = uint.Parse(tokens[0], CultureInfo.InvariantCulture);

				for (uint t = 0; t < triangleCount; t++)
				{
					line = reader.ReadLine();

					tokens = line.Split(SplitOptions, StringSplitOptions.RemoveEmptyEntries);

					tri[t].vertex0.X = float.Parse(tokens[0], CultureInfo.InvariantCulture);
					tri[t].vertex0.Y = float.Parse(tokens[1], CultureInfo.InvariantCulture);
					tri[t].vertex0.Z = float.Parse(tokens[2], CultureInfo.InvariantCulture);
					tri[t].vertex1.X = float.Parse(tokens[3], CultureInfo.InvariantCulture);
					tri[t].vertex1.Y = float.Parse(tokens[4], CultureInfo.InvariantCulture);
					tri[t].vertex1.Z = float.Parse(tokens[5], CultureInfo.InvariantCulture);
					tri[t].vertex2.X = float.Parse(tokens[6], CultureInfo.InvariantCulture);
					tri[t].vertex2.Y = float.Parse(tokens[7], CultureInfo.InvariantCulture);
					tri[t].vertex2.Z = float.Parse(tokens[8], CultureInfo.InvariantCulture);
				}
			}

			// construct the BVH
			BuildBVH();
		}

		const int SCRWIDTH = 640, SCRHEIGHT = 640;


		void Tick(int counter)
		{
            var buffer = new Color4f[SCRWIDTH * SCRHEIGHT];

            var bufferIndex = 0;

            Stopwatch stopwatch = Stopwatch.StartNew();

            // define the corners of the screen in worldspace
            float3 p0 = new float3(-2.5f, 0.8f, -0.5f), p1 = new float3(-0.5f, 0.8f, -0.5f), p2 = new float3(-2.5f, -1.2f, -0.5f);
            Ray ray = new Ray();
            ray.O = new float3(-1.5f, -0.2f, -2.5f);

            for (int y = 0; y < SCRHEIGHT; y++) for (int x = 0; x < SCRWIDTH; x++)
			{
				// calculate the position of a pixel on the screen in worldspace
				float3 pixelPos = p0 + (p1 - p0) * (x / (float)SCRWIDTH) + (p2 - p0) * (y / (float)SCRHEIGHT);
				// define the ray in worldspace
				ray.D = float3.normalize(pixelPos - ray.O);
				// initially the ray has an 'infinite length'
				ray.t = 1e30f;

				IntersectBVH(ref ray, rootNodeIdx);

                var c = ray.t < 1e30f ? Utility.Remap(ray.t, 1.2f, 3.4f, 1f, 0f, true) : 0f;
                var a = ray.t < 1e30f ? 1f : c;

                buffer[bufferIndex++] = new Color4f(c, c, c, a);

                //buffer[bufferIndex++] = ray.t < 1e30f ? new Color4f(1, 1, 1, 1) : new Color4f(0, 0, 0, 1);
            }

            stopwatch.Stop();

			Console.WriteLine($"Tick duration: {stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff")}");

            SaveBufferAsImage(buffer, SCRWIDTH, SCRHEIGHT, $@"C:\Steven\Atelier\Scrbl\Scrbl.JaccoBikker\.output\Tick.{counter}.png");
        }

        void TickFast(int counter)
        {
            var buffer = new Color4f[SCRWIDTH * SCRHEIGHT];

            var bufferIndex = 0;

            Stopwatch stopwatch = Stopwatch.StartNew();

			// draw the scene
			// define the corners of the screen in worldspace
			float3 p0 = new float3( -2.5f, 0.8f, -0.5f ), p1 = new float3(-0.5f, 0.8f, -0.5f), p2 = new float3(-2.5f, -1.2f, -0.5f);
			Ray ray = new Ray();
            ray.O = new float3(-1.5f, -0.2f, -2.5f);
            //Timer t;
            // render tiles of pixels
            for (int y = 0; y < SCRHEIGHT; y += 4) for (int x = 0; x < SCRWIDTH; x += 4)
			{
				// render a single tile
				for (int v = 0; v < 4; v++) for (int u = 0; u < 4; u++)
				{
					// calculate the position of a pixel on the screen in worldspace
					float3 pixelPos = p0 + (p1 - p0) * ((x + u) / (float)SCRWIDTH) + (p2 - p0) * ((y + v) / (float)SCRHEIGHT);
					// define the ray in worldspace					
					ray.D = float3.normalize(pixelPos - ray.O);
					ray.t = 1e30f;
					// calculare reciprocal ray directions to speedup AABB intersections
					ray.rD = new float3(1 / ray.D.X, 1 / ray.D.Y, 1 / ray.D.Z);
					
					IntersectBVH(ref ray, 0);

                    var c = ray.t < 1e30f ? Utility.Remap(ray.t, 1.2f, 3.4f, 1f, 0f, true) : 0f;

                    buffer[bufferIndex++] = new Color4f(c, c, c, c);
                }
            }

			stopwatch.Stop();

			Console.WriteLine($"TickFast duration: {stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff")}");

            SaveBufferAsImage(buffer, SCRWIDTH, SCRHEIGHT, $@"C:\Steven\Atelier\Scrbl\Scrbl.JaccoBikker\.output\TickFast.{counter}.png");
        }

        unsafe static void SaveBufferAsImage(Color4f[] buffer, int width, int height, string path)
        {
            var byteBuffer = new byte[width * height * 4];

            var bufferIndex = 0;
            var byteBufferIndex = 0;

            for (int j = 0; j < height; j++)
            {
                for (int i = 0; i < width; i++)
                {
                    byteBuffer[byteBufferIndex++] = (byte)(255 * Math.Clamp(buffer[bufferIndex].X, 0.0, 1.0));
                    byteBuffer[byteBufferIndex++] = (byte)(255 * Math.Clamp(buffer[bufferIndex].Y, 0.0, 1.0));
                    byteBuffer[byteBufferIndex++] = (byte)(255 * Math.Clamp(buffer[bufferIndex].Z, 0.0, 1.0));
                    byteBuffer[byteBufferIndex++] = (byte)(255 * Math.Clamp(buffer[bufferIndex].W, 0.0, 1.0));
                    bufferIndex++;
                }
            }

            using (var image = SixLabors.ImageSharp.Image.LoadPixelData<Rgba32>(byteBuffer, width, height))
            {
                image.SaveAsPng(path);
            }

            fixed (void* bufferHandle = &buffer[0])
            {
                var byteSpan = new Span<byte>(bufferHandle, width * height * sizeof(Color4f));

                using (var image = SixLabors.ImageSharp.Image.LoadPixelData<RgbaVector>(byteSpan, width, height))
                {
                    image.SaveAsPng(path + ".---.png");
                }
            }
        }

        public void Run()
		{
			Init();

            for (var i = 0; i < 10; i++)
            {
                Tick(i);
            }

            for (var i = 0; i < 10; i++)
            {
                TickFast(i);
            }
        }
    }

	internal class Program
	{

		static void Main(string[] args)
		{
			new Sample().Run();

			Console.WriteLine($"Press a key to exit...");

			Console.ReadKey(false);
		}
	}
}
