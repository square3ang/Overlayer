using Overlayer.Core;
using Overlayer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection.Emit;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using CompressionLevel = System.IO.Compression.CompressionLevel;

namespace Overlayer.Utils;

public static class Extensions {
    public const string DefaultTrimStr = "..($LeftCount)";
    public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> kvp, out TKey key, out TValue value) {
        key = kvp.Key;
        value = kvp.Value;
    }
    public static double Round(this double value, int digits = -1) => digits < 0 ? value : Math.Round(value, digits);
    public static double Round(this float value, int digits = -1) => digits < 0 ? value : Math.Round(value, digits);
    public static string Trim(this string str, int maxLength = -1, string afterTrimStr = DefaultTrimStr) {
        return maxLength >= 0 && str.Length > maxLength
            ? str.Substring(0, maxLength) + afterTrimStr?.Replace("$LeftCount", StringConverter.FromInt32(str.Length - maxLength))
            : str;
    }
    public static string ToString(this double value, string format) => value.ToString(format);
    public static string ToString(this float value, string format) => value.ToString(format);
    public static string PadZero(double value, int digits) => value.ToString("F" + digits);
    public static T[] SplitParse<T>(this string str, char splitter) where T : Enum {
        string[] split = str.Split(splitter);
        return Array.ConvertAll(split, EnumHelper<T>.Parse);
    }
    public static bool Convert(this ILGenerator il, Type to) {
        switch(Type.GetTypeCode(to)) {
            case TypeCode.Char:
            case TypeCode.Int16:
                il.Emit(OpCodes.Conv_I2);
                return true;
            case TypeCode.SByte:
                il.Emit(OpCodes.Conv_I1);
                return true;
            case TypeCode.Byte:
                il.Emit(OpCodes.Conv_U1);
                return true;
            case TypeCode.UInt16:
                il.Emit(OpCodes.Conv_U2);
                return true;
            case TypeCode.Boolean:
            case TypeCode.Int32:
                il.Emit(OpCodes.Conv_I4);
                return true;
            case TypeCode.UInt32:
                il.Emit(OpCodes.Conv_U4);
                return true;
            case TypeCode.Int64:
                il.Emit(OpCodes.Conv_I8);
                return true;
            case TypeCode.UInt64:
                il.Emit(OpCodes.Conv_U8);
                return true;
            case TypeCode.Single:
                il.Emit(OpCodes.Conv_R4);
                return true;
            case TypeCode.Double:
                il.Emit(OpCodes.Conv_R8);
                return true;
            case TypeCode.String:
                il.Emit(OpCodes.Call, typeof(Convert).GetMethod("ToString", new[] { to }));
                return true;
            default:
                return false;
        }
    }
    public static GameObject MakeFlexible(this GameObject go) {
        ContentSizeFitter csf = go.GetComponent<ContentSizeFitter>() ?? go.AddComponent<ContentSizeFitter>();
        csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        return go;
    }
    public static bool Apply(this FontMeta meta, out FontData font) {
        if(FontManager.TryGetFont(meta.name, out font)) {
            font.lineSpacing = meta.lineSpacing;
            font.lineSpacingTMP = meta.lineSpacing;
            font.fontScale = meta.fontScale;
            return true;
        }
        return false;
    }
    public static bool IfTrue(this bool b, Action a) {
        if(b) {
            a();
        }

        return b;
    }
    /// <summary>
    /// For Avoid Warning
    /// </summary>
    /// <param name="task"></param>
    /// <returns></returns>
    public static async void Await(this Task task) => await task;
    public static Vector2 WithRelativeX(this Vector2 vector, float x) => new(vector.x + x, vector.y);
    public static Vector2 WithRelativeY(this Vector2 vector, float y) => new(vector.x, vector.y + y);
    public static Vector3 WithRelativeX(this Vector3 vector, float x) => new(vector.x + x, vector.y, vector.z);
    public static Vector3 WithRelativeY(this Vector3 vector, float y) => new(vector.x, vector.y + y, vector.z);
    public static Vector3 WithRelativeZ(this Vector3 vector, float z) => new(vector.x, vector.y, vector.z + z);
    public static byte[] Compress(this byte[] data) {
        using(MemoryStream output = new()) {
            using(DeflateStream dstream = new(output, CompressionLevel.Optimal)) {
                dstream.Write(data, 0, data.Length);
            }

            return output.ToArray();
        }
    }
    public static byte[] Decompress(this byte[] data) {
        using(MemoryStream input = new(data)) {
            using(MemoryStream output = new()) {
                using(DeflateStream dstream = new(input, CompressionMode.Decompress)) {
                    dstream.CopyTo(output);
                }

                return output.ToArray();
            }
        }
    }
}
