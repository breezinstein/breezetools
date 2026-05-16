using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Breezinstein.Tools.Tests
{
    [TestFixture]
    public class SerializableDictionaryTests
    {
        [Test]
        public void Add_StoresKeyValuePair()
        {
            var dict = new SerializableDictionary<string, int>();

            dict.Add("a", 1);

            Assert.AreEqual(1, dict.Count);
            Assert.IsTrue(dict.ContainsKey("a"));
            Assert.AreEqual(1, dict["a"]);
        }

        [Test]
        public void Add_DuplicateKey_Throws()
        {
            var dict = new SerializableDictionary<string, int> { { "a", 1 } };
            Assert.Throws<ArgumentException>(() => dict.Add("a", 2));
        }

        [Test]
        public void Indexer_AssignToNewKey_AddsEntry()
        {
            var dict = new SerializableDictionary<string, int>();

            dict["alpha"] = 10;
            dict["beta"] = 20;

            Assert.AreEqual(2, dict.Count);
            Assert.AreEqual(10, dict["alpha"]);
            Assert.AreEqual(20, dict["beta"]);
        }

        [Test]
        public void Indexer_AssignToExistingKey_OverwritesValue()
        {
            var dict = new SerializableDictionary<string, int> { { "x", 1 } };

            dict["x"] = 99;

            Assert.AreEqual(1, dict.Count);
            Assert.AreEqual(99, dict["x"]);
        }

        [Test]
        public void TryGetValue_KnownKey_ReturnsTrueAndValue()
        {
            var dict = new SerializableDictionary<string, string> { { "k", "v" } };

            bool found = dict.TryGetValue("k", out var value);

            Assert.IsTrue(found);
            Assert.AreEqual("v", value);
        }

        [Test]
        public void TryGetValue_UnknownKey_ReturnsFalseAndDefault()
        {
            var dict = new SerializableDictionary<string, int>();

            bool found = dict.TryGetValue("missing", out var value);

            Assert.IsFalse(found);
            Assert.AreEqual(default(int), value);
        }

        [Test]
        public void Remove_ExistingKey_ReturnsTrueAndShrinks()
        {
            var dict = new SerializableDictionary<string, int>
            {
                { "a", 1 }, { "b", 2 }, { "c", 3 },
            };

            bool removed = dict.Remove("b");

            Assert.IsTrue(removed);
            Assert.AreEqual(2, dict.Count);
            Assert.IsFalse(dict.ContainsKey("b"));
            // Remaining items still accessible
            Assert.AreEqual(1, dict["a"]);
            Assert.AreEqual(3, dict["c"]);
        }

        [Test]
        public void Remove_UnknownKey_ReturnsFalse()
        {
            var dict = new SerializableDictionary<string, int>();
            Assert.IsFalse(dict.Remove("nope"));
        }

        [Test]
        public void Clear_EmptiesDictionary()
        {
            var dict = new SerializableDictionary<string, int>
            {
                { "a", 1 }, { "b", 2 },
            };

            dict.Clear();

            Assert.AreEqual(0, dict.Count);
            Assert.IsFalse(dict.ContainsKey("a"));
        }

        [Test]
        public void KeysAndValues_ReturnAllEntriesInInsertionOrder()
        {
            var dict = new SerializableDictionary<string, int>
            {
                { "x", 10 }, { "y", 20 }, { "z", 30 },
            };

            CollectionAssert.AreEqual(new[] { "x", "y", "z" }, dict.Keys.ToArray());
            CollectionAssert.AreEqual(new[] { 10, 20, 30 }, dict.Values.ToArray());
        }

        [Test]
        public void Enumeration_YieldsKeyValuePairs()
        {
            var dict = new SerializableDictionary<string, int>
            {
                { "a", 1 }, { "b", 2 },
            };

            var collected = new List<KeyValuePair<string, int>>();
            foreach (var kvp in dict) collected.Add(kvp);

            Assert.AreEqual(2, collected.Count);
            Assert.AreEqual("a", collected[0].Key);
            Assert.AreEqual(1, collected[0].Value);
            Assert.AreEqual("b", collected[1].Key);
            Assert.AreEqual(2, collected[1].Value);
        }

        [Test]
        public void OnAfterDeserialize_RebuildsLookup()
        {
            // Simulate Unity's deserialization path: populate via the indexer, force
            // OnAfterDeserialize, then ensure existing lookups still work.
            var dict = new SerializableDictionary<string, int>
            {
                { "a", 1 }, { "b", 2 },
            };

            ((UnityEngine.ISerializationCallbackReceiver)dict).OnAfterDeserialize();

            Assert.IsTrue(dict.ContainsKey("a"));
            Assert.IsTrue(dict.ContainsKey("b"));
            Assert.AreEqual(2, dict["b"]);
            Assert.AreEqual(2, dict.Count);
        }

        [Test]
        public void CopyTo_FillsArrayStartingAtIndex()
        {
            var dict = new SerializableDictionary<string, int>
            {
                { "a", 1 }, { "b", 2 },
            };
            var array = new KeyValuePair<string, int>[4];

            dict.CopyTo(array, 1);

            Assert.AreEqual(default(KeyValuePair<string, int>), array[0]);
            Assert.AreEqual("a", array[1].Key);
            Assert.AreEqual("b", array[2].Key);
        }

        [Test]
        public void Contains_ChecksKeyMembership()
        {
            var dict = new SerializableDictionary<string, int> { { "a", 1 } };

            Assert.IsTrue(dict.Contains(new KeyValuePair<string, int>("a", 1)));
            // Per implementation Contains only checks the key, not the value.
            Assert.IsTrue(dict.Contains(new KeyValuePair<string, int>("a", 999)));
            Assert.IsFalse(dict.Contains(new KeyValuePair<string, int>("missing", 1)));
        }
    }
}
