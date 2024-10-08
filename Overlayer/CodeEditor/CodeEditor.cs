using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using HarmonyLib;
using Overlayer.Core;
using Overlayer.Tags;
using Overlayer.Utils;
using UnityModManagerNet;

namespace Overlayer.CodeEditor;

using UnityEngine;
using System.Reflection;

[System.Serializable]
public class CodeEditor
{
    public string controlName { get; set; }
    public System.Action onValueChange;
    public int tabSpaces = 2;
    public System.Func<string, string> highlighter { get; set; }

    private string cachedCode { get; set; }
    private string cachedHighlightedCode { get; set; }

    private CodeTheme theme;

    private int charWidth = 11;
    private bool pressedTab = false;
    private bool pressedShift = false;

    private MovingManEditor movingManEditor;
    private ColorRangeEditor colorRangeEditor;
    private int editingHash;


    public static List<int> ignoreTextAreaNext = new();

    private static Regex tagRegex = new(@"{(.*?)}", RegexOptions.Compiled);

    public bool isFocused
    {
        get { return GUI.GetNameOfFocusedControl() == controlName; }
    }

    public CodeEditor(string controlName, CodeTheme theme)
    {
        this.controlName = controlName;
        this.theme = theme;
        highlighter = code => code;
    }

    public string Update(string code)
    {
        // Update logic without using TextEditor (since it's editor-only)
        return code;
    }

    private string selectedtag = "Developer";

