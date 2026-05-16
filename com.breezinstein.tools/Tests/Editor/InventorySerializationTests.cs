using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Breezinstein.Tools.Inventory.Tests
{
    [TestFixture]
    public class InventorySerializationTests
    {
        private InventoryItemData potion;
        private InventoryItemData scroll;
        private ItemDatabase database;
        private string saveKey;

        [SetUp]
        public void SetUp()
        {
            potion = InventoryTestUtility.CreateItem("Potion", maxStack: 10, id: "potion-id");
            scroll = InventoryTestUtility.CreateItem("Scroll", maxStack: 5, id: "scroll-id");
            database = InventoryTestUtility.CreateDatabase(potion, scroll);
            saveKey = "breeze_inventory_test_" + System.Guid.NewGuid().ToString("N");
        }

        [TearDown]
        public void TearDown()
        {
            InventorySerializer.DeleteSave(saveKey);
            InventoryTestUtility.Destroy(potion);
            InventoryTestUtility.Destroy(scroll);
            InventoryTestUtility.Destroy(database);
        }

        [Test]
        public void SaveAndLoad_RoundTripsContents()
        {
            var inv = new Inventory(4);
            inv.Add(potion, 7);
            inv.Add(scroll, 3);

            InventorySerializer.Save(inv, saveKey);

            var loaded = new Inventory(4);
            bool ok = InventorySerializer.Load(loaded, saveKey, database);

            Assert.IsTrue(ok);
            Assert.AreEqual(4, loaded.Capacity);
            Assert.AreEqual(7, loaded.GetCount(potion));
            Assert.AreEqual(3, loaded.GetCount(scroll));
        }

        [Test]
        public void SaveAndLoad_PreservesSlotIndices()
        {
            var inv = new Inventory(4);
            inv.Add(potion, 7); // slot 0
            inv.MoveSlot(0, 2); // slot 2

            InventorySerializer.Save(inv, saveKey);

            var loaded = new Inventory(4);
            InventorySerializer.Load(loaded, saveKey, database);

            Assert.IsNull(loaded.GetSlot(0));
            Assert.AreEqual(7, loaded.GetSlot(2).Quantity);
            Assert.AreEqual(potion.Id, loaded.GetSlot(2).ItemId);
        }

        [Test]
        public void LoadOrCreate_NoFile_ReturnsEmptyInventoryWithFallbackCapacity()
        {
            var inv = InventorySerializer.LoadOrCreate(saveKey, database, fallbackCapacity: 12);
            Assert.AreEqual(12, inv.Capacity);
            Assert.IsTrue(inv.IsEmpty);
        }

        [Test]
        public void Load_MissingItemId_SkipsSlotWithoutThrowing()
        {
            var inv = new Inventory(2);
            inv.Add(potion, 3);
            InventorySerializer.Save(inv, saveKey);

            // Database that doesn't know about 'potion'
            var partialDb = InventoryTestUtility.CreateDatabase(scroll);
            try
            {
                var loaded = new Inventory(2);
                LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex(".*"));
                bool ok = InventorySerializer.Load(loaded, saveKey, partialDb);
                Assert.IsTrue(ok);
                Assert.AreEqual(0, loaded.GetCount(potion));
                Assert.IsTrue(loaded.IsEmpty);
            }
            finally
            {
                InventoryTestUtility.Destroy(partialDb);
            }
        }
    }
}
