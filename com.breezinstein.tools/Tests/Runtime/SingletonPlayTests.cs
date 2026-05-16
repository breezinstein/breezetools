using System.Collections;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Breezinstein.Tools.Tests.PlayMode
{
    public class SingletonProbe : Singleton<SingletonProbe>
    {
        public int awakeCalls;
        protected override void Awake()
        {
            awakeCalls++;
            base.Awake();
        }
    }

    public class SingletonPlayTests
    {
        [TearDown]
        public void Cleanup()
        {
            foreach (var probe in Object.FindObjectsByType<SingletonProbe>(FindObjectsSortMode.None))
            {
                Object.Destroy(probe.gameObject);
            }
            PlayTestUtil.ResetSingleton<SingletonProbe>();
        }

        [UnityTest]
        public IEnumerator Instance_LazyCreatesGameObject_OnFirstAccess()
        {
            yield return null;
            var instance = SingletonProbe.Instance;
            Assert.IsNotNull(instance);
            Assert.IsNotNull(instance.gameObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Instance_IsStable_AcrossRepeatedAccess()
        {
            yield return null;
            var first = SingletonProbe.Instance;
            var second = SingletonProbe.Instance;
            Assert.AreSame(first, second);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Instance_FindsPreExistingComponent_InsteadOfCreatingNew()
        {
            var preExisting = new GameObject("PreExistingSingleton").AddComponent<SingletonProbe>();
            yield return null;

            var resolved = SingletonProbe.Instance;
            Assert.AreSame(preExisting, resolved,
                "Instance should adopt a component already present in the scene rather than create a new GameObject.");
            yield return null;
        }

        [UnityTest]
        public IEnumerator SecondInstance_IsDestroyed_AndThrowsOnAwake()
        {
            var original = SingletonProbe.Instance;
            yield return null;

            var dup = new GameObject("DuplicateSingleton");
            dup.SetActive(false);
            var dupComponent = dup.AddComponent<SingletonProbe>();

            LogAssert.Expect(LogType.Exception,
                new Regex("Instance of .*SingletonProbe.* already exists"));

            dup.SetActive(true);

            yield return null;
            yield return null;

            Assert.IsTrue(dupComponent == null,
                "Duplicate Singleton component should be destroyed by its own Awake.");
            Assert.AreSame(original, SingletonProbe.Instance,
                "Original instance must remain authoritative after the duplicate is destroyed.");
        }
    }
}