    public string Draw(string code, GUIStyle style, params GUILayoutOption[] options)
    {
        if (movingManEditor)
        {
            if (editingHash == code.GetHashCode())
            {
                code = movingManEditor.codesBefore + "MovingMan(" + movingManEditor.targetTag + "," +
                       movingManEditor.startSize + "," + movingManEditor.endSize + "," +
                       movingManEditor.defaultSize + "," + movingManEditor.speed + "," +
                       movingManEditor.invert + "," + movingManEditor.ease + ")" + movingManEditor.codesAfter;
                editingHash = code.GetHashCode();
            }
        }

        if (colorRangeEditor)
        {
            if (editingHash == code.GetHashCode())
            {
                code = colorRangeEditor.codesBefore + "ColorRange(" + colorRangeEditor.targetTag + "," +
                       colorRangeEditor.valueMin + "," + colorRangeEditor.valueMax + "," +
                       ColorUtility.ToHtmlStringRGBA(colorRangeEditor.colorMin) + "," +
                       ColorUtility.ToHtmlStringRGBA(colorRangeEditor.colorMax) + "," +
                       colorRangeEditor.ease +
                       ")" + colorRangeEditor.codesAfter;
                editingHash = code.GetHashCode();
            }
        }

        GUILayout.BeginHorizontal();

        Drawer.DrawTags(ref selectedtag);

        if (Drawer.Button(Main.Lang.Get("INSERT", "Insert")))
        {
            TextEditor editor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
            var sb = new StringBuilder(code);
            sb.Insert(editor.selectIndex, "{" + selectedtag + "}");
            code = sb.ToString();
            ignoreTextAreaNext.Clear();
        }

        GUILayout.FlexibleSpace();

        GUILayout.EndHorizontal();
        float lineCountWidth = code.Split('\n').Length.ToString().Length * charWidth;
        var preBackgroundColor = GUI.backgroundColor;
        var preColor = GUI.color;
        Color preSelection = GUI.skin.settings.selectionColor;
        Color preCursor = GUI.skin.settings.cursorColor;
        float preFlashSpeed = GUI.skin.settings.cursorFlashSpeed;

        GUI.backgroundColor = GetColor(theme.background);
        GUI.color = GetColor(theme.color);
        GUI.skin.settings.selectionColor = GetColor(theme.selection);
        GUI.skin.settings.cursorColor = GetColor(theme.cursor);
        GUI.skin.settings.cursorFlashSpeed = 0;

        var backStyle = new GUIStyle(style);
        backStyle.richText = false;
        backStyle.normal.textColor = Color.clear;
        backStyle.hover.textColor = Color.clear;
        backStyle.active.textColor = Color.clear;
        backStyle.focused.textColor = Color.clear;


        backStyle.normal.background = Texture2D.whiteTexture;
        backStyle.hover.background = Texture2D.whiteTexture;
        backStyle.active.background = Texture2D.whiteTexture;
        backStyle.focused.background = Texture2D.whiteTexture;

        backStyle.padding.left = 5;

        GUILayout.BeginHorizontal();

        // Line numbers
        DrawLineNumbers(code, style);

        // Handle tab key
        bool usedTab = Event.current.type != EventType.Layout
                       && (Event.current.keyCode == KeyCode.Tab || Event.current.character == '\t');

        pressedTab = usedTab && Event.current.type == EventType.KeyDown;
        pressedShift = Event.current.shift;

        if (usedTab)
            Event.current.Use();

        // Drawing the text area using GUILayout
        GUI.SetNextControlName(controlName);
        var editorw = ((Rect)AccessTools.Field(typeof(UnityModManager.UI), "mWindowRect")
                .GetValue(UnityModManager.UI.Instance))
            .width - (55 + lineCountWidth);


        if (!ignoreTextAreaNext.Contains(code.GetHashCode()) && !movingManEditor && !colorRangeEditor)
        {
            string editedCode = GUILayout.TextArea(code, backStyle, GUILayout.ExpandHeight(true),
                GUILayout.Width(editorw));
            if (editedCode != code)
            {
                code = editedCode;
                ignoreTextAreaNext.Clear();
                onValueChange?.Invoke();
            }
        }

        else
        {
            //Main.Logger.Log("Ignore");
            GUILayout.Box(code, backStyle, GUILayout.ExpandHeight(true),
                GUILayout.Width(editorw));
        }


        if (cachedCode != code)
        {
            cachedCode = code;
            cachedHighlightedCode = highlighter(code);
        }

        // Render syntax highlighting
        GUI.backgroundColor = Color.clear;

        var foreStyle = new GUIStyle(style);
        foreStyle.richText = true;

        foreStyle.normal.textColor = GUI.color;
        foreStyle.hover.textColor = GUI.color;
        foreStyle.active.textColor = GUI.color;
        foreStyle.focused.textColor = GUI.color;
        foreStyle.padding.left = 5;

        // Render highlighted text
        GUI.Label(GUILayoutUtility.GetLastRect(), cachedHighlightedCode, foreStyle);

        var found = false;

        var i = 0;


        var tooltip = new Dictionary<string, string>();
        foreach (var toolt in Tags.Tooltip.tooltip)
        {
            tooltip[toolt.Key] = Main.Lang.Get("TOOLTIP_" + toolt.Key.ToUpper(), toolt.Value);
        }

        // Get Tags
        foreach (Match match in tagRegex.Matches(code))
        {
            var tag = match.Groups[1].Value;
            var start = match.Groups[1].Index;
            var end = start + match.Groups[1].Length;
            var codesBefore = code.Substring(0, start);
            var codesAfter = code.Substring(end, code.Length - end);
            var lines = codesBefore.Split('\n');
            var lastline = lines[lines.Length - 1];
            var height = style.CalcSize(new GUIContent("A")).y;

            var len = lines.Length - 1;
            var xc = 0f;
            foreach (var l in lines)
            {
                xc = style.CalcSize(new GUIContent(l)).x;
                while (xc >= editorw - 5)
                {
                    xc -= editorw - 5;
                    len++;
                }
            }

            //Main.Logger.Log(lastline);

            var width = style.CalcSize(new GUIContent(lastline)).x;


            while (width >= editorw - 5)
            {
                width -= editorw - 5;
            }

            //Main.Logger.Log(width + "");


            var y = len * height;

            var x = width + 5;

            var rects = new List<Rect>();

            var rect1 = GUILayoutUtility.GetLastRect();
            rect1.x += x;
            rect1.y += y;

            rect1.width = style.CalcSize(new GUIContent(match.Groups[1].Value)).x;

            rect1.height = height;

            var rc = 0;

            while (rect1.width + width >= editorw - 5)
            {
                rc++;
                var chai = rect1.width + width - (editorw - 5);
                rect1.width -= editorw - 5;
                var rect2 = new Rect(rect1);
                rect2.x = 5;
                rect2.y += height * rc;
                if (chai >= editorw - 5) chai = editorw - 5;
                rect2.width = chai;
                rects.Add(rect2);
                
            }

            rects.Add(rect1);

            var mvm = match.Groups[1].Value.StartsWith("MovingMan");

            var cr = match.Groups[1].Value.StartsWith("ColorRange");

            var special = mvm || cr;

            if (mvm && !Main.Settings.useMovingManEditor) special = false;
            if (cr && !Main.Settings.useColorRangeEditor) special = false;

            if (Event.current.type == EventType.Repaint)
            {
                foreach (var rect in rects)
                {
                    if (rect.Contains(Event.current.mousePosition))
                    {
                        if (special)
                            found = true;

                        var pars = match.Groups[1].Value.Split('(')[0].Split(':')[0];
                        if (TagManager.tags.ContainsKey(pars) && tooltip.TryGetValue(pars, out var tooltipp))
                        {
                            Main.showTooltip = true;
                            Main.tooltip = tooltipp;
                        }
                    }
                }
            }
            else
            {
                if (special && !found)
                    found = ignoreTextAreaNext.Contains(code.GetHashCode());
            }


            if (special)
            {
                foreach (var rect in rects)
                {
                    if (GUI.Button(rect, ""))
                    {
                        if (cr)
                        {
                            colorRangeEditor = new GameObject().AddComponent<ColorRangeEditor>();
                            Object.DontDestroyOnLoad(colorRangeEditor);
                            colorRangeEditor.Initialize(match.Groups[1].Value, codesBefore, codesAfter);
                        }
                        else if (mvm)
                        {
                            movingManEditor = new GameObject().AddComponent<MovingManEditor>();
                            Object.DontDestroyOnLoad(movingManEditor);
                            movingManEditor.Initialize(match.Groups[1].Value, codesBefore, codesAfter);
                        }

                        editingHash = code.GetHashCode();
                    }
                }
            }
        }

        if (found)
        {
            if (!ignoreTextAreaNext.Contains(code.GetHashCode()))
                ignoreTextAreaNext.Add(code.GetHashCode());
        }
        else
        {
            ignoreTextAreaNext.Remove(code.GetHashCode());
        }

        GUI.backgroundColor = preBackgroundColor;
        GUI.color = preColor;
        GUI.skin.settings.selectionColor = preSelection;
        GUI.skin.settings.cursorColor = preCursor;
        GUI.skin.settings.cursorFlashSpeed = preFlashSpeed;

        GUILayout.EndHorizontal();

        return code;
    }

