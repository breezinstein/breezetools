using NUnit.Framework;

namespace Breezinstein.Tools.Inventory.Tests
{
    [TestFixture]
    public class InventorySlotOperationsTests
    {
        private InventoryItemData potion;
        private InventoryItemData scroll;

        [SetUp]
        public void SetUp()
        {
            potion = InventoryTestUtility.CreateItem("Potion", maxStack: 10);
            scroll = InventoryTestUtility.CreateItem("Scroll", maxStack: 5);
        }

        [TearDown]
        public void TearDown()
        {
            InventoryTestUtility.Destroy(potion);
            InventoryTestUtility.Destroy(scroll);
        }

        [Test]
        public void SwapSlots_DifferentItems_SwapsContents()
        {
            var inv = new Inventory(3);
            inv.Add(potion, 5);
            inv.Add(scroll, 3);

            inv.SwapSlots(0, 1);

            Assert.AreEqual(scroll.Id, inv.GetSlot(0).ItemId);
            Assert.AreEqual(potion.Id, inv.GetSlot(1).ItemId);
        }

        [Test]
        public void MoveSlot_OntoEmpty_TransfersStack()
        {
            var inv = new Inventory(3);
            inv.Add(potion, 5);

            inv.MoveSlot(0, 2);

            Assert.IsNull(inv.GetSlot(0));
            Assert.AreEqual(5, inv.GetSlot(2).Quantity);
        }

        [Test]
        public void MoveSlot_OntoSameItem_MergesUpToMaxStack()
        {
            var inv = new Inventory(3);
            inv.Add(potion, 6);
            inv.Add(potion, 6); // 10 in slot 0, 2 in slot 1

            Assert.AreEqual(10, inv.GetSlot(0).Quantity);
            Assert.AreEqual(2, inv.GetSlot(1).Quantity);

            inv.MoveSlot(1, 0);

            Assert.AreEqual(10, inv.GetSlot(0).Quantity);
            Assert.AreEqual(2, inv.GetSlot(1).Quantity, "Should remain because slot 0 was already full.");
        }

        [Test]
        public void MoveSlot_OntoSameItem_FullyMergesWhenSpaceAvailable()
        {
            var inv = new Inventory(3);
            inv.Add(potion, 3);
            inv.GetSlot(0); // sanity
            // Manually place into slot 1 by adding more
            inv.Add(potion, 4); // tops up slot 0 to 7

            // Now both go to slot 0 — split intentionally
            inv.SplitStack(0, 2, 3);
            Assert.AreEqual(4, inv.GetSlot(0).Quantity);
            Assert.AreEqual(3, inv.GetSlot(2).Quantity);

            inv.MoveSlot(2, 0);

            Assert.AreEqual(7, inv.GetSlot(0).Quantity);
            Assert.IsNull(inv.GetSlot(2));
        }

        [Test]
        public void SplitStack_SplitsRequestedQuantityToEmptySlot()
        {
            var inv = new Inventory(3);
            inv.Add(potion, 8);

            bool ok = inv.SplitStack(0, 2, 3);

            Assert.IsTrue(ok);
            Assert.AreEqual(5, inv.GetSlot(0).Quantity);
            Assert.AreEqual(3, inv.GetSlot(2).Quantity);
            Assert.AreEqual(potion.Id, inv.GetSlot(2).ItemId);
        }

        [Test]
        public void SplitStack_RefusesToEmptySource()
        {
            var inv = new Inventory(3);
            inv.Add(potion, 5);

            // amount equals stack size -> would leave source empty, should be rejected
            bool ok = inv.SplitStack(0, 1, 5);
            Assert.IsFalse(ok);
            Assert.AreEqual(5, inv.GetSlot(0).Quantity);
            Assert.IsNull(inv.GetSlot(1));
        }

        [Test]
        public void SplitStack_RefusesIntoOccupiedSlot()
        {
            var inv = new Inventory(3);
            inv.Add(potion, 5);
            inv.Add(scroll, 1);

            bool ok = inv.SplitStack(0, 1, 2);
            Assert.IsFalse(ok);
            Assert.AreEqual(5, inv.GetSlot(0).Quantity);
        }

        [Test]
        public void Resize_Shrinking_DropsItemsThatNoLongerFit()
        {
            var inv = new Inventory(3);
            inv.Add(potion, 10); // slot 0 full
            inv.Add(scroll, 3);  // slot 1
            inv.Add(potion, 3);  // slot 2 (slot 0 is full so it spills here)

            Assert.AreEqual(3, inv.GetSlot(2).Quantity);

            var dropped = inv.Resize(2);

            Assert.AreEqual(2, inv.Capacity);
            Assert.AreEqual(1, dropped.Count);
            Assert.AreEqual(potion.Id, dropped[0].ItemId);
            Assert.AreEqual(3, dropped[0].Quantity);
        }

        [Test]
        public void Compact_PushesEmptiesToTheEnd()
        {
            var inv = new Inventory(4);
            inv.Add(potion, 5);
            inv.Add(scroll, 2);
            inv.Remove(potion, 5); // slot 0 now empty, slot 1 still has scroll

            inv.Compact();

            Assert.AreEqual(scroll.Id, inv.GetSlot(0).ItemId);
            Assert.IsNull(inv.GetSlot(1));
        }

        [Test]
        public void SortByName_OrdersAlphabetically()
        {
            var inv = new Inventory(3);
            inv.Add(scroll, 1);
            inv.Add(potion, 1);

            inv.SortByName();

            Assert.AreEqual(potion.Id, inv.GetSlot(0).ItemId);
            Assert.AreEqual(scroll.Id, inv.GetSlot(1).ItemId);
        }
    }
}
