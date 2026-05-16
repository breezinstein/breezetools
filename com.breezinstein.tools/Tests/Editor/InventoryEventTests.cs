using NUnit.Framework;

namespace Breezinstein.Tools.Inventory.Tests
{
    [TestFixture]
    public class InventoryEventTests
    {
        private InventoryItemData potion;

        [SetUp]
        public void SetUp()
        {
            potion = InventoryTestUtility.CreateItem("Potion", maxStack: 10);
        }

        [TearDown]
        public void TearDown()
        {
            InventoryTestUtility.Destroy(potion);
        }

        [Test]
        public void Add_RaisesItemAddedWithActuallyAddedAmount()
        {
            var inv = new Inventory(1);
            int reportedAmount = 0;
            InventoryItemData reportedItem = null;
            inv.ItemAdded += (_, e) =>
            {
                reportedItem = e.Item;
                reportedAmount = e.Amount;
            };

            inv.Add(potion, 25); // capacity 1, max stack 10 -> 10 added, 15 overflow

            Assert.AreSame(potion, reportedItem);
            Assert.AreEqual(10, reportedAmount);
        }

        [Test]
        public void Add_DoesNotRaiseEventWhenNothingFits()
        {
            var inv = new Inventory(1);
            inv.Add(potion, 10);
            int callCount = 0;
            inv.ItemAdded += (_, _) => callCount++;

            inv.Add(potion, 5); // no space

            Assert.AreEqual(0, callCount);
        }

        [Test]
        public void Remove_RaisesItemRemovedWithActualAmount()
        {
            var inv = new Inventory(2);
            inv.Add(potion, 4);
            int reportedAmount = 0;
            inv.ItemRemoved += (_, e) => reportedAmount = e.Amount;

            int removed = inv.Remove(potion, 100);

            Assert.AreEqual(4, removed);
            Assert.AreEqual(4, reportedAmount);
        }

        [Test]
        public void SlotChanged_FiresOnAddAndRemove()
        {
            var inv = new Inventory(2);
            int slotChanges = 0;
            inv.SlotChanged += (_, _) => slotChanges++;

            inv.Add(potion, 1);
            Assert.GreaterOrEqual(slotChanges, 1);

            int prev = slotChanges;
            inv.Remove(potion, 1);
            Assert.Greater(slotChanges, prev);
        }

        [Test]
        public void Clear_RaisesClearedEvent()
        {
            var inv = new Inventory(2);
            inv.Add(potion, 3);
            bool cleared = false;
            inv.Cleared += (_, _) => cleared = true;

            inv.Clear();

            Assert.IsTrue(cleared);
        }
    }
}