    private string UpdateEditorTabs(string content, bool shift)
    {
        // Custom tab handling logic for runtime
        string tabrep = new string(' ', tabSpaces);

        // Normal case
        if (!shift)
        {
            content += tabrep;
        }
        else if (content.Length >= tabSpaces)
        {
            content = content.Remove(content.Length - tabSpaces, tabSpaces);
        }

        return content;
    }

    private void DrawLineNumbers(string code, GUIStyle baseStyle)
    {
        float lineCountWidth = code.Split('\n').Length.ToString().Length * charWidth;

        // Reserve space
        Rect rect = GUILayoutUtility.GetRect(lineCountWidth, 100, GUILayout.ExpandHeight(true));

        string lineString = "";
        var i = 0;
        float curwidth;
        foreach (var st in code.Split('\n'))
        {
            curwidth = 0;
            lineString += ++i + "\n";
            var editorw = ((Rect)AccessTools.Field(typeof(UnityModManager.UI), "mWindowRect")
                    .GetValue(UnityModManager.UI.Instance))
                .width - (55 + lineCountWidth);
            foreach (var ch in st)
            {
                curwidth += baseStyle.CalcSize(new GUIContent(ch.ToString())).x;
                if (curwidth >= editorw - 5)
                {
                    lineString += "\n";
                    curwidth -= editorw - 5;
                }
            }
        }

        GUIStyle style = new GUIStyle(baseStyle);
        style.normal.textColor = Color.white;

        style.normal.background = Texture2D.whiteTexture;
        style.hover.background = Texture2D.whiteTexture;
        style.active.background = Texture2D.whiteTexture;
        style.focused.background = Texture2D.whiteTexture;

        style.alignment = TextAnchor.UpperCenter;

        GUI.backgroundColor = GetColor(theme.linenumbg);

        GUI.Label(rect, new GUIContent(lineString), style);

        GUI.backgroundColor = GetColor(theme.background);
    }

    private Color GetColor(string colorCode)
    {
        Color color = Color.magenta;
        ColorUtility.TryParseHtmlString(colorCode, out color);
        return color;
    }
}