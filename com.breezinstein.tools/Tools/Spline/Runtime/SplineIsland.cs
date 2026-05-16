#if BREEZINSTEIN_HAS_SPLINES
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace Breezinstein.Tools.Spline
{
    /// <summary>
    /// Generates a solid extruded mesh from a closed <see cref="SplineContainer"/> on the
    /// same GameObject. Supports taper, optional rounded top bevel, slope-aware smoothing,
    /// and an automatic <see cref="MeshCollider"/>.
    /// <para>
    /// Setup:
    /// <list type="number">
    ///   <item>Place this component on the same GameObject as your SplineContainer.</item>
    ///   <item>Adjust Height, Sample Count, Top Scale, and Bottom Scale.</item>
    ///   <item>Right-click the component header and choose "Generate Mesh",
    ///         or enable Auto Update to regenerate whenever a value changes in the Inspector.</item>
    /// </list>
    /// </para>
    /// </summary>
    [ExecuteInEditMode]
    [RequireComponent(typeof(SplineContainer))]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class SplineIsland : MonoBehaviour
    {
        [Tooltip("Height of the extrusion (upward from Y = baseY).")]
        [SerializeField] private float height = 0.5f;

        [Tooltip("Y position of the bottom face.")]
        [SerializeField] private float baseY = 0f;

        [Tooltip("Number of vertices sampled around the perimeter. Higher = smoother curves.")]
        [SerializeField] [Range(8, 256)] private int sampleCount = 64;

        [Header("Taper")]
        [Tooltip("Scale of the top perimeter relative to the centroid. 1 = no taper, 0 = point.")]
        [SerializeField] private float topScale = 1f;
        [Tooltip("Scale of the bottom perimeter relative to the centroid. 1 = no taper, 0 = point.")]
        [SerializeField] private float bottomScale = 1f;

        [Header("Bevel (Top)")]
        [Tooltip("Radius of the rounded edge between the top face and the sides. " +
                 "Eats into Height — total mesh height stays constant. 0 = no bevel.")]
        [SerializeField] [Min(0f)] private float bevelSize = 0f;

        [Tooltip("Subdivisions across the bevel arc. 0 = no bevel, 1 = chamfer, 2-6 = rounded cozy look.")]
        [SerializeField] [Range(0, 8)] private int bevelSegments = 3;

        [Header("Shading")]
        [Tooltip("Corners sharper than this angle get a hard normal break; shallower corners are smoothed. 0 = fully hard, 180 = fully smooth.")]
        [SerializeField] [Range(0f, 180f)] private float smoothingAngle = 60f;

        [Header("Collider")]
        [Tooltip("Add a MeshCollider using the generated mesh.")]
        [SerializeField] private bool generateCollider = true;

        [Tooltip("Regenerate automatically when Inspector values change (editor only).")]
        [SerializeField] private bool autoUpdate = false;

        [Header("Info")]
        [SerializeField] private int lastTriangleCount;

        // ---------------------------------------------------------------------
        // Entry points
        // ---------------------------------------------------------------------

        [ContextMenu("Generate Mesh")]
        public void GenerateMesh()
        {
            SplineContainer sc = GetComponent<SplineContainer>();
            MeshFilter mf = GetComponent<MeshFilter>();

            if (sc == null || sc.Splines == null || sc.Splines.Count == 0)
            {
                Debug.LogWarning("SplineIsland: No spline found on this GameObject.");
                return;
            }

            UnityEngine.Splines.Spline spline = sc.Splines[0];
            int n = Mathf.Max(3, sampleCount);

            // Sample and normalise to CCW (Unity: CCW polygon → CW tri winding = front face for +Y)
            Vector2[] perim = SamplePerimeter(spline, n);
            if (SignedArea(perim) < 0f)
                System.Array.Reverse(perim);

            Vector2 centroid = Vector2.zero;
            foreach (Vector2 p in perim) centroid += p;
            centroid /= perim.Length;

            Vector2[] perimTop = ScaleRing(perim, centroid, topScale);
            Vector2[] perimBot = ScaleRing(perim, centroid, bottomScale);

            // Reuse existing mesh to avoid leaking instances on every regenerate.
            // Per-instance name so multiple SplineIslands in the same scene
            // never share or overwrite each other's mesh.
            string meshName = $"SplineIsland_Mesh_{GetInstanceID()}";
            Mesh mesh = mf.sharedMesh;
            if (mesh == null || mesh.name != meshName)
            {
                mesh = new Mesh { name = meshName };
                mf.sharedMesh = mesh;
            }
            else
            {
                mesh.Clear();
            }

            BuildMesh(mesh, perimTop, perimBot);
            lastTriangleCount = mesh.triangles.Length / 3;

            // Ensure MeshRenderer has exactly 2 material slots (slot 0 = top/bottom, slot 1 = sides)
            MeshRenderer mr = GetComponent<MeshRenderer>();
            Material[] mats = mr.sharedMaterials;
            if (mats.Length != 2)
            {
                Material m0 = mats.Length > 0 ? mats[0] : null;
                Material m1 = mats.Length > 1 ? mats[1] : m0;
                mr.sharedMaterials = new Material[] { m0, m1 };
            }

            if (generateCollider)
            {
                MeshCollider col = GetComponent<MeshCollider>();
                if (col == null) col = gameObject.AddComponent<MeshCollider>();
                col.sharedMesh = mesh;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!autoUpdate) return;
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this != null) GenerateMesh();
            };
        }
