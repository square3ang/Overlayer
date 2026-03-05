using Overlayer;
using Overlayer.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RapidGUI;

public static partial class RGUI {
    static int popupControlId;
    static readonly PopupWindow popupWindow = new();

    public static string SelectionPopup(string current, string[] displayOptions) {
        var idx = Array.IndexOf(displayOptions, current);
        GUILayout.Box(current, RGUIStyle.alignLeftBox);
        var newIdx = PopupOnLastRect(idx, displayOptions);
        if(newIdx != idx) {
            current = displayOptions[newIdx];
        }
        return current;
    }

    public static int SelectionPopup(int selectionIndex, string[] displayOptions) {
        var label = (selectionIndex < 0 || displayOptions.Length <= selectionIndex)
            ? ""
            : displayOptions[selectionIndex];
        GUILayout.Box(label, RGUIStyle.alignLeftBox);
        return PopupOnLastRect(selectionIndex, displayOptions);
    }

    public static int SelectionPopup(int selectionIndex, string[] displayOptions,
        Dictionary<string, string> tooltips = null, params GUILayoutOption[] options) {
        var label = (selectionIndex < 0 || displayOptions.Length <= selectionIndex)
            ? ""
            : displayOptions[selectionIndex];
        GUILayout.Box(label, RGUIStyle.alignLeftBox, options);
        return PopupOnLastRect(selectionIndex, displayOptions, -1, "", tooltips);
    }

    public static string SelectionPopup(string current, string[] displayOptions, Texture2D[] images) {
        var idx = Array.IndexOf(displayOptions, current);
        var image = images != null && idx < images.Length ? images[idx] : null;
        int newIdx;
        GUILayout.BeginHorizontal();
        GUILayout.Space(2);
        GUILayout.Label(image, GUILayout.Width(14));
        GUILayout.Box(current, RGUIStyle.alignLeftBox);
        GUILayout.EndHorizontal();
        newIdx = PopupOnLastRect(idx, displayOptions, images);

        if(newIdx != idx) {
            current = displayOptions[newIdx];
        }
        return current;
    }

    public static int SelectionPopup(int selectionIndex, string[] displayOptions, Texture2D[] images) {
        if(selectionIndex < 0 || displayOptions.Length <= selectionIndex) {
            GUILayout.Box("", RGUIStyle.alignLeftBox);
        } else {
            var image = images != null && selectionIndex < images.Length ? images[selectionIndex] : null;
            GUILayout.BeginHorizontal();
            GUILayout.Space(2);
            GUILayout.Label(image, GUILayout.Width(14));
            GUILayout.Box(displayOptions[selectionIndex], RGUIStyle.alignLeftBox);
            GUILayout.EndHorizontal();
        }

        return PopupOnLastRect(selectionIndex, displayOptions, images);
    }

    public static int SelectionPopup(int selectionIndex, string[] displayOptions, Texture2D[] images,
        Dictionary<string, string> tooltips = null, params GUILayoutOption[] options) {
        if(selectionIndex < 0 || displayOptions.Length <= selectionIndex) {
            GUILayout.Box("", RGUIStyle.alignLeftBox);
        } else {
            var image = images != null && selectionIndex < images.Length ? images[selectionIndex] : null;
            GUILayout.BeginHorizontal();
            GUILayout.Space(2);
            GUILayout.Label(image, GUILayout.Width(14));
            GUILayout.Box(displayOptions[selectionIndex], RGUIStyle.alignLeftBox);
            GUILayout.EndHorizontal();
        }

        return PopupOnLastRect(selectionIndex, displayOptions, images, -1, "", tooltips);
    }

    public static int PopupOnLastRect(string[] displayOptions, string label = "") =>
        PopupOnLastRect(-1, displayOptions, -1, label);

    public static int PopupOnLastRect(string[] displayOptions, int button, string label = "") =>
        PopupOnLastRect(-1, displayOptions, button, label);

    public static int PopupOnLastRect(string[] displayOptions, Texture2D[] images, string label = "") =>
        PopupOnLastRect(-1, displayOptions, images, -1, label);

    public static int PopupOnLastRect(string[] displayOptions, Texture2D[] images, int button, string label = "") =>
        PopupOnLastRect(-1, displayOptions, images, button, label);

    public static int PopupOnLastRect(int selectionIndex, string[] displayOptions, int mouseButton = -1,
        string label = "", Dictionary<string, string> tooltips = null) => Popup(GUILayoutUtility.GetLastRect(),
        mouseButton, selectionIndex, displayOptions, null,
        label, tooltips);

    public static int PopupOnLastRect(int selectionIndex, string[] displayOptions, Texture2D[] images, int mouseButton = -1,
       string label = "", Dictionary<string, string> tooltips = null) => Popup(GUILayoutUtility.GetLastRect(),
       mouseButton, selectionIndex, displayOptions, images,
       label, tooltips);

