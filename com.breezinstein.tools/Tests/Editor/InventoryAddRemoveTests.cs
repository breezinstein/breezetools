using NUnit.Framework;

namespace Breezinstein.Tools.Inventory.Tests
{
    [TestFixture]
    public class InventoryAddRemoveTests
    {
        private InventoryItemData potion;
        private InventoryItemData sword;

        [SetUp]
        public void SetUp()
        {
            potion = InventoryTestUtility.CreateItem("Potion", maxStack: 10);
            sword = InventoryTestUtility.CreateItem("Sword", maxStack: 1);
        }

        [TearDown]
        public void TearDown()
        {
            InventoryTestUtility.Destroy(potion);
            InventoryTestUtility.Destroy(sword);
        }

        [Test]
        public void Add_NewItem_PlacesFullAmountInFirstSlot()
        {
            var inv = new Inventory(5);

            var result = inv.Add(potion, 7);

            Assert.AreEqual(7, result.Added);
            Assert.AreEqual(0, result.Overflow);
            Assert.AreEqual(7, inv.GetCount(potion));
            Assert.AreEqual(1, inv.UsedSlots);
            Assert.AreEqual(7, inv.GetSlot(0).Quantity);
        }

        [Test]
        public void Add_FixesOriginalBug_AddingTenOfNewItemRecordsTen()
        {
            // Regression test for the original bug where Add(data, 10) only recorded 1
            // because InventoryItem's constructor always set the stack to 1.
            var inv = new Inventory(3);
            inv.Add(potion, 10);
            Assert.AreEqual(10, inv.GetCount(potion));
        }

        [Test]
        public void Add_SameItem_StacksUpToMaxThenSpillsToNextSlot()
        {
            var inv = new Inventory(3);

            inv.Add(potion, 6);
            var result = inv.Add(potion, 7);

            Assert.AreEqual(7, result.Added);
            Assert.AreEqual(0, result.Overflow);
            Assert.AreEqual(13, inv.GetCount(potion));
            Assert.AreEqual(10, inv.GetSlot(0).Quantity);
            Assert.AreEqual(3, inv.GetSlot(1).Quantity);
        }

        [Test]
        public void Add_BeyondCapacity_ReturnsOverflow()
        {
            var inv = new Inventory(2);

            var result = inv.Add(potion, 25);

            Assert.AreEqual(20, result.Added);
            Assert.AreEqual(5, result.Overflow);
            Assert.IsTrue(inv.IsFull);
        }

        [Test]
        public void Add_UnstackableItem_TakesOneSlotPerUnit()
        {
            var inv = new Inventory(4);

            var result = inv.Add(sword, 3);

            Assert.AreEqual(3, result.Added);
            Assert.AreEqual(0, result.Overflow);
            Assert.AreEqual(3, inv.UsedSlots);
            for (int i = 0; i < 3; i++)
            {
                Assert.AreEqual(1, inv.GetSlot(i).Quantity);
                Assert.AreEqual(sword.Id, inv.GetSlot(i).ItemId);
            }
        }

        [Test]
        public void Add_NonPositiveAmount_NoOp()
        {
            var inv = new Inventory(3);

            Assert.AreEqual(0, inv.Add(potion, 0).Added);
            Assert.AreEqual(0, inv.Add(potion, -5).Added);
            Assert.AreEqual(0, inv.UsedSlots);
        }

        [Test]
        public void Remove_ExactCount_LeavesEmptySlot()
        {
            var inv = new Inventory(3);
            inv.Add(potion, 5);

            int removed = inv.Remove(potion, 5);

            Assert.AreEqual(5, removed);
            Assert.AreEqual(0, inv.GetCount(potion));
            Assert.IsNull(inv.GetSlot(0));
        }

        [Test]
        public void Remove_MoreThanAvailable_RemovesOnlyWhatIsThere()
        {
            var inv = new Inventory(3);
            inv.Add(potion, 4);

            int removed = inv.Remove(potion, 100);

            Assert.AreEqual(4, removed);
            Assert.AreEqual(0, inv.GetCount(potion));
            Assert.IsTrue(inv.IsEmpty);
        }

        [Test]
        public void TryRemove_NotEnough_DoesNotMutateInventory()
        {
            var inv = new Inventory(3);
            inv.Add(potion, 3);

            bool ok = inv.TryRemove(potion, 5);

            Assert.IsFalse(ok);
            Assert.AreEqual(3, inv.GetCount(potion));
        }

        [Test]
        public void TryRemove_SpansMultipleStacks()
        {
            var inv = new Inventory(3);
            inv.Add(potion, 25);

            bool ok = inv.TryRemove(potion, 15);

            Assert.IsTrue(ok);
            Assert.AreEqual(10, inv.GetCount(potion));
        }

        [Test]
        public void Has_RespectsThreshold()
        {
            var inv = new Inventory(3);
            inv.Add(potion, 5);

            Assert.IsTrue(inv.Has(potion));
            Assert.IsTrue(inv.Has(potion, 5));
            Assert.IsFalse(inv.Has(potion, 6));
        }

        [Test]
        public void Clear_EmptiesEverySlot()
        {
            var inv = new Inventory(4);
            inv.Add(potion, 5);
            inv.Add(sword, 2);

            inv.Clear();

            Assert.IsTrue(inv.IsEmpty);
            Assert.AreEqual(0, inv.UsedSlots);
        }

        [Test]
        public void Capacity_RejectsZeroOrNegative()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new Inventory(0));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new Inventory(-3));
        }
    }
}