#endif

        // ---------------------------------------------------------------------
        // Perimeter sampling
        // ---------------------------------------------------------------------

        private Vector2[] SamplePerimeter(UnityEngine.Splines.Spline spline, int n)
        {
            Vector2[] pts = new Vector2[n];
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / n;
                float3 p = SplineUtility.EvaluatePosition(spline, t);
                pts[i] = new Vector2(p.x, p.z);
            }
            return pts;
        }

        private static Vector2[] ScaleRing(Vector2[] perim, Vector2 centroid, float scale)
        {
            Vector2[] ring = new Vector2[perim.Length];
            for (int i = 0; i < perim.Length; i++)
                ring[i] = centroid + (perim[i] - centroid) * scale;
            return ring;
        }

        // ---------------------------------------------------------------------
        // Mesh construction
        // ---------------------------------------------------------------------

        private void BuildMesh(Mesh mesh, Vector2[] perimTop, Vector2[] perimBot)
        {
            int n = perimTop.Length;

            // --- Resolve bevel: it eats into Height, can't exceed half of it.
            int bSeg = Mathf.Max(0, bevelSegments);
            float bSize = Mathf.Max(0f, bevelSize);
            if (bSize > height * 0.5f) bSize = height * 0.5f;
            bool hasBevel = bSeg > 0 && bSize > 1e-5f;
            if (!hasBevel) { bSeg = 0; bSize = 0f; }

            float topY = baseY + height;
            float sideTopY = topY - bSize;

            List<Vector3> verts = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> flatTris = new List<int>(); // submesh 0: top + bottom
            List<int> sideTris = new List<int>(); // submesh 1: sides + bevel

            // --- Outward edge normals (horizontal). CCW polygon: rotate edge dir 90° CW in XZ.
            Vector3[] edgeNorm = new Vector3[n];
            for (int i = 0; i < n; i++)
            {
                int next = (i + 1) % n;
                float dx = perimTop[next].x - perimTop[i].x;
                float dz = perimTop[next].y - perimTop[i].y;
                float len = Mathf.Sqrt(dx * dx + dz * dz);
                if (len > 1e-4f) { dx /= len; dz /= len; }
                edgeNorm[i] = new Vector3(dz, 0f, -dx);
            }

            // --- Per-corner shading normals (auto-smooth, slope-aware).
            // Build the actual 3D face normal for each edge by crossing the horizontal
            // edge direction with the slope direction from top of side down to bottom.
            // When topScale == bottomScale slope is straight down and this collapses to
            // the horizontal edgeNorm. Using these eliminates the bevel-vs-side shading
            // seam that appears when topScale != bottomScale.
            Vector3[] actualEdgeNorm = new Vector3[n];
            for (int i = 0; i < n; i++)
            {
                int next = (i + 1) % n;
                Vector3 edgeVec = new Vector3(
                    perimTop[next].x - perimTop[i].x, 0f,
                    perimTop[next].y - perimTop[i].y);
                Vector3 slopeVec = new Vector3(
                    perimBot[i].x - perimTop[i].x,
                    baseY - sideTopY,
                    perimBot[i].y - perimTop[i].y);

                if (edgeVec.sqrMagnitude > 1e-8f && slopeVec.sqrMagnitude > 1e-8f)
                {
                    Vector3 fn = Vector3.Cross(edgeVec, slopeVec).normalized;
                    actualEdgeNorm[i] = Vector3.Dot(fn, edgeNorm[i]) >= 0f ? fn : -fn;
                }
                else
                {
                    actualEdgeNorm[i] = edgeNorm[i];
                }
            }

            float cosThresh = Mathf.Cos(smoothingAngle * Mathf.Deg2Rad);
            Vector3[] leftNorm = new Vector3[n];   // normal at start of edge i
            Vector3[] rightNorm = new Vector3[n];  // normal at end   of edge i
            for (int i = 0; i < n; i++)
            {
                int prev = (i - 1 + n) % n;
                int next = (i + 1) % n;

                float dotL = Vector3.Dot(actualEdgeNorm[prev], actualEdgeNorm[i]);
                leftNorm[i] = dotL >= cosThresh
                    ? (actualEdgeNorm[prev] + actualEdgeNorm[i]).normalized
                    : actualEdgeNorm[i];

                float dotR = Vector3.Dot(actualEdgeNorm[i], actualEdgeNorm[next]);
                rightNorm[i] = dotR >= cosThresh
                    ? (actualEdgeNorm[i] + actualEdgeNorm[next]).normalized
                    : actualEdgeNorm[i];
            }

            // --- Geometric corner outward + offset scale (independent of smoothing).
            // Inset along the bisector by `d * cornerOffsetScale[i]` produces a uniform
            // perpendicular inset of `d` on both adjacent edges, so they stay parallel.
            Vector2[] cornerOutXZ = new Vector2[n];
            float[] cornerOffsetScale = new float[n];
            for (int i = 0; i < n; i++)
            {
                int prev = (i - 1 + n) % n;
                Vector3 sum = edgeNorm[prev] + edgeNorm[i];
                if (sum.sqrMagnitude < 1e-8f)
                {
                    cornerOutXZ[i] = new Vector2(edgeNorm[i].x, edgeNorm[i].z);
                    cornerOffsetScale[i] = 1f;
                }
                else
                {
                    Vector3 bisector = sum.normalized;
                    float cosHalf = Vector3.Dot(bisector, edgeNorm[i]);
                    cornerOutXZ[i] = new Vector2(bisector.x, bisector.z);
                    cornerOffsetScale[i] = 1f / Mathf.Max(cosHalf, 0.1f); // clamp at very sharp corners
                }
            }

            // --- Bevel rings: ring 0 sits at sideTopY (perimTop XZ), ring bSeg sits at topY (inset by bSize).
            // When hasBevel == false: rings = 1, ring 0 at topY with no inset (used as top face directly).
            int rings = bSeg + 1;
            Vector2[][] ringPos = new Vector2[rings][];
            float[] ringY = new float[rings];
            float[] ringCos = new float[rings];
            float[] ringSin = new float[rings];
            for (int k = 0; k < rings; k++)
            {
                float theta = (bSeg == 0) ? Mathf.PI * 0.5f
                                          : (k / (float)bSeg) * Mathf.PI * 0.5f;
                float cosT = Mathf.Cos(theta);
                float sinT = Mathf.Sin(theta);
                float inset = bSize * (1f - cosT);
                float rise = bSize * sinT;

                Vector2[] r = new Vector2[n];
                for (int i = 0; i < n; i++)
                    r[i] = perimTop[i] - cornerOutXZ[i] * (inset * cornerOffsetScale[i]);

                ringPos[k] = r;
                ringY[k] = sideTopY + rise;
                ringCos[k] = cosT;
                ringSin[k] = sinT;
            }

            // v-coordinate: side strip uses [0, vSideTop], bevel uses [vSideTop, 1].
            float vSideTop = hasBevel ? (height - bSize) / height : 1f;

            // --- Side strip (submesh 1): perimBot @ baseY  →  ringPos[0] @ ringY[0]
            {
                Vector2[] topRing = ringPos[0];
                float topYring = ringY[0];
                for (int i = 0; i < n; i++)
                {
                    int next = (i + 1) % n;

                    Vector3 tl = new Vector3(topRing[i].x,    topYring, topRing[i].y);
                    Vector3 tr = new Vector3(topRing[next].x, topYring, topRing[next].y);
                    Vector3 bl = new Vector3(perimBot[i].x,    baseY,   perimBot[i].y);
                    Vector3 br = new Vector3(perimBot[next].x, baseY,   perimBot[next].y);

                    int v = verts.Count;
                    verts.AddRange(new[]   { tl, tr, br, bl });
                    normals.AddRange(new[] { leftNorm[i], rightNorm[i], rightNorm[i], leftNorm[i] });

                    float u0 = (float)i       / n;
                    float u1 = (float)(i + 1) / n;
                    uvs.AddRange(new Vector2[]
                    {
                        new Vector2(u0, vSideTop), new Vector2(u1, vSideTop),
                        new Vector2(u1, 0f),       new Vector2(u0, 0f)
                    });

                    sideTris.AddRange(new[] { v, v + 1, v + 2,  v, v + 2, v + 3 });
                }
            }

            // --- Bevel strips (submesh 1): ringPos[k] → ringPos[k+1]. Normals tilt up by θ.
            if (hasBevel)
            {
                float vRange = 1f - vSideTop;
                for (int k = 0; k < bSeg; k++)
                {
                    Vector2[] rA = ringPos[k];
                    Vector2[] rB = ringPos[k + 1];
                    float yA = ringY[k];
                    float yB = ringY[k + 1];
                    float cA = ringCos[k],     sA = ringSin[k];
                    float cB = ringCos[k + 1], sB = ringSin[k + 1];
                    float vA = vSideTop + vRange * (k       / (float)bSeg);
                    float vB = vSideTop + vRange * ((k + 1) / (float)bSeg);

                    for (int i = 0; i < n; i++)
                    {
                        int next = (i + 1) % n;

                        Vector3 bl = new Vector3(rA[i].x,    yA, rA[i].y);
                        Vector3 br = new Vector3(rA[next].x, yA, rA[next].y);
                        Vector3 tl = new Vector3(rB[i].x,    yB, rB[i].y);
                        Vector3 tr = new Vector3(rB[next].x, yB, rB[next].y);

                        // Tilt per-corner side normals up by θ so shading is continuous from
                        // the side (θ=0 → horizontal) into the top (θ=π/2 → +Y).
                        Vector3 nLA = (leftNorm[i]  * cA + Vector3.up * sA).normalized;
                        Vector3 nRA = (rightNorm[i] * cA + Vector3.up * sA).normalized;
                        Vector3 nLB = (leftNorm[i]  * cB + Vector3.up * sB).normalized;
                        Vector3 nRB = (rightNorm[i] * cB + Vector3.up * sB).normalized;

                        int v = verts.Count;
                        verts.AddRange(new[]   { tl,  tr,  br,  bl  });
                        normals.AddRange(new[] { nLB, nRB, nRA, nLA });

                        float u0 = (float)i       / n;
                        float u1 = (float)(i + 1) / n;
                        uvs.AddRange(new Vector2[]
                        {
                            new Vector2(u0, vB), new Vector2(u1, vB),
                            new Vector2(u1, vA), new Vector2(u0, vA)
                        });

                        sideTris.AddRange(new[] { v, v + 1, v + 2,  v, v + 2, v + 3 });
                    }
                }
            }

            // --- Top + bottom faces (submesh 0). UV bbox spans both rings to keep [0,1] range.
            {
                Vector2[] topRing = ringPos[bSeg];
                float topYring = ringY[bSeg];

                Vector2 uvMin = topRing[0], uvMax = topRing[0];
                for (int i = 0; i < n; i++)
                {
                    uvMin = Vector2.Min(uvMin, topRing[i]);  uvMax = Vector2.Max(uvMax, topRing[i]);
                    uvMin = Vector2.Min(uvMin, perimBot[i]); uvMax = Vector2.Max(uvMax, perimBot[i]);
                }
                Vector2 uvSize = uvMax - uvMin;
                uvSize.x = uvSize.x > 0f ? uvSize.x : 1f;
                uvSize.y = uvSize.y > 0f ? uvSize.y : 1f;

                int topBase = verts.Count;
                for (int i = 0; i < n; i++)
                {
                    verts.Add(new Vector3(topRing[i].x, topYring, topRing[i].y));
                    normals.Add(Vector3.up);
                    uvs.Add(new Vector2((topRing[i].x - uvMin.x) / uvSize.x,
                                        (topRing[i].y - uvMin.y) / uvSize.y));
                }
                foreach (int t in Triangulate(topRing, winding: -1)) flatTris.Add(topBase + t);

                int botBase = verts.Count;
                for (int i = 0; i < n; i++)
                {
                    verts.Add(new Vector3(perimBot[i].x, baseY, perimBot[i].y));
                    normals.Add(Vector3.down);
                    uvs.Add(new Vector2((perimBot[i].x - uvMin.x) / uvSize.x,
                                        (perimBot[i].y - uvMin.y) / uvSize.y));
                }
                foreach (int t in Triangulate(perimBot, winding: 1)) flatTris.Add(botBase + t);
            }

            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.SetVertices(verts);
            mesh.subMeshCount = 2;
            mesh.SetTriangles(flatTris, 0);
            mesh.SetTriangles(sideTris, 1);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.RecalculateBounds();
        }

        // ---------------------------------------------------------------------
        // Ear-clipping triangulator
        // ---------------------------------------------------------------------

        /// <summary>
        /// Returns triangle indices for the given polygon. Assumes input is CCW
        /// (normalised before calling).
        /// winding =  1 → CCW output (front-facing when viewed from -Y, i.e. bottom face)
        /// winding = -1 → CW  output (front-facing when viewed from +Y, i.e. top face)
        /// </summary>
        private List<int> Triangulate(Vector2[] polygon, int winding)
        {
            List<int> result = new List<int>();
            List<int> indices = new List<int>(polygon.Length);
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

        private bool IsEar(Vector2[] poly, List<int> indices, int a, int b, int c)
        {
            Vector2 A = poly[a], B = poly[b], C = poly[c];

            if (Cross2D(A, B, C) <= 0f) return false;

            foreach (int idx in indices)
            {
                if (idx == a || idx == b || idx == c) continue;
                if (PointInTriangle(poly[idx], A, B, C)) return false;
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
