#if BREEZINSTEIN_HAS_SPLINES
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Breezinstein.Tools.Spline
{
    /// <summary>
    /// Generates editor-time hole visuals (base, walls, stencil mask) from a closed
    /// <see cref="SplineContainer"/>. The hole shape follows the spline outline instead of
    /// being restricted to a circle.
    /// <para>
    /// Setup:
    /// <list type="number">
    ///   <item>Place this component on a GameObject that also has a SplineContainer.</item>
    ///   <item>Assign the three hole materials (mask, base, optional sides override).</item>
    ///   <item>Right-click the component header → "Rebuild Hole Visuals", or enable Auto Update.</item>
    ///   <item>The surface above must use a compatible stencil shader.</item>
    /// </list>
    /// </para>
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(SplineContainer))]
    public class SplineHole : MonoBehaviour
    {
        [Header("Shape")]
        [Tooltip("Number of vertices sampled around the perimeter. Higher = smoother curves.")]
        [SerializeField] [Range(8, 256)] private int sampleCount = 64;

        [Header("Depth")]
        [Tooltip("How deep the hole is, in local units.")]
        [SerializeField] [Min(0.001f)] private float holeDepth = 0.05f;

        [Header("Materials")]
        [Tooltip("Material using a hole-mask (stencil-writing) shader.")]
        [SerializeField] private Material holeMaskMaterial;

        [Tooltip("Material used for the flat polygon at the bottom of the hole.")]
        [SerializeField] private Material holeBaseMaterial;

        [Tooltip("Material for the wall inside the hole. Falls back to holeBaseMaterial if unassigned.")]
        [SerializeField] private Material holeSidesMaterial;

        [Header("Options")]
        [Tooltip("Regenerate automatically when Inspector values change (editor only).")]
        [SerializeField] private bool autoUpdate = false;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!autoUpdate) return;
            EditorApplication.delayCall += () =>
            {
                if (this == null) return;
                if (PrefabUtility.IsPartOfPrefabAsset(this)) return;
                RebuildHoleVisuals();
            };
        }

        [ContextMenu("Rebuild Hole Visuals")]
        private void RebuildHoleVisuals()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i);
                if (child.name == "HoleBase" || child.name == "HoleWalls" || child.name == "HoleMask")
                    DestroyImmediate(child.gameObject);
            }

            SplineContainer sc = GetComponent<SplineContainer>();
            if (sc == null || sc.Splines == null || sc.Splines.Count == 0)
            {
                Debug.LogWarning("SplineHole: No spline found on this GameObject.");
                return;
            }

            int n = Mathf.Max(3, sampleCount);
            Vector2[] perim = SamplePerimeter(sc.Splines[0], n);

            // Ensure CCW winding (positive signed area in XZ)
            if (SignedArea(perim) < 0f) System.Array.Reverse(perim);

            Mesh polyMesh = BuildPolygonMesh(perim);
            Mesh wallMesh = BuildWallMesh(perim, holeDepth);

            if (holeBaseMaterial != null)
            {
                var obj = CreateChild("HoleBase");
                obj.transform.localPosition = new Vector3(0f, -holeDepth, 0f);
                AssignMesh(obj, polyMesh, holeBaseMaterial);
            }

            Material wallMat = holeSidesMaterial != null ? holeSidesMaterial : holeBaseMaterial;
            if (wallMat != null)
            {
                var obj = CreateChild("HoleWalls");
                obj.transform.localPosition = Vector3.zero;
                AssignMesh(obj, wallMesh, wallMat);
            }

            if (holeMaskMaterial != null)
            {
                var obj = CreateChild("HoleMask");
                obj.transform.localPosition = Vector3.zero;
                AssignMesh(obj, polyMesh, holeMaskMaterial);
            }

            EditorUtility.SetDirty(gameObject);
        }

        private GameObject CreateChild(string childName)
        {
            var obj = new GameObject(childName);
            obj.transform.SetParent(transform, false);
            obj.transform.localRotation = Quaternion.identity;
            return obj;
        }

        private static void AssignMesh(GameObject obj, Mesh mesh, Material mat)
        {
            var mf = obj.AddComponent<MeshFilter>();
            var mr = obj.AddComponent<MeshRenderer>();
            mf.sharedMesh = mesh;
            mr.sharedMaterial = mat;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;
        }
