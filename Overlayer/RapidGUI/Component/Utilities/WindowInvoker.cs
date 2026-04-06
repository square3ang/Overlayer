using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RapidGUI;

public static class WindowInvoker {
    private static readonly HashSet<IDoGUIWindow> Windows = [];

    static WindowInvoker() {
        RapidGUIBehaviour.Instance.onGUI += DoGUI;
    }

    public static void Add(IDoGUIWindow window) {
        Windows.Add(window);
    }

    public static void Remove(IDoGUIWindow window) {
        Windows.Remove(window);
    }

    private static IDoGUIWindow focusedWindow;

    public static void SetFocusedWindow(IDoGUIWindow window) {
        focusedWindow = window;
    }

    private static void DoGUI() {
        Windows.ToList().ForEach(l => l?.DoGUIWindow());

        var evt = Event.current;

        if (evt.type == EventType.KeyUp
            && evt.keyCode == RapidGUIBehaviour.Instance.closeFocusedWindowKey
            && GUIUtility.keyboardControl == 0
           )
            if (Windows.Contains(focusedWindow)) {
                focusedWindow.CloseWindow();
                focusedWindow = null;
            }

        if (Event.current.type == EventType.Repaint) Windows.Clear();
    }
}