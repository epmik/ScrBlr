using NUnit.Framework;
using OpenTK.Mathematics;
using System;

namespace Scrblr.Core.Tests
{
    public class DistanceTests
    {
        [SetUp]
        public void Setup()
        {

        }

        [TearDown]
        public void TearDown()
        {

        }

        [Test]
        public void point_line_2d()
        {
            var point = new Vector2(0, 0);
            var from = new Vector2(0, 0);
            var to = new Vector2(1, 0);
            var line = new Line2(ref from, ref to);

            var result = Distance.Compute(ref point, ref line);

            Assert.IsTrue(result.Distance == 0f);
            Assert.IsTrue(result.Intersection == line.From);




            point = new Vector2(1, 0);
            from = new Vector2(0, 0);
            to = new Vector2(1, 0);
            line = new Line2(ref from, ref to);

            result = Distance.Compute(ref point, ref line);

            Assert.IsTrue(result.Distance == 0f);
            Assert.IsTrue(result.Intersection == line.To);




            point = new Vector2(0, 1);
            from = new Vector2(0, 0);
            to = new Vector2(1, 0);
            line = new Line2(ref from, ref to);

            result = Distance.Compute(ref point, ref line);

            Assert.IsTrue(result.Distance == 1f);
            Assert.IsTrue(result.Intersection == line.From);




            point = new Vector2(2, 1);
            from = new Vector2(0, 0);
            to = new Vector2(1, 0);
            line = new Line2(ref from, ref to);

            result = Distance.Compute(ref point, ref line);

            Assert.IsTrue(FuzzyEquals(result.Distance, 1.4142f));
            Assert.IsTrue(result.Intersection == line.To);




            point = new Vector2(-1, 1);
            from = new Vector2(0, 0);
            to = new Vector2(1, 0);
            line = new Line2(ref from, ref to);

            result = Distance.Compute(ref point, ref line);

            Assert.IsTrue(FuzzyEquals(result.Distance, 1.4142f));
            Assert.IsTrue(result.Intersection == line.From);




            point = new Vector2(0, 1);
            from = new Vector2(-1, 0);
            to = new Vector2(1, 0);
            line = new Line2(ref from, ref to);

            result = Distance.Compute(ref point, ref line);

            Assert.IsTrue(FuzzyEquals(result.Distance, 1f));
            Assert.IsTrue(result.Intersection == new Vector2(0f, 0f));
        }

        private static bool FuzzyEquals(float a, float b, float fuzzy = 0.0001f)
        {
            return MathF.Abs(a - b) <= fuzzy;
        }
    }
}