using System;
using System.Reflection;

namespace Overlayer.Core.Scripting.JSNet;

public static class StringHelper {
    private static readonly double[] dPow = GetDoublePow();

    private static readonly float[] fPow = GetFloatPow();

    public static readonly MethodInfo TInt8 = typeof(StringHelper).GetMethod("ToInt8");

    public static readonly MethodInfo TInt16 = typeof(StringHelper).GetMethod("ToInt16");

    public static readonly MethodInfo TInt32 = typeof(StringHelper).GetMethod("ToInt32");

    public static readonly MethodInfo TInt64 = typeof(StringHelper).GetMethod("ToInt64");

    public static readonly MethodInfo TUInt8 = typeof(StringHelper).GetMethod("ToUInt8");

    public static readonly MethodInfo TUInt16 = typeof(StringHelper).GetMethod("ToUInt16");

    public static readonly MethodInfo TUInt32 = typeof(StringHelper).GetMethod("ToUInt32");

    public static readonly MethodInfo TUInt64 = typeof(StringHelper).GetMethod("ToUInt64");

    public static readonly MethodInfo TFloat = typeof(StringHelper).GetMethod("ToFloat");

    public static readonly MethodInfo TDouble = typeof(StringHelper).GetMethod("ToDouble");

    public static readonly MethodInfo TEnum = typeof(StringHelper).GetMethod("ToEnum");

    public static readonly MethodInfo TObject = typeof(StringHelper).GetMethod("ToObject");

    public static readonly MethodInfo TBool = typeof(StringHelper).GetMethod("ToBoolean");

    public static readonly MethodInfo FInt8 = typeof(StringHelper).GetMethod("FromInt8");

    public static readonly MethodInfo FInt16 = typeof(StringHelper).GetMethod("FromInt16");

    public static readonly MethodInfo FInt32 = typeof(StringHelper).GetMethod("FromInt32");

    public static readonly MethodInfo FInt64 = typeof(StringHelper).GetMethod("FromInt64");

    public static readonly MethodInfo FUInt8 = typeof(StringHelper).GetMethod("FromUInt8");

    public static readonly MethodInfo FUInt16 = typeof(StringHelper).GetMethod("FromUInt16");

    public static readonly MethodInfo FUInt32 = typeof(StringHelper).GetMethod("FromUInt32");

    public static readonly MethodInfo FUInt64 = typeof(StringHelper).GetMethod("FromUInt64");

    public static readonly MethodInfo FFloat = typeof(StringHelper).GetMethod("FromFloat");

    public static readonly MethodInfo FDouble = typeof(StringHelper).GetMethod("FromDouble");

    public static readonly MethodInfo FEnum = typeof(StringHelper).GetMethod("FromEnum");

    public static readonly MethodInfo FObject = typeof(StringHelper).GetMethod("FromObject");

    public static readonly MethodInfo FBool = typeof(StringHelper).GetMethod("FromBoolean");

    public unsafe static sbyte ToInt8(string s) {
        if(s == null || s.Length == 0) {
            return 0;
        }
        sbyte b = 0;
        bool flag = s[0] == '-';
        fixed(char* ptr = s) {
            char* ptr2 = ptr;
            if(flag) {
                ptr2++;
            }
            for(; *ptr2 != 0; ptr2++) {
                b = (sbyte)((10 * b) + (*ptr2 - 48));
            }
        }
        return flag ? (sbyte)-b : b;
    }

    public static string FromInt8(sbyte s) => s.ToString();

    public unsafe static short ToInt16(string s) {
        if(s == null || s.Length == 0) {
            return 0;
        }
        short num = 0;
        bool flag = s[0] == '-';
        fixed(char* ptr = s) {
            char* ptr2 = ptr;
            if(flag) {
                ptr2++;
            }
            for(; *ptr2 != 0; ptr2++) {
                num = (short)((10 * num) + (*ptr2 - 48));
            }
        }
        return flag ? (short)-num : num;
    }

    public static string FromInt16(short s) => s.ToString();

    public unsafe static int ToInt32(string s) {
        if(s == null || s.Length == 0) {
            return 0;
        }
        int num = 0;
        bool flag = s[0] == '-';
        fixed(char* ptr = s) {
            char* ptr2 = ptr;
            if(flag) {
                ptr2++;
            }
            for(; *ptr2 != 0; ptr2++) {
                num = (10 * num) + (*ptr2 - 48);
            }
        }
        return flag ? -num : num;
    }

    public static string FromInt32(int s) => s.ToString();

    public unsafe static long ToInt64(string s) {
        if(s == null || s.Length == 0) {
            return 0L;
        }
        long num = 0L;
        bool flag = s[0] == '-';
        fixed(char* ptr = s) {
            char* ptr2 = ptr;
            if(flag) {
                ptr2++;
            }
            for(; *ptr2 != 0; ptr2++) {
                num = (10 * num) + (*ptr2 - 48);
            }
        }
        return flag ? -num : num;
    }

    public static string FromInt64(long s) => s.ToString();

    public unsafe static byte ToUInt8(string s) {
        if(s == null || s.Length == 0) {
            return 0;
        }
        byte b = 0;
        fixed(char* ptr = s) {
            for(char* ptr2 = ptr; *ptr2 != 0; ptr2++) {
                b = (byte)((10 * b) + (*ptr2 - 48));
            }
        }
        return b;
    }

    public static string FromUInt8(byte s) => s.ToString();

