using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Breezinstein.Tools { 
    public interface IVibrator
    {
        void Init();
        bool IsInitialized();
        bool HasVibrator();
        void Vibrate(long milliseconds);
        void Vibrate(long[] pattern, int repeat);
        void VibratePop();
        void VibratePeek();
        void VibrateNope();
    }
}
