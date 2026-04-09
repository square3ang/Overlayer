using Overlayer.Core;
using Overlayer.Tags;
using Overlayer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using UnityEngine;
using Object = UnityEngine.Object;

namespace Overlayer.CodeEditor;

public class CodeEditor {
    public static CodeEditor instance = new("OverlayerCodeEditor", new CodeTheme {
        background = "#333333",
        linenumbg = "#222222",
        color = "#FFFFFF",
        selection = "#264F78",
        cursor = "#D4D4D4"
    });

    public static Regex color = new("<<b></b>color=(.*?)>", RegexOptions.Compiled);

    public static void Initialize() {
        instance.highlighter = str => {
            str = str.Replace("<", "<<b></b>");

            var colorHighlighted = new List<string>();
            foreach(Match m in color.Matches(str)) {
                if(!colorHighlighted.Contains(m.Groups[1].Value) && ColorUtility.TryParseHtmlString(m.Groups[1].Value, out _)) {
                    str = str.Replace("<<b></b>color=" + m.Groups[1].Value + ">",
                        "<<b></b>color=<color=" + m.Groups[1].Value + ">" + m.Groups[1].Value + "</color>>");
                    colorHighlighted.Add(m.Groups[1].Value);
                }
            }

            var highlighted = new List<string>();

            foreach(Match match in tagRegex.Matches(str)) {
                if(highlighted.Contains(match.Groups[1].Value)) {
                    continue;
                }

                var fullTag = match.Groups[1].Value;
                var splitChar = fullTag.Contains(':') ? ':' : (fullTag.Contains(';') ? ';' : '\0');
                var name = splitChar != '\0' ? fullTag.Split(splitChar)[0] : fullTag;

                if(TagManager.tags.ContainsKey(name)) {
                    if(splitChar == ';') {
                        str = str.Replace("{" + fullTag + "}", "<color=blue>{" + fullTag + "}</color>");
                    } else if((Main.Settings.MovingManEditor && name == nameof(Effect.MovingMan)) ||
                              (Main.Settings.ColorRangeEditor && name == nameof(Effect.ColorRange)) ||
                              (Main.Settings.EasedValueEditor && name == nameof(Effect.EasedValue))) {
                        str = str.Replace("{" + fullTag + "}", "<color=orange>{" + fullTag + "}</color>");
                    } else if(name.EndsWith("Hex")) {
                        try {
                            var val = (string)TagManager.tags[name].Tag.Getter.Invoke(null,
                                new object[] { "-1", Overlayer.Utils.Extensions.DefaultTrimStr });
                            str = str.Replace("{" + fullTag + "}", "<color=#" + val + ">{" + fullTag + "}</color>");
                        } catch {
                            str = str.Replace("{" + fullTag + "}", "<color=lightblue>{" + fullTag + "}</color>");
                        }
                    } else {
                        str = str.Replace("{" + fullTag + "}", "<color=lightblue>{" + fullTag + "}</color>");
                    }
                } else {
                    str = str.Replace("{" + fullTag + "}", "<color=red>{" + fullTag + "}</color>");
                }

                highlighted.Add(fullTag);
            }

            return str;
        };
    }

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
    private EasedValueEditor easedValueEditor;
    private int editingHash;

    private static Regex tagRegex = new(@"{(.*?)}", RegexOptions.Compiled);

    public bool isFocused => GUI.GetNameOfFocusedControl() == controlName;

    public CodeEditor(string controlName, CodeTheme theme) {
        this.controlName = controlName;
        this.theme = theme;
        highlighter = code => code;
    }

    private string selectedtag = nameof(Developers.Developer);

    internal Dictionary<string, UndoRedoManager> undoRedoManagers = [];

