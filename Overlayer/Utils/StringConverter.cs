using System;
using System.Reflection;

namespace Overlayer.Utils;

public static class StringConverter {
    public static unsafe sbyte ToInt8(string s) {
        if (s == null || s.Length == 0) return 0;

        sbyte result = 0;
        var unary = s[0] == 45;
        fixed (char* v = s) {
            var c = v;
            if (unary) c++;

            while (*c != '\0') {
                result = (sbyte)(10 * result + (*c - 48));
                c++;
            }
        }

        return unary ? (sbyte)-result : result;
    }

    public static string FromInt8(sbyte s) {
        return s.ToString();
    }

    public static unsafe short ToInt16(string s) {
        if (s == null || s.Length == 0) return 0;

        short result = 0;
        var unary = s[0] == 45;
        fixed (char* v = s) {
            var c = v;
            if (unary) c++;

            while (*c != '\0') {
                result = (short)(10 * result + (*c - 48));
                c++;
            }
        }

        return unary ? (short)-result : result;
    }

    public static string FromInt16(short s) {
        return s.ToString();
    }

    public static unsafe int ToInt32(string s) {
        if (s == null || s.Length == 0) return 0;

        var result = 0;
        var unary = s[0] == 45;
        fixed (char* v = s) {
            var c = v;
            if (unary) c++;

            while (*c != '\0') {
                result = 10 * result + (*c - 48);
                c++;
            }
        }

        return unary ? -result : result;
    }

    public static string FromInt32(int s) {
        return s.ToString();
    }

    public static unsafe long ToInt64(string s) {
        if (s == null || s.Length == 0) return 0;

        long result = 0;
        var unary = s[0] == 45;
        fixed (char* v = s) {
            var c = v;
            if (unary) c++;

            while (*c != '\0') {
                result = 10 * result + (*c - 48);
                c++;
            }
        }

        return unary ? -result : result;
    }

    public static string FromInt64(long s) {
        return s.ToString();
    }

    public static unsafe byte ToUInt8(string s) {
        if (s == null || s.Length == 0) return 0;

        byte result = 0;
        fixed (char* v = s) {
            var c = v;
            while (*c != '\0') {
                result = (byte)(10 * result + (*c - 48));
                c++;
            }
        }

        return result;
    }

    public static string FromUInt8(byte s) {
        return s.ToString();
    }

    public static unsafe ushort ToUInt16(string s) {
        if (s == null || s.Length == 0) return 0;

        ushort result = 0;
        fixed (char* v = s) {
            var c = v;
            while (*c != '\0') {
                result = (ushort)(10 * result + (*c - 48));
                c++;
            }
        }

        return result;
    }

    public static string FromUInt16(ushort s) {
        return s.ToString();
    }

    public static unsafe uint ToUInt32(string s) {
        if (s == null || s.Length == 0) return 0;

        uint result = 0;
        fixed (char* v = s) {
            var c = v;
            while (*c != '\0') {
                result = (uint)(10 * result + (*c - 48));
                c++;
            }
        }

        return result;
    }

    public static string FromUInt32(uint s) {
        return s.ToString();
    }

    public static unsafe ulong ToUInt64(string s) {
        if (s == null || s.Length == 0) return 0;

        ulong result = 0;
        fixed (char* v = s) {
            var c = v;
            while (*c != '\0') {
                result = 10 * result + (*c - 48ul);
                c++;
            }
        }

        return result;
    }

    public static string FromUInt64(ulong s) {
        return s.ToString();
    }

    public static unsafe double ToDouble(string s) {
        if (s == null || s.Length == 0) return 0;

        double result = 0;
        var isDot = false;
        var dCount = 1;
        var unary = s[0] == 45;
        fixed (char* v = s) {
            var c = v;
            if (unary) c++;

            while (*c != '\0') {
                if (*c == '.') {
                    isDot = true;
                    goto Continue;
                }

                if (!isDot)
                    result = 10 * result + (*c - 48);
                else
                    result += (*c - 48) / dPow[dCount++];

                Continue:
                c++;
            }
        }

        return unary ? -result : result;
    }

    public static string FromDouble(double s) {
        return s.ToString();
    }

    public static unsafe float ToFloat(string s) {
        if (s == null || s.Length == 0) return 0;

        float result = 0;
        var isDot = false;
        var dCount = 1;
        var unary = s[0] == 45;
        fixed (char* v = s) {
            var c = v;
            if (unary) c++;

            while (*c != '\0') {
                if (*c == '.') {
                    isDot = true;
                    goto Continue;
                }

                if (!isDot)
                    result = 10 * result + (*c - 48);
                else
                    result += (*c - 48) / fPow[dCount++];

                Continue:
                c++;
            }
        }

        return unary ? -result : result;
    }

    public static string FromFloat(float s) {
        return s.ToString();
    }

    private static readonly double[] dPow = GetDoublePow();

    private static double[] GetDoublePow() {
        var max = 309;
        var exps = new double[max];
        for (var i = 0; i < max; i++) exps[i] = Math.Pow(10, i);

        return exps;
    }

