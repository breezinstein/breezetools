using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Breezinstein.Tools
{
#if UNITY_ANDROID
    public class AndroidVibrator : IVibrator
    {
        public static AndroidJavaClass unityPlayer;
        public static AndroidJavaObject currentActivity;
        public static AndroidJavaObject vibrator;
        public static AndroidJavaObject context;
        public static AndroidJavaClass vibrationEffect;

        private bool initialized = false;

        public bool IsInitialized()
        {
            return initialized;
        }
        public void Init()
        {
            if (initialized) return;

            unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            vibrator = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");
            context = currentActivity.Call<AndroidJavaObject>("getApplicationContext");

            if (AndroidVersion >= 26)
            {
                vibrationEffect = new AndroidJavaClass("android.os.VibrationEffect");
            }


        }

        public bool HasVibrator()
        {
            AndroidJavaClass contextClass = new AndroidJavaClass("android.content.Context");
            string Context_VIBRATOR_SERVICE = contextClass.GetStatic<string>("VIBRATOR_SERVICE");
            AndroidJavaObject systemService = context.Call<AndroidJavaObject>("getSystemService", Context_VIBRATOR_SERVICE);
            if (systemService.Call<bool>("hasVibrator"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static void Vibrate()
        {
            Handheld.Vibrate();
        }

        public void Vibrate(long milliseconds)
        {
            if (AndroidVersion >= 26)
            {
                AndroidJavaObject createOneShot = vibrationEffect.CallStatic<AndroidJavaObject>("createOneShot", milliseconds, -1);
                vibrator.Call("vibrate", createOneShot);

            }
            else
            {
                vibrator.Call("vibrate", milliseconds);
            }
        }

        public void Vibrate(long[] pattern, int repeat)
        {
            if (AndroidVersion >= 26)
            {
                AndroidJavaObject createWaveform = vibrationEffect.CallStatic<AndroidJavaObject>("createWaveform", pattern, repeat);
                vibrator.Call("vibrate", createWaveform);
            }
            else
            {
                vibrator.Call("vibrate", pattern, repeat);
            }
        }

        public void VibrateNope()
        {
            long[] pattern = { 0, 50, 50, 50 };
            Vibrate(pattern, -1);
        }

        public void VibratePeek()
        {
            Vibrate(100);
        }

        public void VibratePop()
        {
            Vibrate(50);
        }

        public void Cancel()
        {
            vibrator.Call("cancel");
        }

        public static int AndroidVersion
        {
            get
            {
                int iVersionNumber = 0;
                if (Application.platform == RuntimePlatform.Android)
                {
                    string androidVersion = SystemInfo.operatingSystem;
                    int sdkPos = androidVersion.IndexOf("API-");
                    iVersionNumber = int.Parse(androidVersion.Substring(sdkPos + 4, 2).ToString());
                }
                return iVersionNumber;
            }
        }
    }
#endif
}
