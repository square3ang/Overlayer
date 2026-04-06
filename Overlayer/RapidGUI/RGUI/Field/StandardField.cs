using System;
using Overlayer;
using Overlayer.Core;
using UnityEngine;

namespace RapidGUI;

public static partial class RGUI {
    private static readonly GUILayoutOption fieldWidthMin = GUILayout.MinWidth(80f);

    private static object StandardField(object v, Type type) {
        return StandardField(v, type, null);
    }

    private static object StandardField(object v, Type type, GUILayoutOption option) {
        var ret = v;

        var unparsedStr = UnparsedStr.Create();
        var color = unparsedStr.hasStr && !unparsedStr.CanParse(type) ? Color.red : GUI.color;

        using (new ColorScope(color)) {
            var text = unparsedStr.Get() ?? (v != null ? v.ToString() : "");
            var displayStr = GUILayout.TextField(text,
                Main.Settings.LegacyTheme ? GUI.skin.textField : Drawer.myTextField, option ?? fieldWidthMin);
            if (displayStr != text) {
                try {
                    ret = Convert.ChangeType(displayStr, type);
                    if (ret.ToString() == displayStr) displayStr = null;
                }
                catch { }

                unparsedStr.Set(displayStr);
            }
        }

        return ret;
    }
}