    public string Draw(string code, GUIStyle style, string id, params GUILayoutOption[] options) {
        if(!undoRedoManagers.ContainsKey(id)) {
            undoRedoManagers[id] = new UndoRedoManager();
            undoRedoManagers[id].SaveState(code);
        }

        controlName = id;
        var oldEvent = new Event(Event.current);
        if(movingManEditor) {
            if(editingHash == code.GetHashCode()) {
                code = movingManEditor.codesBefore + nameof(Effect.MovingMan) + "(" + movingManEditor.targetTag + "," +
                       movingManEditor.startSize + "," + movingManEditor.endSize + "," +
                       movingManEditor.defaultSize + "," + movingManEditor.speed + "," +
                       movingManEditor.invert + "," + movingManEditor.ease + ")" + movingManEditor.codesAfter;
                editingHash = code.GetHashCode();
            }
        }

        if(colorRangeEditor) {
            if(editingHash == code.GetHashCode()) {
                code = colorRangeEditor.codesBefore + nameof(Effect.ColorRange) + "(" + colorRangeEditor.targetTag + "," +
                       colorRangeEditor.valueMin + "," + colorRangeEditor.valueMax + "," +
                       ColorUtility.ToHtmlStringRGBA(colorRangeEditor.colorMin) + "," +
                       ColorUtility.ToHtmlStringRGBA(colorRangeEditor.colorMax) + "," +
                       colorRangeEditor.ease + "," + colorRangeEditor.maxLength +
                       ")" + colorRangeEditor.codesAfter;
                editingHash = code.GetHashCode();
            }
        }

        if(easedValueEditor) {
            if(editingHash == code.GetHashCode()) {
                code = easedValueEditor.codesBefore + nameof(Effect.EasedValue) + "(" + easedValueEditor.targetTag + "," +
                       easedValueEditor.digits + "," + easedValueEditor.speed + "," +
                       easedValueEditor.ease + ")" + easedValueEditor.codesAfter;
                editingHash = code.GetHashCode();
            }
        }

        GUILayout.BeginHorizontal();
        GUILayout.Label(Drawer.Icon_Parse);
        GUILayout.Space(2);
        Drawer.DrawTags(ref selectedtag);

        if(Drawer.Button(Main.Lang.Get("INSERT", "Insert"))) {
            TextEditor editor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
            var sb = new StringBuilder(code);
            sb.Insert(editor.selectIndex, "{" + selectedtag + "}");
            code = sb.ToString();
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

        var backStyle = new GUIStyle(style) {
            richText = false
        };
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

        if(usedTab) {
            Event.current.Use();
        }

        // Drawing the text area using GUILayout
        GUI.SetNextControlName(controlName);
        var editorw = 700;

        if(isFocused) {
            if(Event.current.type == EventType.KeyDown) {
                var oldcode = code;
                if(Event.current.keyCode == KeyCode.Z && Event.current.control) {
                    if(Event.current.shift) {
                        var tx = undoRedoManagers[id].Redo();
                        if(tx != null) {
                            code = tx;
                        }
                    } else {
                        var tx = undoRedoManagers[id].Undo();
                        if(tx != null) {
                            code = tx;
                        }
                    }
                } else if((Event.current.keyCode == KeyCode.Y && Event.current.control) || (Event.current.shift && Event.current.keyCode == KeyCode.Z)) {
                    var tx = undoRedoManagers[id].Redo();
                    if(tx != null) {
                        code = tx;
                    }
                }

                if(code != oldcode) {
                    Event.current.Use();
                }
            }
        }

        if(!movingManEditor && !colorRangeEditor && !easedValueEditor) {
            GUI.SetNextControlName(id);
            string editedCode = GUILayout.TextArea(code, backStyle, GUILayout.ExpandHeight(true),
                GUILayout.Width(Math.Max(editorw, style.CalcSize(new GUIContent(code)).x + 5)));
            if(editedCode != code) {
                code = editedCode;
                undoRedoManagers[id].SaveState(code);
                onValueChange?.Invoke();
            }
        } else {
            GUILayout.Box(code, backStyle, GUILayout.ExpandHeight(true),
                GUILayout.Width(Math.Max(editorw, style.CalcSize(new GUIContent(code)).x + 5)));
        }

        if(cachedCode != code) {
            cachedCode = code;
            cachedHighlightedCode = highlighter(code);
        }

        // Render syntax highlighting
        GUI.backgroundColor = Color.clear;

        var foreStyle = new GUIStyle(style) {
            richText = true
        };

        foreStyle.normal.textColor = GUI.color;
        foreStyle.hover.textColor = GUI.color;
        foreStyle.active.textColor = GUI.color;
        foreStyle.focused.textColor = GUI.color;
        foreStyle.padding.left = 5;

        // Render highlighted text
        GUI.Label(GUILayoutUtility.GetLastRect(), cachedHighlightedCode, foreStyle);

        var i = 0;
        var bak = Event.current;
        Event.current = oldEvent;

        if(!movingManEditor && !colorRangeEditor && !easedValueEditor) {
            // Get Tags
            foreach(Match match in tagRegex.Matches(code)) {
                var tag = match.Groups[1].Value;
                var start = match.Groups[1].Index;
                var end = start + match.Groups[1].Length;
                var codesBefore = code.Substring(0, start);
                var codesAfter = code.Substring(end, code.Length - end);
                var lines = codesBefore.Split('\n');
                var lastline = lines[lines.Length - 1];
                var height = style.lineHeight;

                var len = lines.Length - 1;
                /*var xc = 0f;
                foreach (var l in lines)
                {
                    xc = style.CalcSize(new GUIContent(l)).x;
                    while (xc >= editorw - 5)
                    {
                        xc -= editorw - 5;
                        len++;
                    }
                }*/

                var width = style.CalcSize(new GUIContent(lastline)).x;

                /*while (width >= editorw - 5)
                {
                    width -= editorw - 5;
                }*/

                var y = len * height;

                var x = width + 5;

                var rect = GUILayoutUtility.GetLastRect();
                rect.x += x;
                rect.y += y;

                rect.width = style.CalcSize(new GUIContent(match.Groups[1].Value)).x;

                rect.height = height;

                var mvm = match.Groups[1].Value.StartsWith(nameof(Effect.MovingMan));

                var cr = match.Groups[1].Value.StartsWith(nameof(Effect.ColorRange));

                var ev = match.Groups[1].Value.StartsWith(nameof(Effect.EasedValue));

                var special = mvm || cr || ev;

                if(mvm && !Main.Settings.MovingManEditor) {
                    special = false;
                }

                if(cr && !Main.Settings.ColorRangeEditor) {
                    special = false;
                }

                if(ev && !Main.Settings.EasedValueEditor) {
                    special = false;
                }

                if(rect.Contains(Event.current.mousePosition)) {
                    var pars = match.Groups[1].Value.Split('(')[0]
                                     .Split([':', ';'], 2)[0];
                    Main.tooltip = TagManager.tags.ContainsKey(pars)
                                   ? Main.Lang.Get($"TAG_DESC_{pars.ToUpper()}", TagDesc.GetTagDesc(pars))
                                   : Main.Lang.Get("NOT_EXIST_TAG", "This tag does not exist");
                }

                if(special) {
                    if(GUI.Button(rect, "")) {
                        if(cr) {
                            colorRangeEditor = new GameObject().AddComponent<ColorRangeEditor>();
                            Object.DontDestroyOnLoad(colorRangeEditor);
                            colorRangeEditor.Initialize(match.Groups[1].Value, codesBefore, codesAfter);
                        } else if(mvm) {
                            movingManEditor = new GameObject().AddComponent<MovingManEditor>();
                            Object.DontDestroyOnLoad(movingManEditor);
                            movingManEditor.Initialize(match.Groups[1].Value, codesBefore, codesAfter);
                        } else if(ev) {
                            easedValueEditor = new GameObject().AddComponent<EasedValueEditor>();
                            Object.DontDestroyOnLoad(easedValueEditor);
                            easedValueEditor.Initialize(match.Groups[1].Value, codesBefore, codesAfter);
                        }

                        editingHash = code.GetHashCode();
                    }
                }
            }
        }

        Event.current = bak;

        GUI.backgroundColor = preBackgroundColor;
        GUI.color = preColor;
        GUI.skin.settings.selectionColor = preSelection;
        GUI.skin.settings.cursorColor = preCursor;
        GUI.skin.settings.cursorFlashSpeed = preFlashSpeed;

        GUILayout.EndHorizontal();

        return code;
    }

    private string UpdateEditorTabs(string content, bool shift) {
        // Custom tab handling logic for runtime
        string tabrep = new(' ', tabSpaces);

        // Normal case
        if(!shift) {
            content += tabrep;
        } else if(content.Length >= tabSpaces) {
            content = content.Remove(content.Length - tabSpaces, tabSpaces);
        }

        return content;
    }

    private void DrawLineNumbers(string code, GUIStyle baseStyle) {
        float lineCountWidth = code.Split('\n').Length.ToString().Length * charWidth;

        // Reserve space
        Rect rect = GUILayoutUtility.GetRect(lineCountWidth, 100, GUILayout.ExpandHeight(true),
            GUILayout.ExpandWidth(false));

        string lineString = "";
        var i = 0;
        float curwidth;
        foreach(var st in code.Split('\n')) {
            curwidth = 0;
            lineString += ++i + "\n";
            foreach(var ch in st) {
                curwidth += baseStyle.CalcSize(new GUIContent(ch.ToString())).x;
                /*if (curwidth >= editorw - 5)
                {
                    lineString += "\n";
                    curwidth -= editorw - 5;
                }*/
            }
        }

        GUIStyle style = new(baseStyle);
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

    private Color GetColor(string colorCode) {
        Color color = Color.magenta;
        ColorUtility.TryParseHtmlString(colorCode, out color);
        return color;
    }
}