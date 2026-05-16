using System.Collections;
using Breezinstein.Tools.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Breezinstein.Tools.Tests.PlayMode
{
    public class RadialLayoutGroupPlayTests
    {
        private GameObject root;

        [TearDown]
        public void TearDown()
        {
            if (root != null) Object.Destroy(root);
        }

        private RadialLayoutGroup BuildLayout(int childCount, float radius, float arc,
            RadialLayoutGroup.RadialLayoutStart startFrom)
        {
            root = new GameObject("RadialRoot", typeof(RectTransform));
            var rect = root.GetComponent<RectTransform>();
            rect.position = Vector3.zero;
            rect.sizeDelta = new Vector2(400, 400);

            var layout = root.AddComponent<RadialLayoutGroup>();
            layout.Radius = radius;
            layout.Arc = arc;
            layout.Offset = 0f;
            layout.StartFrom = startFrom;

            for (int i = 0; i < childCount; i++)
            {
                var child = new GameObject($"Pip_{i}", typeof(RectTransform));
                child.transform.SetParent(root.transform, false);
            }
            return layout;
        }

        [UnityTest]
        public IEnumerator UpdateChildren_SpreadsFourChildren_AcrossCardinals_StartingTop()
        {
            var layout = BuildLayout(4, radius: 100f, arc: 360f,
                startFrom: RadialLayoutGroup.RadialLayoutStart.top);

            yield return null;
            LayoutRebuilder.ForceRebuildLayoutImmediate(layout.GetComponent<RectTransform>());
            yield return null;

            var children = new RectTransform[4];
            for (int i = 0; i < 4; i++) children[i] = (RectTransform)layout.transform.GetChild(i);

            // angleStep = 360/4 = 90. direction = up. Rotations applied around Z axis.
            var origin = layout.transform.position;
            float r = 100f;
            AssertVector(origin + new Vector3(0, r, 0), children[0].position, "i=0 / 0deg");
            AssertVector(origin + new Vector3(-r, 0, 0), children[1].position, "i=1 / 90deg");
            AssertVector(origin + new Vector3(0, -r, 0), children[2].position, "i=2 / 180deg");
            AssertVector(origin + new Vector3(r, 0, 0), children[3].position, "i=3 / 270deg");
        }

        [UnityTest]
        public IEnumerator UpdateChildren_HalfArc_KeepsChildrenInsideArcSpan()
        {
            var layout = BuildLayout(3, radius: 50f, arc: 180f,
                startFrom: RadialLayoutGroup.RadialLayoutStart.right);

            yield return null;
            LayoutRebuilder.ForceRebuildLayoutImmediate(layout.GetComponent<RectTransform>());
            yield return null;

            foreach (Transform child in layout.transform)
            {
                float distance = Vector3.Distance(child.position, layout.transform.position);
                Assert.AreEqual(50f, distance, 0.01f,
                    "Each child must sit on the configured radius.");
            }
        }

        private static void AssertVector(Vector3 expected, Vector3 actual, string label)
        {
            Assert.AreEqual(expected.x, actual.x, 0.01f, $"{label} x");
            Assert.AreEqual(expected.y, actual.y, 0.01f, $"{label} y");
            Assert.AreEqual(expected.z, actual.z, 0.01f, $"{label} z");
        }
    }
}
