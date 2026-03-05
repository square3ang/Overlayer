namespace Overlayer.Olly;

public static class OllyUtils {
    public static void InitLanguage() => isKorean = Main.Lang.Language == "ko-KR";
    private static bool isKorean = false;
    public static string Tr(string en, string ko) => isKorean ? ko : en;

    public static int BitIndex(int bitValue) {
        return bitValue switch {
            1 => 1,
            2 => 2,
            4 => 3,
            8 => 4,
            16 => 5,
            32 => 6,
            64 => 7,
            128 => 8,
            256 => 9,
            512 => 10,
            1024 => 11,
            2048 => 12,
            4096 => 13,
            8192 => 14,
            16384 => 15,
            32768 => 16,
            65536 => 17,
            131072 => 18,
            262144 => 19,
            524288 => 20,
            1048576 => 21,
            2097152 => 22,
            4194304 => 23,
            8388608 => 24,
            16777216 => 25,
            33554432 => 26,
            67108864 => 27,
            134217728 => 28,
            268435456 => 29,
            536870912 => 30,
            1073741824 => 31,
            _ => 0,
        };
    }
}