    private static readonly float[] fPow = GetFloatPow();

    private static float[] GetFloatPow() {
        var max = 39;
        var exps = new float[max];
        for (var i = 0; i < max; i++) exps[i] = (float)Math.Pow(10, i);

        return exps;
    }

    public static T ToEnum<T>(string s) where T : Enum {
        return EnumHelper<T>.Parse(s);
    }

    public static string FromEnum<T>(T e) where T : Enum {
        return e.ToString();
    }

    public static bool ToBoolean(string s) {
        return s.Equals("true", StringComparison.OrdinalIgnoreCase);
    }

    public static string FromBoolean(bool b) {
        return b.ToString();
    }

    public static MethodInfo GetToConverter(Type numType) {
        if (numType == typeof(sbyte)) return TInt8;

        if (numType == typeof(short)) return TInt16;

        if (numType == typeof(int)) return TInt32;

        if (numType == typeof(long)) return TInt64;

        return numType == typeof(byte)
            ? TUInt8
            : numType == typeof(ushort)
                ? TUInt16
                : numType == typeof(uint)
                    ? TUInt32
                    : numType == typeof(ulong)
                        ? TUInt64
                        : numType == typeof(float)
                            ? TFloat
                            : numType == typeof(double)
                                ? TDouble
                                : numType == typeof(bool)
                                    ? TBool
                                    : typeof(Enum).IsAssignableFrom(numType)
                                        ? TEnum.MakeGenericMethod(numType)
                                        : null;
    }

    public static MethodInfo GetFromConverter(Type numType) {
        if (numType == typeof(sbyte)) return FInt8;

        if (numType == typeof(short)) return FInt16;

        if (numType == typeof(int)) return FInt32;

        if (numType == typeof(long)) return FInt64;

        return numType == typeof(byte)
            ? FUInt8
            : numType == typeof(ushort)
                ? FUInt16
                : numType == typeof(uint)
                    ? FUInt32
                    : numType == typeof(ulong)
                        ? FUInt64
                        : numType == typeof(float)
                            ? FFloat
                            : numType == typeof(double)
                                ? FDouble
                                : numType == typeof(bool)
                                    ? FBool
                                    : typeof(Enum).IsAssignableFrom(numType)
                                        ? FEnum.MakeGenericMethod(numType)
                                        : FObject;
    }

    public static string FromObject(object s) {
        return s?.ToString();
    }

    public static string ToObject(string s) {
        return s;
    }

    public static readonly MethodInfo TInt8 = typeof(StringConverter).GetMethod("ToInt8");
    public static readonly MethodInfo TInt16 = typeof(StringConverter).GetMethod("ToInt16");
    public static readonly MethodInfo TInt32 = typeof(StringConverter).GetMethod("ToInt32");
    public static readonly MethodInfo TInt64 = typeof(StringConverter).GetMethod("ToInt64");
    public static readonly MethodInfo TUInt8 = typeof(StringConverter).GetMethod("ToUInt8");
    public static readonly MethodInfo TUInt16 = typeof(StringConverter).GetMethod("ToUInt16");
    public static readonly MethodInfo TUInt32 = typeof(StringConverter).GetMethod("ToUInt32");
    public static readonly MethodInfo TUInt64 = typeof(StringConverter).GetMethod("ToUInt64");
    public static readonly MethodInfo TFloat = typeof(StringConverter).GetMethod("ToFloat");
    public static readonly MethodInfo TDouble = typeof(StringConverter).GetMethod("ToDouble");
    public static readonly MethodInfo TEnum = typeof(StringConverter).GetMethod("ToEnum");
    public static readonly MethodInfo TObject = typeof(StringConverter).GetMethod("ToObject");
    public static readonly MethodInfo TBool = typeof(StringConverter).GetMethod("ToBoolean");
    public static readonly MethodInfo FInt8 = typeof(StringConverter).GetMethod("FromInt8");
    public static readonly MethodInfo FInt16 = typeof(StringConverter).GetMethod("FromInt16");
    public static readonly MethodInfo FInt32 = typeof(StringConverter).GetMethod("FromInt32");
    public static readonly MethodInfo FInt64 = typeof(StringConverter).GetMethod("FromInt64");
    public static readonly MethodInfo FUInt8 = typeof(StringConverter).GetMethod("FromUInt8");
    public static readonly MethodInfo FUInt16 = typeof(StringConverter).GetMethod("FromUInt16");
    public static readonly MethodInfo FUInt32 = typeof(StringConverter).GetMethod("FromUInt32");
    public static readonly MethodInfo FUInt64 = typeof(StringConverter).GetMethod("FromUInt64");
    public static readonly MethodInfo FFloat = typeof(StringConverter).GetMethod("FromFloat");
    public static readonly MethodInfo FDouble = typeof(StringConverter).GetMethod("FromDouble");
    public static readonly MethodInfo FEnum = typeof(StringConverter).GetMethod("FromEnum");
    public static readonly MethodInfo FObject = typeof(StringConverter).GetMethod("FromObject");
    public static readonly MethodInfo FBool = typeof(StringConverter).GetMethod("FromBoolean");
}