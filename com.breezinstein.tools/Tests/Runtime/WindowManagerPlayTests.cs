using System.Collections;
using System.Collections.Generic;
using Breezinstein.Tools.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Breezinstein.Tools.Tests.PlayMode
{
    public class WindowManagerPlayTests
    {
        private GameObject managerGo;
        private GameObject messageBoxGo;
        private GameObject loadingGo;
        private GameObject notificationGo;
        private readonly List<GameObject> windowGos = new List<GameObject>();

        [TearDown]
        public void TearDown()
        {
            WindowManager.Instance = null;
            if (managerGo != null) Object.Destroy(managerGo);
            if (messageBoxGo != null) Object.Destroy(messageBoxGo);
            if (loadingGo != null) Object.Destroy(loadingGo);
            if (notificationGo != null) Object.Destroy(notificationGo);
            foreach (var w in windowGos) if (w != null) Object.Destroy(w);
            windowGos.Clear();
        }

        private WindowManager BuildManager(int windowCount = 2)
        {
            managerGo = new GameObject("WindowManagerUnderTest");
            managerGo.SetActive(false); // delay Awake until refs are wired

            var manager = managerGo.AddComponent<WindowManager>();
            manager.windows = new List<Window>();
            manager.defaultWindow = 0;

            // Stub MessageBox on an inactive GO so its own Start never fires; WindowManager only
            // toggles its activeSelf flag.
            messageBoxGo = new GameObject("MessageBoxStub");
            messageBoxGo.SetActive(false);
            manager.messageBox = messageBoxGo.AddComponent<MessageBox>();

            // Loading object: stays inactive; isLoading defaults to false so no flip happens.
            loadingGo = new GameObject("LoadingStub");
            loadingGo.SetActive(false);
            manager.loadingObject = loadingGo;

            notificationGo = new GameObject("NotificationStub");
            notificationGo.SetActive(false);
            manager.notificationObject = notificationGo.AddComponent<Notification>();

            for (int i = 0; i < windowCount; i++)
            {
                var go = new GameObject($"Window_{i}", typeof(RectTransform));
                go.transform.SetParent(managerGo.transform, false);
                var window = go.AddComponent<Window>();
                window.ID = i;
                window.windowName = $"window_{i}";
                manager.windows.Add(window);
                windowGos.Add(go);
            }

            managerGo.SetActive(true);
            return manager;
        }

        [UnityTest]
        public IEnumerator Start_OpensDefaultWindow_AndLeavesNavStackEmpty()
        {
            var manager = BuildManager();

            yield return null;
            yield return null;

            Assert.AreSame(manager, WindowManager.Instance);
            Assert.IsTrue(manager.windows[0].gameObject.activeSelf,
                "Default window must be active after Start.");
            Assert.AreEqual(0, manager.navList.Count,
                "Opening the default window must not push onto the nav stack.");
        }

        [UnityTest]
        public IEnumerator OpenWindow_PushesPreviousIdOntoStack()
        {
            var manager = BuildManager();
            yield return null;

            manager.OpenWindow(1);
            yield return null;

            Assert.AreEqual(1, manager.navList.Count,
                "Opening a new window must record the previous one on the nav stack.");
            Assert.AreEqual(0, manager.navList.Peek(),
                "Top of nav stack should be the previously-open window's ID.");
            Assert.IsTrue(manager.windows[1].gameObject.activeSelf,
                "Newly opened window must become active.");
        }

        [UnityTest]
        public IEnumerator OpenWindow_ByName_ResolvesToCorrectId()
        {
            var manager = BuildManager();
            yield return null;

            manager.OpenWindow("window_1");
            yield return null;

            Assert.IsTrue(manager.windows[1].gameObject.activeSelf);
            Assert.AreEqual(0, manager.navList.Peek());
        }

        [UnityTest]
        public IEnumerator OpenPreviousWindow_PopsNavStack()
        {
            var manager = BuildManager();
            yield return null;

            manager.OpenWindow(1);
            yield return null;
            manager.OpenPreviousWindow();
            yield return null;

            Assert.AreEqual(0, manager.navList.Count,
                "Going back should consume the entry it navigated to.");
            Assert.IsTrue(manager.windows[0].gameObject.activeSelf);
        }

        [UnityTest]
        public IEnumerator ClearPreviousStack_EmptiesNavList()
        {
            var manager = BuildManager(windowCount: 3);
            yield return null;

            manager.OpenWindow(1);
            yield return null;
            manager.OpenWindow(2);
            yield return null;
            Assert.AreEqual(2, manager.navList.Count);

            manager.ClearPreviousStack();
            Assert.AreEqual(0, manager.navList.Count);
        }

        [UnityTest]
        public IEnumerator CloseMessageBox_FlipsMessageBoxOpenFlag()
        {
            var manager = BuildManager();
            yield return null;

            // Simulate the flag being set without going through OpenMessageBox (which would
            // activate the stub and trigger MessageBox.Start with unwired TMP/Button fields).
            manager.messageBoxOpen = true;
            manager.CloseMessageBox();

            Assert.IsFalse(manager.messageBoxOpen);
        }

        [UnityTest]
        public IEnumerator ShowLoading_TogglesIsLoadingFlag()
        {
            var manager = BuildManager();
            yield return null;

            Assert.IsFalse(manager.isLoading);
            manager.ShowLoading();
            Assert.IsTrue(manager.isLoading);
            manager.CloseLoading();
            Assert.IsFalse(manager.isLoading);
        }
    }
}
