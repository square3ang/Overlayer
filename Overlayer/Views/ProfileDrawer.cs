using Newtonsoft.Json.Linq;
using Overlayer.Core;
using Overlayer.Models;
using Overlayer.Unity;
using Overlayer.Utils;
using SFB;
using System;
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
        bool needCreateNewText = Drawer.Button(Drawer.Icon_Plus, GUILayout.Width(100));
		GUI.color = new Color(1f, 1f, 0.8f);
		if(Drawer.Button(Drawer.Icon_Down, GUILayout.Width(60))) {
			StandaloneFileBrowser.OpenFilePanelAsync(
				Main.Lang.Get("SELECT_TEXT", "Select Text"),
				Main.Mod.Path,
				new[] { new ExtensionFilter(Main.Lang.Get("OVERLAYER_TEXT_JSON", "Overlayer Text JSON"), "json") },
				true,
				async (texts) => {
					foreach(var text in texts) {
						await Task.Run(() => {
							try {
								if(Path.GetExtension(text) != ".json") {
									return;
								}

								var content = File.ReadAllText(text);
								if(string.IsNullOrWhiteSpace(content)) {
									return;
								}

								var json = JToken.Parse(content);

								Main.MainThreadDispatcher.Enqueue(() => {
									try {
										if(json is JArray arr) {
											ModelUtils.UnwrapList<TextConfig>(arr)
												.ForEach(t => profile.TextManager.Create(t));
										} else if(json is JObject obj) {
											profile.TextManager.Create(TextConfigImporter.Import(obj));
										}

										dragSoltNeedInit = true;
										profile.TextManager.Refresh();
									} catch(Exception ex) {
										Debug.LogError($"Failed to create text from '{text}': {ex}");
									}
								});

							} catch(Exception e) {
								Debug.LogError($"Failed to load text '{text}': {e}");
							}
						});
					}
				}
			);
		}
		GUI.color = old;
        string showAs = Main.Settings.showTextNameAsDisplayText
            ? Main.Lang.Get("TEXT_SHOW_AS_DISPLAY", "Show As <color=#808080>Name</color> / Display Text")
            : Main.Lang.Get("TEXT_SHOW_AS_NAME", "Show As Name / <color=#808080>Display Text</color>");
        if(Drawer.Button(showAs)) {
            Main.Settings.showTextNameAsDisplayText = !Main.Settings.showTextNameAsDisplayText;
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        bool isRepaint = Event.current.type == EventType.Repaint;
        if(dragSoltNeedInit) {
            dragSoltNeedInit = false;
            dragSoltRange = new int[profile.TextManager.Count];
        }

        for(int i = 0; i < profile.TextManager.Count; i++) {
            var text = profile.TextManager.Get(i);
            if(text == null) {
                GUILayout.Label($"[{Main.Lang.Get("ERROR", "Error")}] " + string.Format(Main.Lang.Get("ERROR_THIS_TEXT_INDEX", "Unable to load text data at index {0}"), i.ToString()));
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
                if(Drawer.DrawOnlyBool(ref text.Config.Active)) {
                    text.gameObject.SetActive(text.Config.Active);
                }
                GUILayout.Label("-==-", GUI.skin.label);
                if(dragSoltDragging < 0 && Event.current.type == EventType.MouseDown && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition)) {
                    dragSoltDragging = i;
                    dragSoltInsert = i;
                }
                GUILayout.Space(6);
                GUI.color = new Color(0.8f, 0.8f, 1f);
                if(Drawer.Button(Drawer.Icon_Pencil, GUILayout.Width(80))) {
                    Main.GUI.Push(new TextConfigDrawer(text));
                }
                GUI.color = new Color(0.8f, 1f, 0.8f);
                if(Drawer.Button(Drawer.Icon_Copy, GUILayout.Width(46))) {
                    profile.TextManager.Create(text.Config.Copy());
                    dragSoltNeedInit = true;
                }
                GUI.color = new Color(1f, 0.8f, 0.8f);
                if(Drawer.Button(Drawer.Icon_X, GUILayout.Width(46))) {
                    if(Event.current.shift) {
                        profile.TextManager.Destroy(text);
                    } else {
                        if(UnityEngine.Object.FindAnyObjectByType<DeletePopup>() == null) {
                            var popup = new GameObject().AddComponent<DeletePopup>();
                            UnityEngine.Object.DontDestroyOnLoad(popup);
                            popup.Initialize(text, () => dragSoltNeedInit = true);
                        }
                    }
                    return;
                }
                GUI.color = old;
                string textName;
                if(Main.Settings.showTextNameAsDisplayText) {
                    if(text.Config.Active) {
                        string current = text.GetCurrentText();
                        textName = current?.BreakRichTag();
                        if(string.IsNullOrEmpty(textName)) {
                            textName = Main.Lang.Get("TEXT_EMPTY", "<color=#808080>[ empty ]</color>");
                        }
                        textName = textName.Replace('\n', ' ');
                        if(textName?.Length > 62) {
                            textName = textName.Substring(0, 62) + $"<color=#808080>..({textName.Length - 62})</color>"; //TODO: Scroling Name
                        }
                    } else {
                        textName = Main.Lang.Get("TEXT_INACTIVE", "<i><color=#808080>[ inactive ]</color></i>");
                    }
                } else {
                    textName = text.Config.Active ? text.Config.Name : $"<color=#808080>{text.Config.Name}</color>";
                }
                GUILayout.Label(textName);

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
            if(Event.current.type == EventType.Repaint) {
                dragSoltRange[i] = Mathf.RoundToInt(GUILayoutUtility.GetLastRect().y);
            }
        }

        if(dragSoltInsert == profile.TextManager.Count) {
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
                profile.TextManager.OrderByDrag(dragSoltDragging, dragSoltInsert);

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

                var dtxt = profile.TextManager.Get(dragSoltDragging);

                GUILayout.BeginArea(dragRect);
                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                bool dmyActive = dtxt.Config.Active;
                Drawer.DrawOnlyBool(ref dmyActive);
                GUILayout.Label("-==-", GUI.skin.label);
                GUILayout.Space(6);
                GUI.color = new Color(0.8f, 0.8f, 1f);
                Drawer.ButtonDummy(Drawer.Icon_Pencil, GUILayout.Width(80));
                GUI.color = new Color(0.8f, 1f, 0.8f);
                Drawer.ButtonDummy(Drawer.Icon_Copy, GUILayout.Width(46));
                GUI.color = new Color(1f, 0.8f, 0.8f);
                Drawer.ButtonDummy(Drawer.Icon_X, GUILayout.Width(46));
                GUI.color = old;
                string textName;
                if(Main.Settings.showTextNameAsDisplayText) {
                    if(dtxt.Config.Active) {
                        string current = dtxt.GetCurrentText();
                        textName = current?.BreakRichTag();
                        if(string.IsNullOrEmpty(textName)) {
                            textName = Main.Lang.Get("TEXT_EMPTY", "<color=#808080>[ empty ]</color>");
                        }
                        textName = textName.Replace('\n', ' ');
                        if(textName?.Length > 62) {
                            textName = textName.Substring(0, 62) + $"<color=#808080>..({textName.Length - 62})</color>";
                        }
                    } else {
                        textName = Main.Lang.Get("TEXT_INACTIVE", "<i><color=#808080>[ inactive ]</color></i>");
                    }
                } else {
                    textName = dtxt.Config.Active ? dtxt.Config.Name : $"<color=#808080>{dtxt.Config.Name}</color>";
                }
                GUILayout.Label(textName);
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

        if(needCreateNewText) {
            profile.TextManager.Create(new TextConfig());
            profile.TextManager.Refresh();
            dragSoltNeedInit = true;
        }

        NeoDrawer.StaticInstance.UpdateFocused();
    }
}
