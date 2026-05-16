using NUnit.Framework;

namespace Breezinstein.Tools.Tests
{
    [TestFixture]
    public class EventManagerTests
    {
        // EventManager<T> stores handlers in a static, generic-shared dictionary. Tests use
        // unique event names so they don't bleed into each other or leak handlers.
        private string UniqueName(string label) => $"{label}_{System.Guid.NewGuid():N}";

        [Test]
        public void Trigger_RegisteredHandler_ReceivesPayload()
        {
            string name = UniqueName("evt");
            int captured = 0;
            System.Action<int> handler = p => captured = p;

            EventManager<int>.Register(name, handler);
            try
            {
                EventManager<int>.Trigger(name, 42);
                Assert.AreEqual(42, captured);
            }
            finally
            {
                EventManager<int>.Unregister(name, handler);
            }
        }

        [Test]
        public void Trigger_MultipleHandlers_AllFire()
        {
            string name = UniqueName("evt");
            int callsA = 0, callsB = 0;
            System.Action<string> a = _ => callsA++;
            System.Action<string> b = _ => callsB++;

            EventManager<string>.Register(name, a);
            EventManager<string>.Register(name, b);
            try
            {
                EventManager<string>.Trigger(name, "hi");
                Assert.AreEqual(1, callsA);
                Assert.AreEqual(1, callsB);
            }
            finally
            {
                EventManager<string>.Unregister(name, a);
                EventManager<string>.Unregister(name, b);
            }
        }

        [Test]
        public void Unregister_RemovesOnlyTheSpecifiedHandler()
        {
            string name = UniqueName("evt");
            int callsA = 0, callsB = 0;
            System.Action<int> a = _ => callsA++;
            System.Action<int> b = _ => callsB++;

            EventManager<int>.Register(name, a);
            EventManager<int>.Register(name, b);
            EventManager<int>.Unregister(name, a);

            try
            {
                EventManager<int>.Trigger(name, 1);
                Assert.AreEqual(0, callsA);
                Assert.AreEqual(1, callsB);
            }
            finally
            {
                EventManager<int>.Unregister(name, b);
            }
        }

        [Test]
        public void Trigger_UnknownEvent_NoOp()
        {
            // No handlers registered for this name - should not throw.
            Assert.DoesNotThrow(() => EventManager<int>.Trigger(UniqueName("never"), 0));
        }

        [Test]
        public void Unregister_UnknownEvent_NoOp()
        {
            Assert.DoesNotThrow(() => EventManager<int>.Unregister(UniqueName("never"), _ => { }));
        }

        [Test]
        public void Trigger_AfterAllHandlersRemoved_DoesNotThrow()
        {
            string name = UniqueName("evt");
            System.Action<int> a = _ => { };

            EventManager<int>.Register(name, a);
            EventManager<int>.Unregister(name, a);

            Assert.DoesNotThrow(() => EventManager<int>.Trigger(name, 0));
        }

        [Test]
        public void Generic_TypeArgumentIsolatesHandlers()
        {
            string name = UniqueName("evt");
            int intCalls = 0, stringCalls = 0;
            System.Action<int> intHandler = _ => intCalls++;
            System.Action<string> stringHandler = _ => stringCalls++;

            EventManager<int>.Register(name, intHandler);
            EventManager<string>.Register(name, stringHandler);
            try
            {
                EventManager<int>.Trigger(name, 1);
                Assert.AreEqual(1, intCalls);
                Assert.AreEqual(0, stringCalls);

                EventManager<string>.Trigger(name, "x");
                Assert.AreEqual(1, intCalls);
                Assert.AreEqual(1, stringCalls);
            }
            finally
            {
                EventManager<int>.Unregister(name, intHandler);
                EventManager<string>.Unregister(name, stringHandler);
            }
        }
    }
}
