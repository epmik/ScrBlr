using NUnit.Framework;
using OpenTK.Mathematics;
using System;

namespace Scrblr.Core.Tests
{
    public class IntersectionTests
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
            var linefrom = new Vector2(0, 0);
            var lineto = new Vector2(1, 0);
            var line = new Line2(ref linefrom, ref lineto);

            var result = Intersection.Compute(ref point, ref line);

            Assert.IsTrue(result.Status == Intersection.Status.Intersection);
            Assert.IsTrue(result.Intersection0 == line.From);
            Assert.IsTrue(FuzzyEquals(result.T0, 0f));




            point = new Vector2(1, 0);
            linefrom = new Vector2(0, 0);
            lineto = new Vector2(1, 0);
            line = new Line2(ref linefrom, ref lineto);

            result = Intersection.Compute(ref point, ref line);

            Assert.IsTrue(result.Status == Intersection.Status.Intersection);
            Assert.IsTrue(result.Intersection0 == line.To);
            Assert.IsTrue(FuzzyEquals(result.T0, 1f));




            point = new Vector2(0, 1);
            linefrom = new Vector2(0, 0);
            lineto = new Vector2(4, 0);
            line = new Line2(ref linefrom, ref lineto);

            result = Intersection.Compute(ref point, ref line);

            Assert.IsTrue(result.Status == Intersection.Status.Intersection);
            Assert.IsTrue(result.Intersection0 == line.From);
            Assert.IsTrue(FuzzyEquals(result.T0, 0f));




            point = new Vector2(3, 1);
            linefrom = new Vector2(0, 0);
            lineto = new Vector2(4, 0);
            line = new Line2(ref linefrom, ref lineto);

            result = Intersection.Compute(ref point, ref line);

            Assert.IsTrue(result.Status == Intersection.Status.Intersection);
            Assert.IsTrue(result.Intersection0 == new Vector2(3, 0));
            Assert.IsTrue(FuzzyEquals(result.T0, 0.75f));




            point = new Vector2(4, 1);
            linefrom = new Vector2(0, 0);
            lineto = new Vector2(2, 0);
            line = new Line2(ref linefrom, ref lineto);

            result = Intersection.Compute(ref point, ref line);

            Assert.IsTrue(result.Status == Intersection.Status.None);
            Assert.IsTrue(result.Intersection0 == new Vector2(4, 0));
            Assert.IsTrue(FuzzyEquals(result.T0, 2f));




            point = new Vector2(-1, 1);
            linefrom = new Vector2(0, 0);
            lineto = new Vector2(1, 0);
            line = new Line2(ref linefrom, ref lineto);

            result = Intersection.Compute(ref point, ref line);

            Assert.IsTrue(result.Status == Intersection.Status.None);
            Assert.IsTrue(result.Intersection0 == new Vector2(-1, 0));
            Assert.IsTrue(FuzzyEquals(result.T0, -1f));




            point = new Vector2(0, 1);
            linefrom = new Vector2(-1, 0);
            lineto = new Vector2(1, 0);
            line = new Line2(ref linefrom, ref lineto);

            result = Intersection.Compute(ref point, ref line);

            Assert.IsTrue(result.Status == Intersection.Status.Intersection);
            Assert.IsTrue(result.Intersection0 == new Vector2(0f, 0f));
            Assert.IsTrue(FuzzyEquals(result.T0, 0.5f));




            point = new Vector2(0, 0);
            linefrom = new Vector2(-1, 0);
            lineto = new Vector2(1, 0);
            line = new Line2(ref linefrom, ref lineto);

            result = Intersection.Compute(ref point, ref line);

