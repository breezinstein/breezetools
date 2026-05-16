#if BREEZINSTEIN_HAS_SPLINES
using Breezinstein.Tools.Spline;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace Breezinstein.Tools.Tests
{
    [TestFixture]
    public class SplineIslandTests
    {
        private GameObject host;

        [SetUp]
        public void SetUp()
        {
            host = new GameObject("SplineIslandTestHost");
        }

        [TearDown]
        public void TearDown()
        {
            if (host != null) Object.DestroyImmediate(host);
        }

        private SplineContainer AddSquareSpline()
        {
            var sc = host.AddComponent<SplineContainer>();
            var spline = new UnityEngine.Splines.Spline { Closed = true };
            spline.Add(new BezierKnot(new float3(-1f, 0f, -1f)));
            spline.Add(new BezierKnot(new float3( 1f, 0f, -1f)));
            spline.Add(new BezierKnot(new float3( 1f, 0f,  1f)));
            spline.Add(new BezierKnot(new float3(-1f, 0f,  1f)));
            sc.Spline = spline;
            return sc;
        }

        [Test]
        public void RequiresSplineContainerMeshFilterAndMeshRenderer()
        {
            var requires = typeof(SplineIsland)
                .GetCustomAttributes(typeof(RequireComponent), false);
            Assert.IsTrue(requires.Length > 0, "SplineIsland must declare RequireComponent attributes.");

            bool hasSpline = false, hasFilter = false, hasRenderer = false;
            foreach (RequireComponent rc in requires)
            {
                if (rc.m_Type0 == typeof(SplineContainer) || rc.m_Type1 == typeof(SplineContainer) || rc.m_Type2 == typeof(SplineContainer)) hasSpline = true;
                if (rc.m_Type0 == typeof(MeshFilter) || rc.m_Type1 == typeof(MeshFilter) || rc.m_Type2 == typeof(MeshFilter)) hasFilter = true;
                if (rc.m_Type0 == typeof(MeshRenderer) || rc.m_Type1 == typeof(MeshRenderer) || rc.m_Type2 == typeof(MeshRenderer)) hasRenderer = true;
            }
            Assert.IsTrue(hasSpline, "SplineIsland must require SplineContainer.");
            Assert.IsTrue(hasFilter, "SplineIsland must require MeshFilter.");
            Assert.IsTrue(hasRenderer, "SplineIsland must require MeshRenderer.");
        }

        [Test]
        public void GenerateMesh_ProducesNonEmptyTwoSubmeshMesh()
        {
            AddSquareSpline();
            var island = host.AddComponent<SplineIsland>();

            island.GenerateMesh();

            var mf = host.GetComponent<MeshFilter>();
            Assert.IsNotNull(mf.sharedMesh, "GenerateMesh must assign a mesh to MeshFilter.");
            Assert.Greater(mf.sharedMesh.vertexCount, 0, "Generated mesh should have vertices.");
            Assert.AreEqual(2, mf.sharedMesh.subMeshCount,
                "SplineIsland mesh should have 2 submeshes (top/bottom + sides/bevel).");
            Assert.Greater(mf.sharedMesh.triangles.Length, 0,
                "Generated mesh should have triangles.");
        }

        [Test]
        public void GenerateMesh_AssignsTwoMaterialSlots()
        {
            AddSquareSpline();
            var island = host.AddComponent<SplineIsland>();

            island.GenerateMesh();

            var mr = host.GetComponent<MeshRenderer>();
            Assert.AreEqual(2, mr.sharedMaterials.Length,
                "MeshRenderer should be normalised to 2 material slots.");
        }

        [Test]
        public void GenerateMesh_RegeneratingReusesSameMeshInstance()
        {
            AddSquareSpline();
            var island = host.AddComponent<SplineIsland>();

            island.GenerateMesh();
            var firstMesh = host.GetComponent<MeshFilter>().sharedMesh;

            island.GenerateMesh();
            var secondMesh = host.GetComponent<MeshFilter>().sharedMesh;

            Assert.AreSame(firstMesh, secondMesh,
                "Regenerating should reuse the existing mesh to avoid leaks.");
        }

        [Test]
        public void GenerateMesh_AddsMeshColliderWhenEnabled()
        {
            AddSquareSpline();
            var island = host.AddComponent<SplineIsland>();

            island.GenerateMesh();

            var col = host.GetComponent<MeshCollider>();
            Assert.IsNotNull(col, "MeshCollider should be auto-added when generateCollider is true.");
            Assert.IsNotNull(col.sharedMesh, "MeshCollider.sharedMesh should be assigned.");
        }
    }
}
#endif
