using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;

namespace Breezinstein.Tools.Tests
{
    [TestFixture]
    public class NameGenTests
    {
        // NameGen has a private static List<string> nameList that's normally populated
        // from a Resources/namelists folder. We seed it directly so the tests don't
        // require any project assets.
        private static readonly FieldInfo NameListField = typeof(NameGen).GetField(
            "nameList", BindingFlags.NonPublic | BindingFlags.Static);

        private object savedList;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            Assert.IsNotNull(NameListField, "Expected private static field 'nameList' on NameGen.");
        }

        [SetUp]
        public void SetUp()
        {
            savedList = NameListField.GetValue(null);
        }

        [TearDown]
        public void TearDown()
        {
            NameListField.SetValue(null, savedList);
        }

        private static void SeedNames(params string[] names)
        {
            NameListField.SetValue(null, new List<string>(names));
        }

        [Test]
        public void GenerateRandomSingleName_ReturnsValueFromSeededList()
        {
            SeedNames("Alpha", "Beta", "Gamma");
            string name = NameGen.GenerateRandomSingleName;
            CollectionAssert.Contains(new[] { "Alpha", "Beta", "Gamma" }, name);
        }

        [Test]
        public void GenerateRandomSingleName_StripsNonAsciiDiacritics()
        {
            // 'café' contains 'é' (non-ASCII). Normalize-FormKD + ASCII filter should yield 'cafe'.
            SeedNames("café");
            string name = NameGen.GenerateRandomSingleName;
            Assert.AreEqual("cafe", name);
        }

        [Test]
        public void GenerateRandomDoubleName_ContainsExactlyOneSpace()
        {
            SeedNames("Alpha", "Beta");
            string name = NameGen.GenerateRandomDoubleName;
            int spaces = 0;
            foreach (char c in name) if (c == ' ') spaces++;
            Assert.AreEqual(1, spaces, $"Expected 'First Last' format, got '{name}'.");
            string[] parts = name.Split(' ');
            CollectionAssert.Contains(new[] { "Alpha", "Beta" }, parts[0]);
            CollectionAssert.Contains(new[] { "Alpha", "Beta" }, parts[1]);
        }

        [Test]
        public void GenerateRandomUsername_IsLowercaseAndEndsWithDigits()
        {
            SeedNames("Alpha");
            string username = NameGen.GenerateRandomUsername;

            StringAssert.StartsWith("alpha", username);
            Assert.Greater(username.Length, "alpha".Length, "Expected trailing digits.");
            string suffix = username.Substring("alpha".Length);
            foreach (char c in suffix)
            {
                Assert.IsTrue(char.IsDigit(c), $"Expected digits only in suffix, got '{suffix}'.");
            }
            int number = int.Parse(suffix);
            Assert.GreaterOrEqual(number, 0);
            Assert.LessOrEqual(number, 998);
        }

        [Test]
        public void GenerateRandomDoubleUsername_ConcatenatesTwoNamesPlusDigits()
        {
            SeedNames("Alpha");
            string username = NameGen.GenerateRandomDoubleUsername;

            StringAssert.StartsWith("alphaalpha", username);
            string suffix = username.Substring("alphaalpha".Length);
            Assert.Greater(suffix.Length, 0);
            foreach (char c in suffix) Assert.IsTrue(char.IsDigit(c));
        }
    }
}
