using Newtonsoft.Json.Linq;
using Overlayer.Core;
using Overlayer.Models;
using Overlayer.Unity;
using Overlayer.Utils;
using SFB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace Overlayer.Views;

public class ProfileDrawer : ModelDrawable<ProfileConfig> {
    public OverlayerProfile profile;

    public ProfileDrawer(OverlayerProfile profile) : base(profile.Config) => this.profile = profile;

    public override void OnceCall() => NeoDrawer.StaticInstance.FieldResetDictById();

    private int[] dragSoltRange;
    private bool dragSoltNeedInit = true;
    private int dragSoltDragging = -1;
    private int dragSoltInsert = -1;

    public override void Draw() {
        NeoDrawer.StaticInstance.FieldResetId();

        if(Drawer.DrawBool(Drawer.Icon_Power, Main.Lang.Get("ACTIVE", "Active"), ref model.Active)) {
            profile.gameObject.SetActive(profile.Config.Active);
        }
        GUILayout.BeginHorizontal();
        if(NeoDrawer.StaticInstance.DrawPath(Drawer.Icon_Pencil, Main.Lang.Get("NAME", "Name"), ref model.Name, Main.ProfilePath, "json")) {
            ProfileManager.Rename(profile, model.Name);
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        if(NeoDrawer.StaticInstance.DrawSingleWithSlider(Drawer.Icon_Opacity, Main.Lang.Get("OPACITY", "Opacity"), ref model.Opacity, 0f, 1f, 300f)) {
            profile.ApplyConfig();
        }
        Color old = GUI.color;
        GUILayout.BeginHorizontal();
        Type needCreateNewObject = null;
        if(Drawer.Button(Drawer.Icon_Font, GUILayout.Width(60))) {
            needCreateNewObject = typeof(TextConfig);
        }
        if(Drawer.Button(Drawer.Icon_Image, GUILayout.Width(40))) {
            needCreateNewObject = typeof(ImageConfig);
        }

        GUI.color = new Color(1f, 1f, 0.8f);
        if(Drawer.Button(Drawer.Icon_Down, GUILayout.Width(60))) {
            Task.Run(() => {
                string[] texts = StandaloneFileBrowser.OpenFilePanel(
                    Main.Lang.Get("SELECT_OBJECT", "Select Object"),
                    Main.Mod.Path,
                    [new ExtensionFilter(Main.Lang.Get("OVERLAYER_OBJECT_JSON", "Overlayer Object JSON"), "json")],
                    true
                );
                if(texts == null || texts.Length == 0) {
                    return;
                }
                var configsToAdd = new List<ObjectConfig>();
                foreach(var text in texts) {
                    try {
                        if(Path.GetExtension(text) != ".json") {
                            continue;
                        }
                        string content = File.ReadAllText(text);
                        if(string.IsNullOrWhiteSpace(content)) {
                            continue;
                        }
                        JToken json = JToken.Parse(content);
                        if(json is JArray arr) {
                            foreach(var token in arr) {
                                string type = token["Type"]?.Value<string>();
                                ObjectConfig cfg = string.IsNullOrEmpty(type)
                                    ? new TextConfig()
                                    : type == "Text" ? TextConfigImporter.Import(token) : type == "Image" ? ImageConfigImporter.Import(token) : new TextConfig();
                                configsToAdd.Add(cfg);
                            }
                        } else if(json is JObject obj) {
                            string type = obj["Type"]?.Value<string>();

                            ObjectConfig cfg = type switch {
                                null or "" or "Text" => TextConfigImporter.Import(obj),
                                "Image" => ImageConfigImporter.Import(obj),
                                _ => new TextConfig()
                            };

                            configsToAdd.Add(cfg);
                        }
                    } catch(Exception e) {
                        Debug.LogError($"Failed to load text '{text}': {e}");
                    }
                }
                Main.MainThreadDispatcher.Enqueue(() => {
                    foreach(var cfg in configsToAdd) {
                        profile.ObjectManager.Create(cfg);
                    }
                    dragSoltNeedInit = true;
                    profile.ObjectManager.Refresh();
                });
            });
        }
        GUI.color = old;
        string showAs = Main.Settings.ShowTextNameAsDisplayText
            ? Main.Lang.Get("TEXT_SHOW_AS_DISPLAY", "Show As <color=#808080>Name</color> / Display Text")
            : Main.Lang.Get("TEXT_SHOW_AS_NAME", "Show As Name / <color=#808080>Display Text</color>");
        if(Drawer.Button(showAs)) {
            Main.Settings.ShowTextNameAsDisplayText = !Main.Settings.ShowTextNameAsDisplayText;
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        bool isRepaint = Event.current.type == EventType.Repaint;
        if(dragSoltNeedInit) {
            dragSoltNeedInit = false;
            dragSoltRange = new int[profile.ObjectManager.Count];
        }

        for(int i = 0; i < profile.ObjectManager.Count; i++) {
            var obj = profile.ObjectManager.Get(i);
            if(obj is null) {
                GUILayout.Label($"[{Main.Lang.Get("ERROR", "Error")}] " +
                    string.Format(Main.Lang.Get("ERROR_THIS_OBJECT_INDEX", "Unable to load object data at index {0}"), i));
                continue;
            }

            if(i != dragSoltDragging) {
                if(i == dragSoltInsert) {
                    GUILayout.BeginHorizontal();
                    bool dummy = false;
                    Color oldd = GUI.color;
                    GUI.color = Color.black;
                    Drawer.DrawOnlyBool(ref dummy);
                    GUI.color = oldd;
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                }
                GUILayout.BeginHorizontal();
                if(Drawer.DrawOnlyBool(ref obj.Config.Active)) {
                    obj.gameObject.SetActive(obj.Config.Active);
                }
                GUILayout.Label(GetObjectIcon(obj));
                GUILayout.Space(4);
                GUILayout.Label("-===-", GUI.skin.label);
                if(dragSoltDragging < 0 && Event.current.type == EventType.MouseDown && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition)) {
                    dragSoltDragging = i;
                    dragSoltInsert = i;
                }
                GUILayout.Space(6);
                GUI.color = new Color(0.8f, 0.8f, 1f);
                if(Drawer.Button(Drawer.Icon_Pencil, GUILayout.Width(80))) {
                    switch(obj) {
                        case OverlayerText text:
                            Main.GUI.Push(new TextConfigDrawer(text));
                            break;
                        case OverlayerImage image:
                            Main.GUI.Push(new ImageConfigDrawer(image));
                            break;
                    }
                }
                GUI.color = new Color(0.8f, 1f, 0.8f);
                if(Drawer.Button(Drawer.Icon_Copy, GUILayout.Width(46))) {
                    switch(obj.Config) {
                        case TextConfig cfg:
                            profile.ObjectManager.Create((TextConfig)cfg.Copy());
                            break;
                        case ImageConfig cfg:
                            profile.ObjectManager.Create((ImageConfig)cfg.Copy());
                            break;
                    }
                    dragSoltNeedInit = true;
                }
                GUI.color = new Color(1f, 0.8f, 0.8f);
                if(Drawer.Button(Drawer.Icon_X, GUILayout.Width(46))) {
                    if(Event.current.shift) {
                        profile.ObjectManager.Destroy(obj);
                    } else if(UnityEngine.Object.FindAnyObjectByType<DeletePopup>() is null) {
                        var popup = new GameObject().AddComponent<DeletePopup>();
                        UnityEngine.Object.DontDestroyOnLoad(popup);
                        popup.Initialize(obj, () => dragSoltNeedInit = true);
                    }
                    return;
                }
                GUI.color = old;
                GUILayout.Label(GetObjectName(obj));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
            if(Event.current.type == EventType.Repaint) {
                dragSoltRange[i] = Mathf.RoundToInt(GUILayoutUtility.GetLastRect().y);
            }
        }

        if(dragSoltInsert == profile.ObjectManager.Count) {
            GUILayout.BeginHorizontal();
            bool dummy = false;
            Color oldd = GUI.color;
            GUI.color = Color.black;
            Drawer.DrawOnlyBool(ref dummy);
            GUI.color = oldd;
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        if(dragSoltDragging >= 0) {
            if(Event.current.type == EventType.MouseUp) {
                profile.ObjectManager.OrderByDrag(dragSoltDragging, dragSoltInsert);

                dragSoltDragging = -1;
                dragSoltInsert = -1;

                GUILayout.BeginArea(Rect.zero);
                GUILayout.BeginHorizontal();
                GUILayout.EndHorizontal();
                GUILayout.EndArea();
            } else {
                if(isRepaint) {
                    int insertIndex = -1;
                    for(int i = 0; i < dragSoltRange.Length; i++) {
                        if(Event.current.mousePosition.y - 14 <= dragSoltRange[i]) {
                            insertIndex = i;
                            break;
                        } else if(i == dragSoltRange.Length - 1) {
                            insertIndex = dragSoltRange.Length;
                            break;
                        }
                    }
                    dragSoltInsert = insertIndex;
                }

                float dragWidth = Screen.width;
                float dragHeight = 24;
                Rect dragRect = new(
                    GUILayoutUtility.GetLastRect().x,
                    Event.current.mousePosition.y - (dragHeight * 3),
                    dragWidth,
                    dragHeight
                );

                var dobj = profile.ObjectManager.Get(dragSoltDragging);

                GUILayout.BeginArea(dragRect);
                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                bool dmyActive = dobj.Config.Active;
                Drawer.DrawOnlyBool(ref dmyActive);
                GUILayout.Label(GetObjectIcon(dobj));
                GUILayout.Space(4);
                GUILayout.Label("-===-", GUI.skin.label);
                GUILayout.Space(6);
                GUI.color = new Color(0.8f, 0.8f, 1f);
                Drawer.ButtonDummy(Drawer.Icon_Pencil, GUILayout.Width(80));
                GUI.color = new Color(0.8f, 1f, 0.8f);
                Drawer.ButtonDummy(Drawer.Icon_Copy, GUILayout.Width(46));
                GUI.color = new Color(1f, 0.8f, 0.8f);
                Drawer.ButtonDummy(Drawer.Icon_X, GUILayout.Width(46));
                GUI.color = old;
                GUILayout.Label(GetObjectName(dobj));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.EndArea();
            }
        } else {
            GUILayout.BeginArea(Rect.zero);
            GUILayout.BeginHorizontal();
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        if(needCreateNewObject != null) {
            if(needCreateNewObject == typeof(TextConfig)) {
                var cfg = new TextConfig();
                profile.ObjectManager.Create(cfg);
            } else if(needCreateNewObject == typeof(ImageConfig)) {
                var cfg = new ImageConfig();
                profile.ObjectManager.Create(cfg);
            }
            profile.ObjectManager.Refresh();
            dragSoltNeedInit = true;
        }

        NeoDrawer.StaticInstance.UpdateFocused();
    }
    private string GetObjectName(OverlayerObject obj) {
        if(obj is OverlayerText text) {
            if(Main.Settings.ShowTextNameAsDisplayText) {
                if(text.Config.Active) {
                    string current = text.GetCurrentText();
                    string objName = current?.BreakRichTag();
                    if(string.IsNullOrEmpty(objName)) {
                        objName = Main.Lang.Get("TEXT_EMPTY", "<color=#808080>[ empty ]</color>");
                    }
                    objName = objName.Replace('\n', ' ');
                    if(objName?.Length > 62) {
                        objName = objName.Substring(0, 62) +
                            $"<color=#808080>..({objName.Length - 62})</color>";
                    }
                    return objName;
                }
                return Main.Lang.Get("TEXT_INACTIVE", "<i><color=#808080>[ inactive ]</color></i>");
            }
            return text.Config.Active
                ? text.Config.Name
                : $"<color=#808080>{text.Config.Name}</color>";
        }
        var cfg = obj.Config;
        return cfg.Active
            ? cfg.Name
            : $"<color=#808080>{cfg.Name}</color>";
    }
    private Texture2D GetObjectIcon(OverlayerObject obj) => obj is OverlayerText ? Drawer.Icon_Font : obj is OverlayerImage ? Drawer.Icon_Image : null;
}
