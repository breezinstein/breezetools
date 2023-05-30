using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Breezinstein.Tools
{
#if UNITY_IOS
    public class iOSVibrator : IVibrator
    {
        [DllImport("__Internal")]
        private static extern bool _HasVibrator();

        [DllImport("__Internal")]
        private static extern void _Vibrate();

        [DllImport("__Internal")]
        private static extern void _VibratePop();

        [DllImport("__Internal")]
        private static extern void _VibratePeek();

        [DllImport("__Internal")]
        private static extern void _VibrateNope();

        private bool initialized = false;

        public bool IsInitialized()
        {
            return initialized;
        }

        public bool HasVibrator()
        {
            return _HasVibrator();
        }

        public void Vibrate(long milliseconds)
        {
            Handheld.Vibrate();
        }

        public void VibrateNope()
        {
            _VibrateNope();
        }

        public void VibratePeek()
        {
            _VibratePeek();
        }

        public void VibratePop()
        {
            _VibratePop();
        }

        void IVibrator.Init()
        {
            if (initialized) return;

            initialized = true;
        }

        void IVibrator.Vibrate(long[] pattern, int repeat)
        {
            Handheld.Vibrate();
        }
    }
#endif
}
