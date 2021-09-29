using UnityEngine;
using System;
using System.Text;
using Newtonsoft.Json;

public static class BreezeHelper
{
    public static string Serialize<T>(this T toSerialize)
    {
        //return JsonUtility.ToJson(toSerialize);
        return JsonConvert.SerializeObject(toSerialize);
    }

    public static T Deserialize<T>(this string toDeSerialize)
    {
        return JsonConvert.DeserializeObject<T>(toDeSerialize);
    }

	/// <summary>
    ///Fibonacci number at n in sequence
    /// </summary>
    /// <param name="n">nth number</param>
    /// <returns></returns>
    public static int Fib(int n)
    {
        return (n < 2) ? n : Fib(n - 1) + Fib(n - 2);
    }

    public static string RemoveSpecialCharacters(string str)
    {
        StringBuilder sb = new StringBuilder();
        foreach (char c in str)
        {
            if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '.' || c == '_')
            {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }

    static string Md5Sum(string strToEncrypt)
    {
        byte[] asciiBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(strToEncrypt);
        byte[] hashedBytes = System.Security.Cryptography.MD5CryptoServiceProvider.Create().ComputeHash(asciiBytes);
        string hashedString = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
        return hashedString;
    }
}
