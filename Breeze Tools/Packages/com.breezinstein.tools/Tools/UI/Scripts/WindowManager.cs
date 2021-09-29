using UnityEngine;
using System.Collections.Generic;

namespace Breezinstein.Tools.UI
{
    [RequireComponent(typeof(AudioSource))]
    public class WindowManager : MonoBehaviour
    {
        public static WindowManager Instance;
        public int defaultWindow = 0;
        public List<Window> windows;
        public Stack<int> navList = new Stack<int>();
        public AudioSource audioSource;
        public string buttonSelectSound;
        public string buttonCancelSound;
        Window currentOpenWindow;

        public MessaageBox messageBox;
        public bool messageBoxOpen;

        public GameObject loadingObject;
        public bool isLoading;

        public Notification notificationObject;

        void Awake()
        {
            Instance = this;
            if (audioSource == null)
            { audioSource = GetComponent<AudioSource>(); }
            audioSource.playOnAwake = false;
            isLoading = false;

            PopulateList();
        }

        void Start()
        {

            CloseAllWindowsAtStart();
            CloseMessageBox();
            OpenWindow(defaultWindow);
        }

        void Update()
        {
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                if (messageBoxOpen)
                {
                    CloseMessageBox();
                }

            }
            if (loadingObject.activeInHierarchy != isLoading)
            { loadingObject.SetActive(isLoading); }
        }

        void PopulateList()
        {
            Window[] tempwindows = GetComponentsInChildren<Window>(true);

            for (int i = 0; i < tempwindows.Length; i++)
            {
                if (windows.Contains(tempwindows[i]))
                {
                    //do nothing
                }
                else
                {
                    windows.Add(tempwindows[i]);
                }
            }

            RearrangeList();
        }

        void RearrangeList()
        {
            windows.Sort(delegate (Window a, Window b)
            {
                return (a.ID).CompareTo(b.ID);
            });
        }

        int GetWindowByID(int ID)
        {
            for (int i = 0; i < windows.Count; i++)
            {
                if (windows[i].ID == ID)
                {
                    return i;
                    //return windows [i].ID;
                }
            }
            Debug.LogError("Window ID not found");
            return 0;
        }

        int GetWindowIDFromName(string windowName)
        {
            for (int i = 0; i < windows.Count; i++)
            {
                if (windows[i].windowName == windowName)
                {
                    return windows[i].ID;
                }
            }
            Debug.LogError("Window ID not found");
            return 0;
        }

        public void OpenWindow(int windowID)
        {
            OpenWindow(windowID, false);
        }

        public void OpenWindow(string windowName)
        {
            OpenWindow(GetWindowIDFromName(windowName));
        }

        public void OpenWindow(int windowID, bool goingBack)
        {

            if (currentOpenWindow)
            {
                if (!goingBack)
                {
                    navList.Push(currentOpenWindow.ID);
                }
                currentOpenWindow.Close();
            }

            currentOpenWindow = windows[GetWindowByID(windowID)];
            currentOpenWindow.gameObject.SetActive(true);
            currentOpenWindow.PlayAnimation("Open");
            currentOpenWindow.RTrans.SetAsLastSibling();


        }

        public void CloseWindow(int windowID)
        {

            windows[GetWindowByID(windowID)].PlayAnimation("Close");
        }

        public void CloseAllWindows(int windowID)
        {
            for (int i = 0; i < windows.Count; i++)
            {
                CloseWindow(GetWindowByID(windowID));
            }

        }

        public void CloseAllWindowsAtStart()
        {
            for (int i = 0; i < windows.Count; i++)
            {
                windows[i].gameObject.SetActive(false);
            }
        }


        public void OpenPreviousWindow()
        {
            if (navList.Count > 0)
            {
                if (GetWindowByID(navList.Peek()) == GetWindowByID(currentOpenWindow.ID))
                {
                    navList.Pop();
                }
                else
                {
                    OpenWindow(GetWindowByID(navList.Pop()), true);
                }
            }
        }

        public void PlayButtonSelectSound()
        {
            //AudioManager.Instance.PlaySFX(buttonSelectSound);
            //if (VibrationManager.HasVibrator()&&GlobalVariables.SaveFile.vibrationEnabled)
            //{
            //    VibrationManager.Vibrate(20);
            //}
        }
        public void PlayButtonCancelSound()
        {
            //AudioManager.Instance.PlaySFX(buttonCancelSound);
            //if (VibrationManager.HasVibrator()&&GlobalVariables.SaveFile.vibrationEnabled)
            //{
            //    VibrationManager.Vibrate(20);
            //}
        }

        public void ClearPreviousStack()
        {
            navList.Clear();
        }
        #region MessageBox
        public void OpenMessageBox()
        {
            OpenMessageBox(new MessageTemplate("Header", "Message", "Close"));
        }

        public void OpenMessageBox(MessageTemplate messageInfo)
        {
            messageBox.messageInfo = messageInfo;
            messageBox.gameObject.SetActive(true);
            //messageBox.transform.SetAsLastSibling ();
            messageBoxOpen = true;
        }

        public void CloseMessageBox()
        {
            messageBox.gameObject.SetActive(false);
            messageBoxOpen = false;
        }
        #endregion

        #region Notification
        public void ShowNotification()
        {
            ShowNotification("Message", 2f);
        }

        public void ShowNotification(string message, float duration)
        {
            notificationObject.message = new NotificationItem();
            notificationObject.message.message = message;
            notificationObject.message.duration = duration;
            notificationObject.gameObject.SetActive(true);
        }
        #endregion

        #region Loading
        public void ShowLoading()
        {
            isLoading = true;
            loadingObject.transform.SetAsLastSibling();
        }

        public void CloseLoading()
        {
            isLoading = false;
        }
        #endregion
    }

}