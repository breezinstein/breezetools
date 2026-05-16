using System.Collections;
using Crystal;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Breezinstein.Tools.Tests.PlayMode
{
    public class SafeAreaPlayTests
    {
        private GameObject host;

        [SetUp]
        public void SetUp()
        {
            SafeArea.Sim = SafeArea.SimDevice.None;
        }

        [TearDown]
        public void TearDown()
        {
            SafeArea.Sim = SafeArea.SimDevice.None;
            if (host != null) Object.Destroy(host);
        }

        [UnityTest]
        public IEnumerator Awake_WithoutSim_AppliesFullScreenAnchors()
        {
            host = new GameObject("SafeAreaHost", typeof(RectTransform));
            host.AddComponent<SafeArea>();

            yield return null;
            yield return null;

            var rt = host.GetComponent<RectTransform>();

            // Screen.safeArea in editor is the full game-view rect, so anchors should be (0,0)-(1,1).
            Assert.AreEqual(0f, rt.anchorMin.x, 0.001f);
            Assert.AreEqual(0f, rt.anchorMin.y, 0.001f);
            Assert.AreEqual(1f, rt.anchorMax.x, 0.001f);
            Assert.AreEqual(1f, rt.anchorMax.y, 0.001f);
        }

        [UnityTest]
        public IEnumerator Awake_WithIPhoneXSim_AppliesNonFullScreenAnchors()
        {
            SafeArea.Sim = SafeArea.SimDevice.iPhoneX;

            host = new GameObject("SafeAreaHost", typeof(RectTransform));
            host.AddComponent<SafeArea>();

            yield return null;
            yield return null;

            var rt = host.GetComponent<RectTransform>();
            bool portrait = Screen.height > Screen.width;
            float expectedYRatio = portrait ? 102f / 2436f : 63f / 1125f;

            Assert.AreEqual(expectedYRatio, rt.anchorMin.y, 0.01f,
                "anchorMin.y should match the simulated iPhone X safe-area inset.");
            Assert.AreNotEqual(0f, rt.anchorMin.y,
                "Simulated safe area should not leave the panel at full-screen anchors.");
        }

        [UnityTest]
        public IEnumerator Refresh_PropagatesScreenChanges_BetweenFrames()
        {
            host = new GameObject("SafeAreaHost", typeof(RectTransform));
            host.AddComponent<SafeArea>();

            yield return null;

            // Flipping Sim mid-life should be picked up by the per-frame Refresh.
            SafeArea.Sim = SafeArea.SimDevice.iPhoneXsMax;
            yield return null;
            yield return null;

            var rt = host.GetComponent<RectTransform>();
            Assert.AreNotEqual(0f, rt.anchorMin.y,
                "Per-frame Refresh should re-apply anchors after Sim changes.");
        }
    }
}
