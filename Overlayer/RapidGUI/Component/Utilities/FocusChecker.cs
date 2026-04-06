using UnityEngine;

namespace RapidGUI;

public static class ForcusChecker {
    private static int time;
    private static int mouseId;
    private static int keyboardId;
    private static bool changed;

    public static bool IsChanged() {
        if (time != Time.frameCount) {
            time = Time.frameCount;

            var currentMouse = GUIUtility.hotControl;
            var currentKeyboard = GUIUtility.keyboardControl;

            changed = keyboardId != currentKeyboard || mouseId != currentMouse;
            if (changed) {
                keyboardId = currentKeyboard;
                mouseId = currentMouse;
            }
        }

        return changed;
    }
}