using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Breezinstein.Tools
{
    public class DummyVibrator: IVibrator
    {
        public bool IsInitialized()
        {
            return true;
        }
        public bool HasVibrator()
        {
            return true;
        }

        public void Vibrate(long milliseconds)
        {
        }

        public void VibrateNope()
        {
        }

        public void VibratePeek()
        {
        }

        public void VibratePop()
        {
        }

        void IVibrator.Init()
        {
        }

        void IVibrator.Vibrate(long[] pattern, int repeat)
        {
        }
    }
}
