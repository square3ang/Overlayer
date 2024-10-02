using System.Linq;

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

    public string Draw(string code, GUIStyle style, params GUILayoutOption[] options)
    {
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
        string editedCode = GUILayout.TextArea(code, backStyle, GUILayout.ExpandHeight(true), GUILayout.Width(900));

        if (editedCode != code)
        {
            code = editedCode;
            onValueChange?.Invoke();
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
            foreach (var ch in st)
            {
                curwidth += baseStyle.CalcSize(new GUIContent(ch.ToString())).x;
                if (curwidth >= 900)
                {
                    lineString += "\n";
                    curwidth -= 900;
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