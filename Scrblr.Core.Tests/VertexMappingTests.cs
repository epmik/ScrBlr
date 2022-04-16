using NUnit.Framework;
using System;

namespace Scrblr.Core.Tests
{
    public class VertexMappingTests
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
        public void cannot_construct_VertexMapping_with_duplicate_VertexFlag_in_the_maps()
        {
            var maps = new[] {
                new VertexMapping.Map { VertexFlag = VertexFlag.Position0, ElementType = VertexMapping.ElementType.Single, Count = 3 },
                new VertexMapping.Map { VertexFlag = VertexFlag.Position0, ElementType = VertexMapping.ElementType.Single, Count = 3 },
            };

            Assert.Throws<ArgumentException>(() => { new VertexMapping(maps); });
        }

        [Test]
        public void cannot_construct_VertexMapping_with_VertexFlag_None_in_the_maps()
        {
            var maps = new[] {
                new VertexMapping.Map { VertexFlag = VertexFlag.None, ElementType = VertexMapping.ElementType.Single, Count = 3 },
            };

            Assert.Throws<ArgumentException>(() => { new VertexMapping(maps); });
        }

        [Test]
        public void cannot_construct_VertexMapping_with_OR_VertexFlag_in_the_maps()
        {
            var maps = new[] {
                new VertexMapping.Map { VertexFlag = VertexFlag.Position0 | VertexFlag.Color0, ElementType = VertexMapping.ElementType.Single, Count = 3 },
            };

            Assert.Throws<ArgumentException>(() => { new VertexMapping(maps); });
        }

        [Test]
        public void cannot_construct_VertexMapping_with_ElementType_None_in_the_maps()
        {
            var maps = new[] {
                new VertexMapping.Map { VertexFlag = VertexFlag.Position0, ElementType = VertexMapping.ElementType.None, Count = 3 },
            };

            Assert.Throws<ArgumentException>(() => { new VertexMapping(maps); });
        }

        [Test]
        public void cannot_construct_VertexMapping_with_Count_0_in_the_maps()
        {
            var maps = new[] {
                new VertexMapping.Map { VertexFlag = VertexFlag.Position0, ElementType = VertexMapping.ElementType.Single, Count = 0 },
            };

            Assert.Throws<ArgumentException>(() => { new VertexMapping(maps); });
        }

        [Test]
        public void default_VertexMapping_Map_VertexFlag_value_is_VertexFlag_None()
        {
            var map = new VertexMapping.Map();

            Assert.IsTrue(map.VertexFlag == VertexFlag.None);
        }

        [Test]
        public void default_VertexMapping_Map_ElementType_value_is_ElementType_None()
        {
            var map = new VertexMapping.Map();

            Assert.IsTrue(map.ElementType == VertexMapping.ElementType.None);
        }

        [Test]
        public void default_VertexMapping_Map_Count_value_is_0()
        {
            var map = new VertexMapping.Map();

            Assert.IsTrue(map.Count == 0);
        }
    }
}