            Assert.IsTrue(result.Status == Intersection.Status.Intersection);
            Assert.IsTrue(result.Intersection0 == new Vector2(0f, 0f));
            Assert.IsTrue(FuzzyEquals(result.T0, 0.5f));
        }

        [Test]
        public void line_line_2d()
        {
            var line0from = new Vector2(0, 1);
            var line0to = new Vector2(0, -1);
            var line0 = new Line2(ref line0from, ref line0to);
            var line1from = new Vector2(-1, 0);
            var line1to = new Vector2(1, 0);
            var line1 = new Line2(ref line1from, ref line1to);

            var result = Intersection.Compute(ref line0, ref line1);

            Assert.IsTrue(result.Status == Intersection.Status.Intersection);
            //Assert.IsTrue(result.Distance == 0f);
            Assert.IsTrue(result.Intersection0 == new Vector2(0, 0));
            Assert.IsTrue(result.Intersection1 == new Vector2(0, 0));
            Assert.IsTrue(FuzzyEquals(result.T0, 0.5f));
            Assert.IsTrue(FuzzyEquals(result.T1, 0.5f));




            line0from = new Vector2(0, 3);
            line0to = new Vector2(0, 1);
            line0 = new Line2(ref line0from, ref line0to);
            line1from = new Vector2(-1, 0);
            line1to = new Vector2(1, 0);
            line1 = new Line2(ref line1from, ref line1to);

            result = Intersection.Compute(ref line0, ref line1);

            Assert.IsTrue(result.Status == Intersection.Status.None);
            //Assert.IsTrue(result.Distance == 1f);
            Assert.IsTrue(result.Intersection0 == new Vector2(0, 0));
            Assert.IsTrue(result.Intersection1 == new Vector2(0, 0));
            Assert.IsTrue(FuzzyEquals(result.T0, 1.5f));
            Assert.IsTrue(FuzzyEquals(result.T1, 0.5f));




            line0from = new Vector2(0, -1);
            line0to = new Vector2(0, -3);
            line0 = new Line2(ref line0from, ref line0to);
            line1from = new Vector2(-1, 0);
            line1to = new Vector2(1, 0);
            line1 = new Line2(ref line1from, ref line1to);

            result = Intersection.Compute(ref line0, ref line1);

            Assert.IsTrue(result.Status == Intersection.Status.None);
            //Assert.IsTrue(result.Distance == 1f);
            Assert.IsTrue(result.Intersection0 == new Vector2(0, 0));
            Assert.IsTrue(result.Intersection1 == new Vector2(0, 0));
            Assert.IsTrue(FuzzyEquals(result.T0, -0.5f));
            Assert.IsTrue(FuzzyEquals(result.T1, 0.5f));




            line0from = new Vector2(2, 1);
            line0to = new Vector2(2, -1);
            line0 = new Line2(ref line0from, ref line0to);
            line1from = new Vector2(-1, 0);
            line1to = new Vector2(1, 0);
            line1 = new Line2(ref line1from, ref line1to);

            result = Intersection.Compute(ref line0, ref line1);

            Assert.IsTrue(result.Status == Intersection.Status.None);
            //Assert.IsTrue(result.Distance == 0f);
            Assert.IsTrue(result.Intersection0 == new Vector2(2, 0));
            Assert.IsTrue(result.Intersection1 == new Vector2(2, 0));
            Assert.IsTrue(FuzzyEquals(result.T0, 0.5f));
            Assert.IsTrue(FuzzyEquals(result.T1, 1.5f));




            line0from = new Vector2(-2, 2);
            line0to = new Vector2(2, -2);
            line0 = new Line2(ref line0from, ref line0to);
            line1from = new Vector2(-4, 0);
            line1to = new Vector2(4, 0);
            line1 = new Line2(ref line1from, ref line1to);

            result = Intersection.Compute(ref line0, ref line1);

            Assert.IsTrue(result.Status == Intersection.Status.Intersection);
            //Assert.IsTrue(result.Distance == 0f);
            Assert.IsTrue(result.Intersection0 == new Vector2(0, 0));
            Assert.IsTrue(result.Intersection1 == new Vector2(0, 0));
            Assert.IsTrue(FuzzyEquals(result.T0, 0.5f));
            Assert.IsTrue(FuzzyEquals(result.T1, 0.5f));




            line0from = new Vector2(-2, 2);
            line0to = new Vector2(-1, 1);
            line0 = new Line2(ref line0from, ref line0to);
            line1from = new Vector2(-4, 0);
            line1to = new Vector2(4, 0);
            line1 = new Line2(ref line1from, ref line1to);

            result = Intersection.Compute(ref line0, ref line1);

            Assert.IsTrue(result.Status == Intersection.Status.None);
            //Assert.IsTrue(result.Distance == 0f);
            Assert.IsTrue(result.Intersection0 == new Vector2(0, 0));
            Assert.IsTrue(result.Intersection1 == new Vector2(0, 0));
            Assert.IsTrue(FuzzyEquals(result.T0, 2f));
            Assert.IsTrue(FuzzyEquals(result.T1, 0.5f));




            line0from = new Vector2(-2, 2);
            line0to = new Vector2(-2, 0);
            line0 = new Line2(ref line0from, ref line0to);
            line1from = new Vector2(-4, 2);
            line1to = new Vector2(-4, 0);
            line1 = new Line2(ref line1from, ref line1to);

            result = Intersection.Compute(ref line0, ref line1);

            Assert.IsTrue(result.Status == Intersection.Status.Parallel);
            //Assert.IsTrue(result.Distance == 0f);
            Assert.IsTrue(result.Intersection0 == line0from);
            Assert.IsTrue(result.Intersection1 == line1from);
            Assert.IsTrue(FuzzyEquals(result.T0, 0f));
            Assert.IsTrue(FuzzyEquals(result.T1, 0f));






            //line0from = new Vector2(0, 0);
            //line0to = new Vector2(1, 0);
            //line0 = new Line2(ref line0from, ref line0to);

            //result = Intersection.Compute(ref line0, ref line1);

            //Assert.IsTrue(result.Distance == 0f);
            //Assert.IsTrue(result.Intersection0 == line0.To);
            //Assert.IsTrue(FuzzyEquals(result.T0, 1f));




            //line0from = new Vector2(0, 0);
            //line0to = new Vector2(4, 0);
            //line0 = new Line2(ref line0from, ref line0to);

            //result = Intersection.Compute(ref line0, ref line1);

            //Assert.IsTrue(result.Distance == 1f);
            //Assert.IsTrue(result.Intersection0 == line0.From);
            //Assert.IsTrue(FuzzyEquals(result.T0, 0f));




            //line0from = new Vector2(0, 0);
            //line0to = new Vector2(4, 0);
            //line0 = new Line2(ref line0from, ref line0to);

            //result = Intersection.Compute(ref line0, ref line1);

            //Assert.IsTrue(FuzzyEquals(result.Distance, 1f));
            //Assert.IsTrue(result.Intersection0 == new Vector2(3, 0));
            //Assert.IsTrue(FuzzyEquals(result.T0, 0.75f));




            //line0from = new Vector2(0, 0);
            //line0to = new Vector2(2, 0);
            //line0 = new Line2(ref line0from, ref line0to);

            //result = Intersection.Compute(ref line0, ref line1);

            //Assert.IsTrue(FuzzyEquals(result.Distance, 1f));
            //Assert.IsTrue(result.Intersection0 == new Vector2(4, 0));
            //Assert.IsTrue(FuzzyEquals(result.T0, 2f));




            //line0from = new Vector2(0, 0);
            //line0to = new Vector2(1, 0);
            //line0 = new Line2(ref line0from, ref line0to);

            //result = Intersection.Compute(ref line0, ref line1);

            //Assert.IsTrue(FuzzyEquals(result.Distance, 1f));
            //Assert.IsTrue(result.Intersection0 == new Vector2(-1, 0));
            //Assert.IsTrue(FuzzyEquals(result.T0, -1f));




            //line0from = new Vector2(-1, 0);
            //line0to = new Vector2(1, 0);
            //line0 = new Line2(ref line0from, ref line0to);

            //result = Intersection.Compute(ref line0, ref line1);

            //Assert.IsTrue(FuzzyEquals(result.Distance, 1f));
            //Assert.IsTrue(result.Intersection0 == new Vector2(0f, 0f));
            //Assert.IsTrue(FuzzyEquals(result.T0, 0.5f));




            //line0from = new Vector2(-1, 0);
            //line0to = new Vector2(1, 0);
            //line0 = new Line2(ref line0from, ref line0to);

            //result = Intersection.Compute(ref line0, ref line1);

            //Assert.IsTrue(FuzzyEquals(result.Distance, 0f));
            //Assert.IsTrue(result.Intersection0 == new Vector2(0f, 0f));
            //Assert.IsTrue(FuzzyEquals(result.T0, 0.5f));
        }

        private static bool FuzzyEquals(float a, float b, float fuzzy = 0.0001f)
        {
            return MathF.Abs(a - b) <= fuzzy;
        }
    }
}