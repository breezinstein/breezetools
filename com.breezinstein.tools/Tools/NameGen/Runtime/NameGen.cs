using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Breezinstein.Tools
{
    /// <summary>
    /// Static class for generating random names.
    /// </summary>
    public static class NameGen
    {
        private static List<string> nameList;

        /// <summary>
        /// Loads text assets from the "namelists" resource folder and populates the nameList.
        /// </summary>
        private static void LoadTextAssets()
        {
            nameList = new List<string>();
            TextAsset[] textFiles = Resources.LoadAll<TextAsset>("namelists");
            foreach (TextAsset item in textFiles)
            {
                nameList.AddRange(item.text.Split('\n'));
            }
        }

        /// <summary>
        /// Generates a random single name.
        /// </summary>
        public static string GenerateRandomSingleName
        {
            get
            {
                if (nameList == null)
                {
                    LoadTextAssets();
                }
                return LatinToAscii(nameList[Random.Range(0, nameList.Count)]);
            }
        }

        /// <summary>
        /// Generates a random double name.
        /// </summary>
        public static string GenerateRandomDoubleName
        {
            get
            {
                string result = string.Format("{0} {1}", GenerateRandomSingleName, GenerateRandomSingleName);
                return result;
            }
        }

        /// <summary>
        /// Generates a random username.
        /// </summary>
        public static string GenerateRandomUsername
        {
            get
            {
                int appendInt = Random.Range(0, 999);
                string result = string.Format("{0}{1}", GenerateRandomSingleName.ToLower(), appendInt);
                return result;

            }
        }

        /// <summary>
        /// Generates a random double username.
        /// </summary>
        public static string GenerateRandomDoubleUsername
        {
            get
            {
                int appendInt = Random.Range(0, 999);
                string result = string.Format("{0}{1}{2}", GenerateRandomSingleName.ToLower(), GenerateRandomSingleName.ToLower(), appendInt);
                return result;
            }
        }

        /// <summary>
        /// Converts a string from Latin to ASCII.
        /// </summary>
        /// <param name="inString">The string to convert.</param>
        /// <returns>The converted string.</returns>
        private static string LatinToAscii(string inString)
        {
            StringBuilder newStringBuilder = new StringBuilder();
            newStringBuilder.Append(inString.Normalize(NormalizationForm.FormKD)
                                            .Where(x => x < 128)
                                            .ToArray());
            return newStringBuilder.ToString();
        }
    }
}