    public static int Popup(Rect launchRect, int mouseButton, int selectionIndex, string[] displayOptions, Texture2D[] images = null,
        string label = "", Dictionary<string, string> tooltips = null) {
        var ret = selectionIndex;
        var controlId = GUIUtility.GetControlID(FocusType.Passive);

        // not Popup Owner
        if(popupControlId != controlId) {
            var ev = Event.current;
            var pos = ev.mousePosition;

            if((ev.type == EventType.MouseUp)
                && ((mouseButton < 0) || (ev.button == mouseButton))
                && launchRect.Contains(pos)
                && displayOptions != null
                && displayOptions.Any()
               ) {
                popupWindow.pos = RGUIUtility.GetMouseScreenPos(Vector2.one * 150f);
                popupControlId = controlId;
                ev.Use();
            }
        }
        // Active
        else {
            var type = Event.current.type;

            var result = popupWindow.result;
            if(result.HasValue && type == EventType.Layout) {
                if(result.Value >= 0) // -1 when the popup is closed by clicking outside the window
                {
                    ret = result.Value;
                }

                popupWindow.result = null;
                popupControlId = 0;
            } else {
                if(type is EventType.Layout or EventType.Repaint) {
                    var buttonStyle = RGUIStyle.popupFlatButton;
                    var contentSize = Vector2.zero;
                    for(var i = 0; i < displayOptions.Length; ++i) {
                        var textSize = buttonStyle.CalcSize(RGUIUtility.TempContent(displayOptions[i]));
                        contentSize.x = Mathf.Max(contentSize.x, textSize.x);
                        contentSize.y += textSize.y;
                    }

                    var margin = buttonStyle.margin;
                    contentSize.y += Mathf.Max(0, displayOptions.Length - 1) *
                                     Mathf.Max(margin.top, margin.bottom); // is this right?

                    var vbarSkin = GUI.skin.verticalScrollbar;
                    var vbarSize = vbarSkin.CalcScreenSize(Vector2.zero);
                    var vbarMargin = vbarSkin.margin;

                    var hbarSkin = GUI.skin.horizontalScrollbar;
                    var hbarSize = hbarSkin.CalcScreenSize(Vector2.zero);
                    var hbarMargin = hbarSkin.margin;

                    const float offset = 5f;
                    contentSize +=
                        new Vector2(vbarSize.x + vbarMargin.horizontal, hbarSize.y + hbarMargin.vertical) +
                        (Vector2.one * offset);
                    var size = RGUIStyle.popup.CalcScreenSize(contentSize);
                    var maxSize = new Vector2(Screen.width, Screen.height) - popupWindow.pos;

                    popupWindow.size = Vector2.Min(size, maxSize);
                }

                popupWindow.label = label;
                popupWindow.displayOptions = displayOptions;
                popupWindow.images = images;
                popupWindow.tooltips = tooltips;
                PopupWindow.isOpen = true;
                WindowInvoker.Add(popupWindow);
            }
        }

        return ret;
    }

    public class PopupWindow : IDoGUIWindow {
        public string label;
        public Vector2 pos;
        public Vector2 size;
        public int? result;
        public string[] displayOptions;
        public Texture2D[] images;
        public Dictionary<string, string> tooltips;
        public Vector2 scrollPosition;

        public static bool showTooltip = false;
        public static string tooltip = "";
        public static bool isOpen = false;

        static readonly int PopupWindowId = "Popup".GetHashCode();

        public Rect GetWindowRect() => new(pos, size);

        public void DoGUIWindow() {
            var npopup = new GUIStyle(RGUIStyle.popup);
            npopup.normal.background = Texture2D.blackTexture;
            npopup.hover.background = Texture2D.blackTexture;
            npopup.padding = new RectOffset(1000, 1000, 100, 100);
            var wrect = GetWindowRect();
            wrect.x -= 1000;
            wrect.y -= 100;
            wrect.width += 2000;
            wrect.height += 200;
            GUI.ModalWindow(PopupWindowId, wrect, (id) => {
                var rc = new Rect(new Vector2(1000, 100), GetWindowRect().size);
                GUI.Box(rc, "", RGUIStyle.popup);
                showTooltip = false;

                var bakv = GUI.skin.verticalScrollbar.normal.background;
                var bakvt = GUI.skin.verticalScrollbarThumb.normal.background;

                if(!Main.Settings.useLegacyTheme) {
                    GUI.skin.verticalScrollbar.normal.background = Drawer.jittengray;

                    GUI.skin.verticalScrollbarThumb.normal.background = Drawer.gray;

                }

                using(var sc = new GUILayout.ScrollViewScope(scrollPosition)) {
                    scrollPosition = sc.scrollPosition;

                    for(var j = 0; j < displayOptions.Length; ++j) {

                        if(GUILayout.Button(displayOptions[j], RGUIStyle.popupFlatButton)) {
                            result = j;
                            isOpen = false;
                        }

                        Rect lastRect = GUILayoutUtility.GetLastRect();

                        if(tooltips != null && lastRect
                            .Contains(Event.current.mousePosition) &&
                            tooltips.TryGetValue(displayOptions[j], out var tooltip)) {
                            showTooltip = true;
                            PopupWindow.tooltip = tooltip;
                        }

                        var image = images != null && j < images.Length ? images[j] : null;

                        if(image != null) {
                            lastRect.x += 5;
                            lastRect.width = image.width * 4;
                            lastRect.height = image.height * 4;

                            GUI.Label(lastRect, image);
                        }
                    }
                }

                GUI.skin.verticalScrollbar.normal.background = bakv;
                GUI.skin.verticalScrollbarThumb.normal.background = bakvt;

                var ev = Event.current;
                if((ev.rawType == EventType.MouseDown) &&
                    !rc.Contains(ev.mousePosition)) {
                    result = -1;
                    isOpen = false;
                }

                if(showTooltip) {
                    Drawer.Tooltip(tooltip, true);
                }
            }
                , label, npopup);
        }

        public void CloseWindow() {
            result = -1;
            isOpen = false;
        }
    }
}