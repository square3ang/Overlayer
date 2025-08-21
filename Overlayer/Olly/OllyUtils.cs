namespace Overlayer.Olly {
    public static class OllyUtils {
        public static void InitLanguage() {
            isKorean = Main.Lang.CurrentLanguage == "한국어";
        }
        private static bool isKorean = false;
        public static string Tr(string en, string ko) {
            return isKorean ? ko : en;
        }
    }
}
