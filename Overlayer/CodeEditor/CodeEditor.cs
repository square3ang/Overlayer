using Overlayer.CodeEditor.Impl;
using Overlayer.Core;
using Overlayer.Tags;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using UnityEngine;
using Object = UnityEngine.Object;

namespace Overlayer.CodeEditor;

public class CodeEditor(string controlName, CodeTheme theme) {
    public static CodeEditor Instance = new("OverlayerCodeEditor", new CodeTheme {
        Background = "#333333",
        Linenumbg = "#222222",
        Color = "#FFFFFF",
        Selection = "#264F78",
        Cursor = "#D4D4D4"
    });

    public static Regex color = new("<<b></b>color=(.*?)>", RegexOptions.Compiled);

    public static void Initialize() {
        Instance.Highlighter = str => {
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

            foreach(Match match in TagRegex.Matches(str)) {
                var fullTag = match.Groups[1].Value;

                if(highlighted.Contains(fullTag)) {
                    continue;
                }

                char splitChar = '\0';

                if(fullTag.Contains(':')) {
                    splitChar = ':';
                } else if(fullTag.Contains(';')) {
                    splitChar = ';';
                } else if(fullTag.Contains('(')) {
                    splitChar = '(';
                }

                var name = fullTag;

                if(splitChar != '\0') {
                    name = fullTag.Split(splitChar)[0];
                }

                if(name.EndsWith("()")) {
                    name = name.Substring(0, name.Length - 2);
                }

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
                                ["-1", Overlayer.Utils.Extensions.DefaultTrimStr]);

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

    public string ControlName { get; set; } = controlName;
    public Action OnValueChange;
    public const int TabSpaces = 2;
    public Func<string, string> Highlighter { get; set; } = code => code;

    private string CachedCode { get; set; }
    private string CachedHighlightedCode { get; set; }

    private readonly CodeTheme Theme = theme;

    private const int CharWidth = 11;
    private bool PressedTab = false;
    private bool PressedShift = false;

    private MovingManEditor MovingManEditor;
    private ColorRangeEditor ColorRangeEditor;
    private EasedValueEditor EasedValueEditor;
    private int EditingHash;

    private static readonly Regex TagRegex = new(@"{(.*?)}", RegexOptions.Compiled);

    public bool IsFocused => GUI.GetNameOfFocusedControl() == ControlName;

    private string Selectedtag = nameof(Developers.Developer);

    internal Dictionary<string, UndoRedoManager> UndoRedoManagers = [];

    public string Draw(string code, GUIStyle style, string id, params GUILayoutOption[] options) {
        if(!UndoRedoManagers.ContainsKey(id)) {
            UndoRedoManagers[id] = new UndoRedoManager();
            UndoRedoManagers[id].SaveState(code);
        }

        ControlName = id;
        var oldEvent = new Event(Event.current);
        if(MovingManEditor) {
            if(EditingHash == code.GetHashCode()) {
                code = MovingManEditor.CodesBefore + nameof(Effect.MovingMan) + "(" + MovingManEditor.TargetTag + "," +
                       MovingManEditor.StartSize + "," + MovingManEditor.EndSize + "," +
                       MovingManEditor.DefaultSize + "," + MovingManEditor.Speed + "," +
                       MovingManEditor.Invert + "," + MovingManEditor.Ease + ")" + MovingManEditor.CodesAfter;
                EditingHash = code.GetHashCode();
            }
        }

        if(ColorRangeEditor) {
            if(EditingHash == code.GetHashCode()) {
                code = ColorRangeEditor.CodesBefore + nameof(Effect.ColorRange) + "(" + ColorRangeEditor.TargetTag + "," +
                       ColorRangeEditor.ValueMin + "," + ColorRangeEditor.ValueMax + "," +
                       ColorUtility.ToHtmlStringRGBA(ColorRangeEditor.ColorMin) + "," +
                       ColorUtility.ToHtmlStringRGBA(ColorRangeEditor.ColorMax) + "," +
                       ColorRangeEditor.Ease + "," + ColorRangeEditor.MaxLength +
                       ")" + ColorRangeEditor.CodesAfter;
                EditingHash = code.GetHashCode();
            }
        }

        if(EasedValueEditor) {
            if(EditingHash == code.GetHashCode()) {
                code = EasedValueEditor.CodesBefore + nameof(Effect.EasedValue) + "(" + EasedValueEditor.TargetTag + "," +
                       EasedValueEditor.Digits + "," + EasedValueEditor.Speed + "," +
                       EasedValueEditor.Ease + ")" + EasedValueEditor.CodesAfter;
                EditingHash = code.GetHashCode();
            }
        }

        GUILayout.BeginHorizontal();
        GUILayout.Label(Drawer.Icon_Parse);
        GUILayout.Space(2);
        Drawer.DrawTags(ref Selectedtag);

        if(Drawer.Button(Main.Lang.Get("INSERT", "Insert"))) {
            TextEditor editor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
            var sb = new StringBuilder(code);
            sb.Insert(editor.selectIndex, "{" + Selectedtag + "}");
            code = sb.ToString();
        }

        GUILayout.FlexibleSpace();

        GUILayout.EndHorizontal();
        float lineCountWidth = code.Split('\n').Length.ToString().Length * CharWidth;
        var preBackgroundColor = GUI.backgroundColor;
        var preColor = GUI.color;
        Color preSelection = GUI.skin.settings.selectionColor;
        Color preCursor = GUI.skin.settings.cursorColor;
        float preFlashSpeed = GUI.skin.settings.cursorFlashSpeed;

        GUI.backgroundColor = GetColor(Theme.Background);
        GUI.color = GetColor(Theme.Color);
        GUI.skin.settings.selectionColor = GetColor(Theme.Selection);
        GUI.skin.settings.cursorColor = GetColor(Theme.Cursor);
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

        DrawLineNumbers(code, style);

        bool usedTab = Event.current.type != EventType.Layout
            && (Event.current.keyCode == KeyCode.Tab || Event.current.character == '\t');

        PressedTab = usedTab && Event.current.type == EventType.KeyDown;
        PressedShift = Event.current.shift;

        if(usedTab) {
            Event.current.Use();
        }

        GUI.SetNextControlName(ControlName);
        var editorw = 700;

        if(IsFocused) {
            if(Event.current.type == EventType.KeyDown) {
                var oldcode = code;
                if(Event.current.keyCode == KeyCode.Z && Event.current.control) {
                    if(Event.current.shift) {
                        var tx = UndoRedoManagers[id].Redo();
                        if(tx != null) {
                            code = tx;
                        }
                    } else {
                        var tx = UndoRedoManagers[id].Undo();
                        if(tx != null) {
                            code = tx;
                        }
                    }
                } else if((Event.current.keyCode == KeyCode.Y && Event.current.control) || (Event.current.shift && Event.current.keyCode == KeyCode.Z)) {
                    var tx = UndoRedoManagers[id].Redo();
                    if(tx != null) {
                        code = tx;
                    }
                }

                if(code != oldcode) {
                    Event.current.Use();
                }
            }
        }

        if(!MovingManEditor && !ColorRangeEditor && !EasedValueEditor) {
            GUI.SetNextControlName(id);
            string editedCode = GUILayout.TextArea(code, backStyle, GUILayout.ExpandHeight(true),
                GUILayout.Width(Math.Max(editorw, style.CalcSize(new GUIContent(code)).x + 5)));
            if(editedCode != code) {
                code = editedCode;
                UndoRedoManagers[id].SaveState(code);
                OnValueChange?.Invoke();
            }
        } else {
            GUILayout.Box(code, backStyle, GUILayout.ExpandHeight(true),
                GUILayout.Width(Math.Max(editorw, style.CalcSize(new GUIContent(code)).x + 5)));
        }

        if(CachedCode != code) {
            CachedCode = code;
            CachedHighlightedCode = Highlighter(code);
        }

        GUI.backgroundColor = Color.clear;

        var foreStyle = new GUIStyle(style) {
            richText = true
        };

        foreStyle.normal.textColor = GUI.color;
        foreStyle.hover.textColor = GUI.color;
        foreStyle.active.textColor = GUI.color;
        foreStyle.focused.textColor = GUI.color;
        foreStyle.padding.left = 5;

        GUI.Label(GUILayoutUtility.GetLastRect(), CachedHighlightedCode, foreStyle);
        var bak = Event.current;
        Event.current = oldEvent;

        if(!MovingManEditor && !ColorRangeEditor && !EasedValueEditor) {
            foreach(Match match in TagRegex.Matches(code)) {
                var tag = match.Groups[1].Value;
                var start = match.Groups[1].Index;
                var end = start + match.Groups[1].Length;
                var codesBefore = code.Substring(0, start);
                var codesAfter = code.Substring(end, code.Length - end);
                var lines = codesBefore.Split('\n');
                var lastline = lines[lines.Length - 1];
                var height = style.lineHeight;
                var len = lines.Length - 1;
                var width = style.CalcSize(new GUIContent(lastline)).x;
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
                            CreateEditor(
                                match.Groups[1].Value,
                                codesBefore,
                                codesAfter,
                                out ColorRangeEditor
                            );
                        } else if(mvm) {
                            CreateEditor(
                                match.Groups[1].Value,
                                codesBefore,
                                codesAfter,
                                out MovingManEditor
                            );
                        } else if(ev) {
                            CreateEditor(
                                match.Groups[1].Value,
                                codesBefore,
                                codesAfter,
                                out EasedValueEditor
                            );
                        }

                        EditingHash = code.GetHashCode();
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

    private static T CreateEditor<T>(string tag, string codesBefore, string codesAfter, out T instance )where T : EffectEditor {
        var go = new GameObject(typeof(T).Name);
        var editor = go.AddComponent<T>();

        Object.DontDestroyOnLoad(go);
        editor.Init(tag, codesBefore, codesAfter);

        instance = editor;

        return editor;
    }

    private string UpdateEditorTabs(string content, bool shift) {
        string tabrep = new(' ', TabSpaces);

        if(!shift) {
            content += tabrep;
        } else if(content.Length >= TabSpaces) {
            content = content.Remove(content.Length - TabSpaces, TabSpaces);
        }

        return content;
    }

    private void DrawLineNumbers(string code, GUIStyle baseStyle) {
        float lineCountWidth = code.Split('\n').Length.ToString().Length * CharWidth;

        Rect rect = GUILayoutUtility.GetRect(lineCountWidth, 100, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(false));

        string lineString = "";
        var i = 0;
        float curwidth;
        foreach(var st in code.Split('\n')) {
            curwidth = 0;
            lineString += ++i + "\n";
            foreach(var ch in st) {
                curwidth += baseStyle.CalcSize(new GUIContent(ch.ToString())).x;
            }
        }

        GUIStyle style = new(baseStyle);
        style.normal.textColor = Color.white;

        style.normal.background = Texture2D.whiteTexture;
        style.hover.background = Texture2D.whiteTexture;
        style.active.background = Texture2D.whiteTexture;
        style.focused.background = Texture2D.whiteTexture;

        style.alignment = TextAnchor.UpperCenter;

        GUI.backgroundColor = GetColor(Theme.Linenumbg);

        GUI.Label(rect, new GUIContent(lineString), style);

        GUI.backgroundColor = GetColor(Theme.Background);
    }

    private Color GetColor(string colorCode) {
        Color color = Color.magenta;
        ColorUtility.TryParseHtmlString(colorCode, out color);
        return color;
    }
}