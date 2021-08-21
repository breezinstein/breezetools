using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BreezeTools
{
    public static class NameGen
    {
        private static List<string> nameList;

        private static void LoadTextAssets()
        {
            nameList = new List<string>();
            TextAsset[] textFiles = Resources.LoadAll<TextAsset>("namelists");
            foreach (TextAsset item in textFiles)
            {
                nameList.AddRange(item.text.Split('\n'));
            }
        }

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

        public static string GenerateRandomDoubleName
        {
            get
            {
                string result = string.Format("{0} {1}", GenerateRandomSingleName, GenerateRandomSingleName);
                return result;
            }
        }

        public static string GenerateRandomUsername
        {
            get
            {
                int appendInt = Random.Range(0, 999);
                string result = string.Format("{0}{1}", GenerateRandomSingleName.ToLower(), appendInt);
                return result;

            }
        }

        public static string GenerateRandomDoubleUsername
        {
            get
            {
                int appendInt = Random.Range(0, 999);
                string result = string.Format("{0}{1}{2}", GenerateRandomSingleName.ToLower(), GenerateRandomSingleName.ToLower(), appendInt);
                return result;
            }
        }

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