    public unsafe static ushort ToUInt16(string s) {
        if(s == null || s.Length == 0) {
            return 0;
        }
        ushort num = 0;
        fixed(char* ptr = s) {
            for(char* ptr2 = ptr; *ptr2 != 0; ptr2++) {
                num = (ushort)((10 * num) + (*ptr2 - 48));
            }
        }
        return num;
    }

    public static string FromUInt16(ushort s) => s.ToString();

    public unsafe static uint ToUInt32(string s) {
        if(s == null || s.Length == 0) {
            return 0u;
        }
        uint num = 0u;
        fixed(char* ptr = s) {
            for(char* ptr2 = ptr; *ptr2 != 0; ptr2++) {
                num = (uint)((10 * num) + (*ptr2 - 48));
            }
        }
        return num;
    }

    public static string FromUInt32(uint s) => s.ToString();

    public unsafe static ulong ToUInt64(string s) {
        if(s == null || s.Length == 0) {
            return 0uL;
        }
        ulong num = 0uL;
        fixed(char* ptr = s) {
            for(char* ptr2 = ptr; *ptr2 != 0; ptr2++) {
                num = (10 * num) + (ulong)((long)*ptr2 - 48L);
            }
        }
        return num;
    }

    public static string FromUInt64(ulong s) => s.ToString();

    public unsafe static double ToDouble(string s) {
        if(s == null || s.Length == 0) {
            return 0.0;
        }
        double num = 0.0;
        bool flag = false;
        int num2 = 1;
        bool flag2 = s[0] == '-';
        fixed(char* ptr = s) {
            char* ptr2 = ptr;
            if(flag2) {
                ptr2++;
            }
            for(; *ptr2 != 0; ptr2++) {
                if(*ptr2 == '.') {
                    flag = true;
                } else {
                    num = flag ? (num + ((double)(*ptr2 - 48) / dPow[num2++])) : ((10.0 * num) + (double)(*ptr2 - 48));
                }
            }
        }
        return flag2 ? 0.0 - num : num;
    }

    public static string FromDouble(double s) => s.ToString();

    public unsafe static float ToFloat(string s) {
        if(s == null || s.Length == 0) {
            return 0f;
        }
        float num = 0f;
        bool flag = false;
        int num2 = 1;
        bool flag2 = s[0] == '-';
        fixed(char* ptr = s) {
            char* ptr2 = ptr;
            if(flag2) {
                ptr2++;
            }
            for(; *ptr2 != 0; ptr2++) {
                if(*ptr2 == '.') {
                    flag = true;
                } else {
                    num = flag ? (num + ((float)(*ptr2 - 48) / fPow[num2++])) : ((10f * num) + (float)(*ptr2 - 48));
                }
            }
        }
        return flag2 ? 0f - num : num;
    }

    public static string FromFloat(float s) => s.ToString();

    private static double[] GetDoublePow() {
        int num = 309;
        double[] array = new double[num];
        for(int i = 0; i < num; i++) {
            array[i] = Math.Pow(10.0, i);
        }
        return array;
    }

    private static float[] GetFloatPow() {
        int num = 39;
        float[] array = new float[num];
        for(int i = 0; i < num; i++) {
            array[i] = (float)Math.Pow(10.0, i);
        }
        return array;
    }

    public static T ToEnum<T>(string s) where T : Enum => EnumParser<T>.Parse(s);

    public static string FromEnum<T>(T e) where T : Enum => e.ToString();

    public static bool ToBoolean(string s) => s.Equals("true", StringComparison.OrdinalIgnoreCase);

    public static string FromBoolean(bool b) => b.ToString();

    public static MethodInfo GetToConverter(Type numType) {
        if(numType == typeof(sbyte)) {
            return TInt8;
        }
        if(numType == typeof(short)) {
            return TInt16;
        }
        if(numType == typeof(int)) {
            return TInt32;
        }
        if(numType == typeof(long)) {
            return TInt64;
        }
        if(numType == typeof(byte)) {
            return TUInt8;
        }
        if(numType == typeof(ushort)) {
            return TUInt16;
        }
        return numType == typeof(uint)
            ? TUInt32
            : numType == typeof(ulong)
            ? TUInt64
            : numType == typeof(float)
            ? TFloat
            : numType == typeof(double)
            ? TDouble
            : numType == typeof(bool) ? TBool : typeof(Enum).IsAssignableFrom(numType) ? TEnum.MakeGenericMethod(numType) : null;
    }

    public static MethodInfo GetFromConverter(Type numType) {
        if(numType == typeof(sbyte)) {
            return FInt8;
        }
        if(numType == typeof(short)) {
            return FInt16;
        }
        if(numType == typeof(int)) {
            return FInt32;
        }
        if(numType == typeof(long)) {
            return FInt64;
        }
        if(numType == typeof(byte)) {
            return FUInt8;
        }
        if(numType == typeof(ushort)) {
            return FUInt16;
        }
        return numType == typeof(uint)
            ? FUInt32
            : numType == typeof(ulong)
            ? FUInt64
            : numType == typeof(float)
            ? FFloat
            : numType == typeof(double)
            ? FDouble
            : numType == typeof(bool) ? FBool : typeof(Enum).IsAssignableFrom(numType) ? FEnum.MakeGenericMethod(numType) : FObject;
    }

    public static string FromObject(object s) => s?.ToString();

    public static string ToObject(string s) => s;
}
