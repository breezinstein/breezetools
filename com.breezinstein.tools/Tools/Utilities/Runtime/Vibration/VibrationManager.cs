using System.Collections;
using UnityEngine;

namespace Breezinstein.Tools
{
    public class VibrationManager
    {
        public static bool VibrateEnabled = true;
        private static IVibrator Vibration;

        public static bool CanVibrate()
        {
            if (HasVibrator() == false)
            {
                Debug.Log("Vibration Error: No vibration on device");
                return false;
            }
            if (Vibration.IsInitialized() == false)
            {
                Vibration.Init();
            }
            if (VibrateEnabled == false)
            {
                return false;
            }
            return (Vibration.IsInitialized() && VibrateEnabled);
        }
        public static bool HasVibrator()
        {
            return Vibration.HasVibrator();
        }

        public static void Init()
        {
#if UNITY_EDITOR
            Vibration = new DummyVibrator();
#elif UNITY_IOS          
            Vibration = new iOSVibrator();
#elif UNITY_ANDROID
            Vibration = new AndroidVibrator();
#else
            Vibration = new DummyVibrator();
#endif
            Vibration.Init();
        }

        public static void ToggleVibration(bool value)
        {
            VibrateEnabled = !VibrateEnabled;
        }

        public static void Vibrate(long milliseconds)
        {
            if (CanVibrate())
            {
                Vibration.Vibrate(milliseconds);
            }
        }
        public static void VibratePop()
        {
            if (CanVibrate())
            {
                Vibration.VibratePop();
            }
        }
        public static void VibratePeek()
        {
            if (CanVibrate())
            {
                Vibration.VibratePeek();
            }
        }
        public static void VibrateNope()
        {
            if (CanVibrate())
            {
                Vibration.VibrateNope();
            }
        }
    }
}