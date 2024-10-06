using System;
using System.Collections.Generic;
using System.Linq;
using Overlayer;
using Overlayer.Core;
using UnityEngine;

namespace RapidGUI
{
    public static partial class RGUI
    {
        static int popupControlId;
        static readonly PopupWindow popupWindow = new PopupWindow();

        public static string SelectionPopup(string current, string[] displayOptions)
        {
            var idx = Array.IndexOf(displayOptions, current);
            GUILayout.Box(current, RGUIStyle.alignLeftBox);
            var newIdx = PopupOnLastRect(idx, displayOptions);
            if (newIdx != idx)
            {
                current = displayOptions[newIdx];
            }

            return current;
        }

        public static int SelectionPopup(int selectionIndex, string[] displayOptions)
        {
            var label = (selectionIndex < 0 || displayOptions.Length <= selectionIndex)
                ? ""
                : displayOptions[selectionIndex];
            GUILayout.Box(label, RGUIStyle.alignLeftBox);
            return PopupOnLastRect(selectionIndex, displayOptions);
        }

        public static int SelectionPopup(int selectionIndex, string[] displayOptions,
            Dictionary<string, string> tooltips = null, params GUILayoutOption[] options)
        {
            var label = (selectionIndex < 0 || displayOptions.Length <= selectionIndex)
                ? ""
                : displayOptions[selectionIndex];
            GUILayout.Box(label, RGUIStyle.alignLeftBox, options);
            return PopupOnLastRect(selectionIndex, displayOptions, -1, "", tooltips);
        }

        public static int PopupOnLastRect(string[] displayOptions, string label = "") =>
            PopupOnLastRect(-1, displayOptions, -1, label);

        public static int PopupOnLastRect(string[] displayOptions, int button, string label = "") =>
            PopupOnLastRect(-1, displayOptions, button, label);

        public static int PopupOnLastRect(int selectionIndex, string[] displayOptions, int mouseButton = -1,
            string label = "", Dictionary<string, string> tooltips = null) => Popup(GUILayoutUtility.GetLastRect(),
            mouseButton, selectionIndex, displayOptions,
            label, tooltips);


        public static int Popup(Rect launchRect, int mouseButton, int selectionIndex, string[] displayOptions,
            string label = "", Dictionary<string, string> tooltips = null)
        {
            var ret = selectionIndex;
            var controlId = GUIUtility.GetControlID(FocusType.Passive);

            // not Popup Owner
            if (popupControlId != controlId)
            {
                var ev = Event.current;
                var pos = ev.mousePosition;

                if ((ev.type == EventType.MouseUp)
                    && ((mouseButton < 0) || (ev.button == mouseButton))
                    && launchRect.Contains(pos)
                    && displayOptions != null
                    && displayOptions.Any()
                   )
                {
                    popupWindow.pos = RGUIUtility.GetMouseScreenPos(Vector2.one * 150f);
                    popupControlId = controlId;
                    ev.Use();
                }
            }
            // Active
            else
            {
                var type = Event.current.type;

                var result = popupWindow.result;
                if (result.HasValue && type == EventType.Layout)
                {
                    if (result.Value >= 0) // -1 when the popup is closed by clicking outside the window
                    {
                        ret = result.Value;
                    }

                    popupWindow.result = null;
                    popupControlId = 0;
                }
                else
                {
                    if ((type == EventType.Layout) || (type == EventType.Repaint))
                    {
                        var buttonStyle = RGUIStyle.popupFlatButton;
                        var contentSize = Vector2.zero;
                        for (var i = 0; i < displayOptions.Length; ++i)
                        {
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
                            Vector2.one * offset;
                        var size = RGUIStyle.popup.CalcScreenSize(contentSize);
                        var maxSize = new Vector2(Screen.width, Screen.height) - popupWindow.pos;

                        popupWindow.size = Vector2.Min(size, maxSize);
                    }

                    popupWindow.label = label;
                    popupWindow.displayOptions = displayOptions;
                    popupWindow.tooltips = tooltips;
                    WindowInvoker.Add(popupWindow);
                }
            }

            return ret;
        }


        public class PopupWindow : IDoGUIWindow
        {
            public string label;
            public Vector2 pos;
            public Vector2 size;
            public int? result;
            public string[] displayOptions;
            public Dictionary<string, string> tooltips;
            public Vector2 scrollPosition;

            public static bool showTooltip = false;
            public static string tooltip = "";

            static readonly int PopupWindowId = "Popup".GetHashCode();

            public Rect GetWindowRect() => new Rect(pos, size);

            public void DoGUIWindow()
            {
                var npopup = new GUIStyle(RGUIStyle.popup);
                npopup.normal.background = Texture2D.blackTexture;
                npopup.hover.background = Texture2D.blackTexture;
                npopup.padding = new RectOffset(100, 100, 100, 100);
                var wrect = GetWindowRect();
                wrect.x -= 100;
                wrect.y -= 100;
                wrect.width += 200;
                wrect.height += 200;
                GUI.ModalWindow(PopupWindowId, wrect, (id) =>
                    {
                        GUI.Box(new Rect(100, 100, GetWindowRect().width, GetWindowRect().height), "", RGUIStyle.popup);
                        showTooltip = false;
                        var bakv = GUI.skin.verticalScrollbar.normal.background;
                        GUI.skin.verticalScrollbar.normal.background = Drawer.jittengray;
                        var bakvt = GUI.skin.verticalScrollbarThumb.normal.background;
                        GUI.skin.verticalScrollbarThumb.normal.background = Drawer.gray;
                        using (var sc = new GUILayout.ScrollViewScope(scrollPosition))
                        {
                            scrollPosition = sc.scrollPosition;

                            for (var j = 0; j < displayOptions.Length; ++j)
                            {
                                if (GUILayout.Button(displayOptions[j], RGUIStyle.popupFlatButton))
                                {
                                    result = j;
                                    showTooltip = false;
                                }

                                /*if (displayOptions[j] == "Accuracy")
                                {
                                    Main.Logger.Log(displayOptions[j]);
                                    Main.Logger.Log("Hover: " + GUILayoutUtility.GetLastRect()
                                        .Contains(Event.current.mousePosition));
                                }*/

                                if (tooltips != null &&
                                    GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition) &&
                                    tooltips.TryGetValue(displayOptions[j], out var tooltip))
                                {
                                    Main.Logger.Log("ShowTooltip");
                                    showTooltip = true;
                                    PopupWindow.tooltip = tooltip;
                                }
                            }
                        }

                        GUI.skin.verticalScrollbar.normal.background = bakv;
                        GUI.skin.verticalScrollbarThumb.normal.background = bakvt;

                        var ev = Event.current;
                        if ((ev.rawType == EventType.MouseDown) &&
                            !(new Rect(Vector2.zero, size).Contains(ev.mousePosition)))
                        {
                            result = -1;
                            ;
                            showTooltip = false;
                        }

                        if (showTooltip)
                        {
                            Drawer.Tooltip(tooltip);
                        }
                    }
                    , label, npopup);
            }

            public void CloseWindow()
            {
                result = -1;
                showTooltip = false;
            }
        }
    }
}