public static class RGUILayoutUtility {
    public static bool IsInEditorWindow() =>
#if UNITY_EDITOR
        return GUI.skin.FindStyle("ToggleMixed") != null;
#else
        false;
#endif

}