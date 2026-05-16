#if BREEZINSTEIN_HAS_SPLINES
using System.Reflection;
using Breezinstein.Tools.Spline;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace Breezinstein.Tools.Tests
{
    [TestFixture]
    public class SplineHoleTests
    {
        private GameObject host;

        [SetUp]
        public void SetUp()
        {
            host = new GameObject("SplineHoleTestHost");
        }

        [TearDown]
        public void TearDown()
        {
            if (host != null) Object.DestroyImmediate(host);
        }

        [Test]
        public void RequiresSplineContainer()
        {
            var requires = typeof(SplineHole).GetCustomAttributes(typeof(RequireComponent), false);
            Assert.IsTrue(requires.Length > 0, "SplineHole must declare RequireComponent.");

            bool hasSpline = false;
            foreach (RequireComponent rc in requires)
            {
                if (rc.m_Type0 == typeof(SplineContainer) ||
                    rc.m_Type1 == typeof(SplineContainer) ||
                    rc.m_Type2 == typeof(SplineContainer))
                {
                    hasSpline = true;
                    break;
                }
            }
            Assert.IsTrue(hasSpline, "SplineHole must require SplineContainer.");
        }

        [Test]
        public void IsExecuteAlways()
        {
            var attrs = typeof(SplineHole).GetCustomAttributes(typeof(ExecuteAlways), false);
            Assert.AreEqual(1, attrs.Length,
                "SplineHole must be [ExecuteAlways] so the hole stays in sync outside play mode.");
        }

        [Test]
        public void Triangulate_SquarePolygonProducesTwoTriangles()
        {
            // Validate the pure triangulation helper that powers both SplineHole and SplineIsland.
            // Square polygon has 4 vertices → ear clipping produces exactly 2 triangles (6 indices).
            var poly = new[]
            {
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(1f, 1f),
                new Vector2(0f, 1f),
            };

            MethodInfo triangulate = typeof(SplineHole).GetMethod(
                "Triangulate",
                BindingFlags.Static | BindingFlags.NonPublic);
            Assert.IsNotNull(triangulate, "SplineHole.Triangulate(Vector2[], int) must exist.");

            var tris = (System.Collections.Generic.List<int>)triangulate.Invoke(
                null, new object[] { poly, -1 });

            Assert.IsNotNull(tris);
            Assert.AreEqual(6, tris.Count, "Square should yield exactly 2 triangles.");
            // All indices must be inside the polygon range
            foreach (int idx in tris)
            {
                Assert.GreaterOrEqual(idx, 0);
                Assert.Less(idx, poly.Length);
            }
        }

        [Test]
        public void SignedArea_PositiveForCcwSquare_NegativeForCwSquare()
        {
            MethodInfo signedArea = typeof(SplineHole).GetMethod(
                "SignedArea",
                BindingFlags.Static | BindingFlags.NonPublic);
            Assert.IsNotNull(signedArea, "SplineHole.SignedArea(Vector2[]) must exist.");

            var ccw = new[]
            {
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(1f, 1f),
                new Vector2(0f, 1f),
            };
            var cw = new[]
            {
                new Vector2(0f, 0f),
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(1f, 0f),
            };

            float ccwArea = (float)signedArea.Invoke(null, new object[] { ccw });
            float cwArea = (float)signedArea.Invoke(null, new object[] { cw });

            Assert.AreEqual(1f, ccwArea, 1e-4f, "CCW unit square should have signed area +1.");
            Assert.AreEqual(-1f, cwArea, 1e-4f, "CW unit square should have signed area -1.");
        }

        [Test]
        public void CanBeAddedToGameObjectWithSplineContainer()
        {
            var sc = host.AddComponent<SplineContainer>();
            var spline = new UnityEngine.Splines.Spline { Closed = true };
            spline.Add(new BezierKnot(new float3(-1f, 0f, -1f)));
            spline.Add(new BezierKnot(new float3( 1f, 0f, -1f)));
            spline.Add(new BezierKnot(new float3( 0f, 0f,  1f)));
            sc.Spline = spline;

            var hole = host.AddComponent<SplineHole>();
            Assert.IsNotNull(hole);
        }
    }
}
#endif
