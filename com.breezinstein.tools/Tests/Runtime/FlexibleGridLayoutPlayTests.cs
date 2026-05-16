using System.Collections;
using Breezinstein.Tools.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Breezinstein.Tools.Tests.PlayMode
{
    public class FlexibleGridLayoutPlayTests
    {
        private GameObject root;

        [TearDown]
        public void TearDown()
        {
            if (root != null) Object.Destroy(root);
        }

        private FlexibleGridLayout BuildGrid(int childCount, Vector2 parentSize,
            FlexibleGridLayout.FitType fitType, int rows = 0, int columns = 0)
        {
            root = new GameObject("FlexibleGridRoot", typeof(RectTransform));
            var rect = root.GetComponent<RectTransform>();
            rect.sizeDelta = parentSize;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);

            var layout = root.AddComponent<FlexibleGridLayout>();
            layout.fitType = fitType;
            layout.rows = rows;
            layout.columns = columns;
            layout.spacing = Vector2.zero;

            for (int i = 0; i < childCount; i++)
            {
                var child = new GameObject($"Cell_{i}", typeof(RectTransform));
                child.transform.SetParent(root.transform, false);
            }
            return layout;
        }

        [UnityTest]
        public IEnumerator UniformFit_FourChildren_Produces2x2GridWithEqualCells()
        {
            var layout = BuildGrid(4, new Vector2(200, 200), FlexibleGridLayout.FitType.UNIFORM);

            yield return null;
            LayoutRebuilder.ForceRebuildLayoutImmediate(layout.GetComponent<RectTransform>());
            yield return null;

            Assert.AreEqual(2, layout.rows);
            Assert.AreEqual(2, layout.columns);
            Assert.AreEqual(100f, layout.cellSize.x, 0.01f);
            Assert.AreEqual(100f, layout.cellSize.y, 0.01f);
        }

        [UnityTest]
        public IEnumerator FixedColumnsFit_DerivesRowsFromChildCount()
        {
            var layout = BuildGrid(7, new Vector2(300, 600),
                FlexibleGridLayout.FitType.FIXEDCOLUMNS, columns: 3);

            yield return null;
            LayoutRebuilder.ForceRebuildLayoutImmediate(layout.GetComponent<RectTransform>());
            yield return null;

            // 7 children across 3 columns => ceil(7/3) = 3 rows.
            Assert.AreEqual(3, layout.rows);
            Assert.AreEqual(3, layout.columns);
        }

        [UnityTest]
        public IEnumerator UniformFit_AssignsDistinctAnchoredPositions_PerCell()
        {
            var layout = BuildGrid(4, new Vector2(200, 200), FlexibleGridLayout.FitType.UNIFORM);

            yield return null;
            LayoutRebuilder.ForceRebuildLayoutImmediate(layout.GetComponent<RectTransform>());
            yield return null;

            var positions = new System.Collections.Generic.HashSet<Vector2>();
            foreach (Transform child in layout.transform)
            {
                var rt = (RectTransform)child;
                positions.Add(new Vector2(
                    Mathf.Round(rt.anchoredPosition.x * 100f) / 100f,
                    Mathf.Round(rt.anchoredPosition.y * 100f) / 100f));
            }

            Assert.AreEqual(4, positions.Count,
                "Each cell should occupy a distinct anchored position after layout rebuild.");
        }
    }
}
