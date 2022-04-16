using NUnit.Framework;
using System;

namespace Scrblr.Core.Tests
{
    public class VertexTests
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
        public void cannot_construct_Vertex_with_VertexFlag_None()
        {
            var vertexFlag = VertexFlag.None;

            Assert.Throws<ArgumentException>(() => { new Vertex(vertexFlag); });
        }
    }
}