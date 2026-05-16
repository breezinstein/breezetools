using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace Breezinstein.Tools.Tests
{
    [TestFixture]
    public class BreezeHelperTests
    {
        private class Sample
        {
            public string Name;
            public int Value;
            public Sample[] Children;
        }

        [Test]
        public void Serialize_ProducesJsonStringContainingValues()
        {
            var sample = new Sample { Name = "Alice", Value = 42 };

            string json = BreezeHelper.Serialize(sample);

            Assert.IsNotNull(json);
            StringAssert.Contains("Alice", json);
            StringAssert.Contains("42", json);
        }

        [Test]
        public void Deserialize_RoundTripsObject()
        {
            var sample = new Sample
            {
                Name = "Root",
                Value = 7,
                Children = new[]
                {
                    new Sample { Name = "Child1", Value = 1 },
                    new Sample { Name = "Child2", Value = 2 },
                },
            };

            string json = BreezeHelper.Serialize(sample);
            var loaded = json.Deserialize<Sample>();

            Assert.AreEqual(sample.Name, loaded.Name);
            Assert.AreEqual(sample.Value, loaded.Value);
            Assert.AreEqual(2, loaded.Children.Length);
            Assert.AreEqual("Child1", loaded.Children[0].Name);
            Assert.AreEqual(2, loaded.Children[1].Value);
        }

        [Test]
        public void SaveFile_ThenLoadFile_RoundTripsContent()
        {
            string key = "breeze_helper_test_" + System.Guid.NewGuid().ToString("N");
            using (var scope = new PersistentFileScope(key))
            {
                const string payload = "{\"hello\":\"world\"}";
                BreezeHelper.SaveFile(key, payload);

                Assert.IsTrue(scope.FileExists);
                Assert.IsTrue(BreezeHelper.FileExists(key));
                Assert.AreEqual(payload, BreezeHelper.LoadFile(key));
            }
        }

        [Test]
        public void FileExists_ReturnsFalseForMissingFile()
        {
            string key = "breeze_helper_missing_" + System.Guid.NewGuid().ToString("N");
            using (new PersistentFileScope(key))
            {
                Assert.IsFalse(BreezeHelper.FileExists(key));
            }
        }

        [Test]
        public void LoadFile_MissingFile_ReturnsNull()
        {
            string key = "breeze_helper_load_missing_" + System.Guid.NewGuid().ToString("N");
            using (new PersistentFileScope(key))
            {
                UnityEngine.TestTools.LogAssert.Expect(LogType.Log, new System.Text.RegularExpressions.Regex(".*does not exist.*"));
                string raw = BreezeHelper.LoadFile(key);
                Assert.IsNull(raw);
            }
        }

        [TestCase(0, 0)]
        [TestCase(1, 1)]
        [TestCase(2, 1)]
        [TestCase(3, 2)]
        [TestCase(4, 3)]
        [TestCase(5, 5)]
        [TestCase(6, 8)]
        [TestCase(7, 13)]
        [TestCase(10, 55)]
        public void Fib_MatchesKnownSequence(int n, int expected)
        {
            Assert.AreEqual(expected, BreezeHelper.Fib(n));
        }

        [Test]
        public void RemoveSpecialCharacters_KeepsLettersDigitsDotUnderscore()
        {
            string input = "Hello, World! 123_abc.txt @#$%";
            string cleaned = BreezeHelper.RemoveSpecialCharacters(input);
            Assert.AreEqual("HelloWorld123_abc.txt", cleaned);
        }

        [Test]
        public void RemoveSpecialCharacters_EmptyInput_ReturnsEmpty()
        {
            Assert.AreEqual(string.Empty, BreezeHelper.RemoveSpecialCharacters(string.Empty));
        }

        [Test]
        public void RemoveSpecialCharacters_StripsUnicode()
        {
            string input = "café";
            string cleaned = BreezeHelper.RemoveSpecialCharacters(input);
            Assert.AreEqual("caf", cleaned);
        }
    }
}