#endif

        // ---------------------------------------------------------------------
        // Spline sampling
        // ---------------------------------------------------------------------

        private static Vector2[] SamplePerimeter(UnityEngine.Splines.Spline spline, int n)
        {
            var pts = new Vector2[n];
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / n;
                float3 p = SplineUtility.EvaluatePosition(spline, t);
                pts[i] = new Vector2(p.x, p.z);
            }
            return pts;
        }

        // ---------------------------------------------------------------------
        // Mesh builders
        // ---------------------------------------------------------------------

        /// <summary>Flat polygon mesh lying in the XZ plane at y=0 local.</summary>
        private static Mesh BuildPolygonMesh(Vector2[] perim)
        {
            int n = perim.Length;
            var mesh = new Mesh { name = "SplineHolePoly" };

            var verts = new Vector3[n];
            for (int i = 0; i < n; i++)
                verts[i] = new Vector3(perim[i].x, 0f, perim[i].y);

            // winding: -1 = CW indices → front face points +Y (upward)
            List<int> tris = Triangulate(perim, winding: -1);

            mesh.vertices = verts;
            mesh.triangles = tris.ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        /// <summary>
        /// Open tube with inward-facing normals, from y=0 (surface) to y=-depth.
        /// Visible when looking down into the hole.
        /// </summary>
        private static Mesh BuildWallMesh(Vector2[] perim, float depth)
        {
            int n = perim.Length;
            var mesh = new Mesh { name = "SplineHoleWall" };

            var verts = new Vector3[n * 4];
            var normals = new Vector3[n * 4];
            var tris = new int[n * 6];

            for (int i = 0; i < n; i++)
            {
                int next = (i + 1) % n;
                Vector2 a = perim[i];
                Vector2 b = perim[next];

                float dx = b.x - a.x, dz = b.y - a.y;
                float len = Mathf.Sqrt(dx * dx + dz * dz);
                if (len > 1e-5f) { dx /= len; dz /= len; }
                var inward = new Vector3(-dz, 0f, dx);

                int v = i * 4;
                verts[v]     = new Vector3(a.x, 0f,     a.y);
                verts[v + 1] = new Vector3(b.x, 0f,     b.y);
                verts[v + 2] = new Vector3(b.x, -depth, b.y);
                verts[v + 3] = new Vector3(a.x, -depth, a.y);

                normals[v] = normals[v + 1] = normals[v + 2] = normals[v + 3] = inward;

                int t = i * 6;
                tris[t]     = v;
                tris[t + 1] = v + 3;
                tris[t + 2] = v + 1;
                tris[t + 3] = v + 1;
                tris[t + 4] = v + 3;
                tris[t + 5] = v + 2;
            }

            mesh.vertices = verts;
            mesh.normals = normals;
            mesh.triangles = tris;
            mesh.RecalculateBounds();
            return mesh;
        }

        // ---------------------------------------------------------------------
        // Ear-clipping triangulator (mirrors SplineIsland)
        // ---------------------------------------------------------------------

        private static List<int> Triangulate(Vector2[] polygon, int winding)
        {
            var result = new List<int>();
            var indices = new List<int>(polygon.Length);
            for (int i = 0; i < polygon.Length; i++) indices.Add(i);

            if (SignedArea(polygon) < 0f) indices.Reverse();

            int maxIter = polygon.Length * polygon.Length + 8;
            int iter = 0;

            while (indices.Count > 3 && iter++ < maxIter)
            {
                bool earFound = false;
                for (int i = 0; i < indices.Count; i++)
                {
                    int a = indices[(i - 1 + indices.Count) % indices.Count];
                    int b = indices[i];
                    int c = indices[(i + 1) % indices.Count];

                    if (!IsEar(polygon, indices, a, b, c)) continue;

                    if (winding == 1) { result.Add(a); result.Add(b); result.Add(c); }
                    else              { result.Add(a); result.Add(c); result.Add(b); }

                    indices.RemoveAt(i);
                    earFound = true;
                    break;
                }
                if (!earFound) break;
            }

            if (indices.Count == 3)
            {
                if (winding == 1) { result.Add(indices[0]); result.Add(indices[1]); result.Add(indices[2]); }
                else              { result.Add(indices[0]); result.Add(indices[2]); result.Add(indices[1]); }
            }

            return result;
        }

        private static bool IsEar(Vector2[] poly, List<int> indices, int a, int b, int c)
        {
            if (Cross2D(poly[a], poly[b], poly[c]) <= 0f) return false;
            foreach (int idx in indices)
            {
                if (idx == a || idx == b || idx == c) continue;
                if (PointInTriangle(poly[idx], poly[a], poly[b], poly[c])) return false;
            }
            return true;
        }

        private static float Cross2D(Vector2 a, Vector2 b, Vector2 c)
            => (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x);

        private static bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
        {
            float d1 = Cross2D(a, b, p);
            float d2 = Cross2D(b, c, p);
            float d3 = Cross2D(c, a, p);
            return !((d1 < 0 || d2 < 0 || d3 < 0) && (d1 > 0 || d2 > 0 || d3 > 0));
        }

        private static float SignedArea(Vector2[] poly)
        {
            float area = 0f;
            for (int i = 0; i < poly.Length; i++)
            {
                Vector2 a = poly[i], b = poly[(i + 1) % poly.Length];
                area += (b.x - a.x) * (b.y + a.y);
            }
            return -area * 0.5f;
        }
    }
}
#endif
