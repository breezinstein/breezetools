using NUnit.Framework;

namespace Breezinstein.Tools.Tests
{
    [TestFixture]
    public class VibrationManagerTests
    {
        // VibrationManager keeps static state; remember and restore it between tests.
        private bool savedEnabled;

        [SetUp]
        public void SetUp()
        {
            savedEnabled = VibrationManager.VibrateEnabled;
            VibrationManager.VibrateEnabled = true;
        }

        [TearDown]
        public void TearDown()
        {
            VibrationManager.VibrateEnabled = savedEnabled;
        }

        [Test]
        public void HasVibrator_InEditor_UsesDummyAndReturnsTrue()
        {
            // In editor, Init() picks the DummyVibrator which always reports a vibrator.
            Assert.IsTrue(VibrationManager.HasVibrator());
        }

        [Test]
        public void CanVibrate_WhenEnabled_ReturnsTrue()
        {
            VibrationManager.VibrateEnabled = true;
            Assert.IsTrue(VibrationManager.CanVibrate());
        }

        [Test]
        public void CanVibrate_WhenDisabled_ReturnsFalse()
        {
            VibrationManager.VibrateEnabled = false;
            Assert.IsFalse(VibrationManager.CanVibrate());
        }

        [Test]
        public void Vibrate_DoesNotThrow_WhenEnabled()
        {
            VibrationManager.VibrateEnabled = true;
            Assert.DoesNotThrow(() => VibrationManager.Vibrate(50));
            Assert.DoesNotThrow(VibrationManager.VibratePop);
            Assert.DoesNotThrow(VibrationManager.VibratePeek);
            Assert.DoesNotThrow(VibrationManager.VibrateNope);
        }

        [Test]
        public void Vibrate_DoesNotThrow_WhenDisabled()
        {
            VibrationManager.VibrateEnabled = false;
            // Disabled path early-outs via CanVibrate() and must still be safe to call.
            Assert.DoesNotThrow(() => VibrationManager.Vibrate(50));
        }

        [Test]
        public void DummyVibrator_DirectInterface_ReportsReady()
        {
            IVibrator dummy = new DummyVibrator();
            dummy.Init();
            Assert.IsTrue(dummy.IsInitialized());
            Assert.IsTrue(dummy.HasVibrator());
            Assert.DoesNotThrow(() => dummy.Vibrate(10));
            Assert.DoesNotThrow(() => dummy.Vibrate(new long[] { 0, 100, 50, 100 }, -1));
            Assert.DoesNotThrow(dummy.VibratePop);
            Assert.DoesNotThrow(dummy.VibratePeek);
            Assert.DoesNotThrow(dummy.VibrateNope);
        }
    }
}
