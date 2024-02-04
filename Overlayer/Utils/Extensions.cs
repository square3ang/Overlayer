using JSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using CompressionLevel = System.IO.Compression.CompressionLevel;

namespace Overlayer.Utils
{
    public static class Extensions
    {
        public static readonly Regex RichTagBreaker = new Regex(@"<(color|material|quad|size)=(.|\n)*?>|<\/(color|material|quad|size)>|<(b|i)>|<\/(b|i)>", RegexOptions.Compiled | RegexOptions.Multiline);
        public static string BreakRichTag(this string s) => RichTagBreaker.Replace(s, string.Empty);
        public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> kvp, out TKey key, out TValue value)
        {
            key = kvp.Key;
            value = kvp.Value;
        }
        public static double Round(this double value, int digits = -1) => digits < 0 ? value : Math.Round(value, digits);
        public static double Round(this float value, int digits = -1) => digits < 0 ? value : Math.Round(value, digits);
        public static bool IfTrue(this bool b, Action a)
        {
            if (b) a();
            return b;
        }
        public static string ToStringN(this JsonNode node)
        {
            if (node == null) return null;
            return node.Value;
        }
        /// <summary>
        /// For Avoid Warning
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public static async void Await(this Task task)
        {
            await task;
        }
        public static Vector2 WithRelativeX(this Vector2 vector, float x)
        {
            return new Vector2(vector.x + x, vector.y);
        }
        public static Vector2 WithRelativeY(this Vector2 vector, float y)
        {
            return new Vector2(vector.x, vector.y + y);
        }
        public static Vector3 WithRelativeX(this Vector3 vector, float x)
        {
            return new Vector3(vector.x + x, vector.y, vector.z);
        }
        public static Vector3 WithRelativeY(this Vector3 vector, float y)
        {
            return new Vector3(vector.x, vector.y + y, vector.z);
        }
        public static Vector3 WithRelativeZ(this Vector3 vector, float z)
        {
            return new Vector3(vector.x, vector.y, vector.z + z);
        }
        public static JsonNode IfNotExist(this JsonNode node, JsonNode other) => node == null ? other : node;
        public static byte[] Compress(this byte[] data)
        {
            using (MemoryStream output = new MemoryStream())
            {
                using (DeflateStream dstream = new DeflateStream(output, CompressionLevel.Optimal))
                    dstream.Write(data, 0, data.Length);
                return output.ToArray();
            }
        }
        public static byte[] Decompress(this byte[] data)
        {
            using (MemoryStream input = new MemoryStream(data))
            {
                using (MemoryStream output = new MemoryStream())
                {
                    using (DeflateStream dstream = new DeflateStream(input, CompressionMode.Decompress))
                        dstream.CopyTo(output);
                    return output.ToArray();
                }
            }
        }
    }
}
