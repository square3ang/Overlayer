using Newtonsoft.Json;
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

public class ImageConfigDrawer : ModelDrawable<ImageConfig> {
    public OverlayerImage image;

    public ImageConfigDrawer(OverlayerImage image) : base((ImageConfig)image.Config) => this.image = image;

    public override void OnceCall() => NeoDrawer.StaticInstance.FieldResetDictById();

    public override void Draw() {
        NeoDrawer.StaticInstance.FieldResetId();

        Color old = GUI.color;
        if(Drawer.DrawBool(Drawer.Icon_Power, Main.Lang.Get("ACTIVE", "Active"), ref model.Active)) {
            image.gameObject.SetActive(model.Active);
        }
        bool _drag = model.Drag;
        Drawer.DrawBool(Drawer.Icon_Drag, Main.Lang.Get("DRAG", "Drag"), ref _drag);
        if(model.Drag != _drag) {
            model.Drag = _drag;
        }
        Drawer.DrawString(Drawer.Icon_Pencil, Main.Lang.Get("NAME", "Name"), ref model.Name);
        bool changed = false;
        changed |= NeoDrawer.StaticInstance.DrawSize2(Main.Lang.Get("POSITION", "Position"), ref model.Position, 0, 1);
        changed |= NeoDrawer.StaticInstance.DrawSize2(Main.Lang.Get("SCALE", "Scale"), ref model.Scale, 0, 10);
        changed |= NeoDrawer.StaticInstance.DrawSize2(Main.Lang.Get("PIVOT", "Pivot"), ref model.Pivot, 0, 1);
        changed |= NeoDrawer.StaticInstance.DrawRotate3(Main.Lang.Get("ROTATION", "Rotation"), ref model.Rotation, -180, 180);
        changed |= NeoDrawer.StaticInstance.DrawColor(ref model.Color);
        GUILayout.BeginHorizontal();
        if(Drawer.Button(Drawer.Icon_Image, GUILayout.Width(40))) {
            System.Threading.Tasks.Task.Run(() => {
                var extensions = new[]
                {
                    new ExtensionFilter("Image Files", "png", "jpg", "jpeg"),
                    new ExtensionFilter("All Files", "*")
                };
                string baseDir = Path.Combine(Main.Mod.Path, "Overlayer");
                string[] paths = StandaloneFileBrowser.OpenFilePanel(
                    Main.Lang.Get("SELECT_IMAGES", "Select Images"),
                    baseDir,
                    extensions,
                    true
                );
                if(paths.Length > 0) {
                    Main.MainThreadDispatcher.Enqueue(() => {
                        foreach(var path in paths) {
                            string p = path.StartsWith(Main.Mod.Path)
                                ? path.Replace(Main.Mod.Path, "{ModDir}").Replace("\\", "/")
                                : path;
                            model.Images.Add(p);
                            this.image.ApplyImages();
                        }
                        changed = true;
                    });
                }
            });
        }
        if(Drawer.Button(Drawer.Icon_Plus, GUILayout.Width(50))) {
            model.Images.Add(string.Empty);
            changed = true;
        }
        GUILayout.EndHorizontal();
        for(int i = 0; i < model.Images.Count; i++) {
            GUILayout.BeginHorizontal();

            GUI.color = new Color(1f, 0.8f, 0.8f);
            if(Drawer.Button(Drawer.Icon_X, GUILayout.Width(46))) {
                model.Images.RemoveAt(i);
                this.image.ApplyImages();
                i--;
                GUILayout.EndHorizontal();
                continue;
            }

            GUI.color = old;

            if(Drawer.Button(Drawer.Icon_OpenFolder, GUILayout.Width(40))) {
                int index = i;
                var currentModel = model;

                Task.Run(() => {
                    var extensions = new[] {
                        new ExtensionFilter("Image Files", "png", "jpg", "jpeg"),
                        new ExtensionFilter("All Files", "*")
                    };
                    string baseDir = Path.Combine(Main.Mod.Path, "Overlayer");
                    string[] paths = StandaloneFileBrowser.OpenFilePanel(
                        Main.Lang.Get("SELECT_IMAGE_FILE", "Select Image File"),
                        baseDir,
                        extensions,
                        false
                    );

                    if(paths.Length > 0) {
                        string path = paths[0];
                        if(path.StartsWith(Main.Mod.Path)) {
                            path = path.Replace(Main.Mod.Path, "{ModDir}").Replace("\\", "/");
                        }

                        string finalPath = path;
                        Main.MainThreadDispatcher.Enqueue(() => {
                            if(index < currentModel.Images.Count) {
                                currentModel.Images[index] = finalPath;
                            }
                            this.image.ApplyImages();
                        });
                    }
                });
            }

            string img = model.Images[i];
            if(Drawer.DrawOnlyString(ref img)) {
                model.Images[i] = img;
                this.image.ApplyImages();
            }

            GUILayout.EndHorizontal();
        }

        changed |= Drawer.DrawCodeEditor(Drawer.Icon_Play, Main.Lang.Get("PLAYING_COMMAND", "Playing Command"), model.Name + "PlayingCommand", ref model.PlayingCommand);
        changed |= Drawer.DrawCodeEditor(Drawer.Icon_Pause, Main.Lang.Get("NOT_PLAYING_COMMAND", "Not Playing Command"), model.Name + "NotPlayingCommand", ref model.NotPlayingCommand);
        GUILayout.BeginHorizontal();
        GUI.color = new Color(1f, 0.8f, 1f);
        if(Drawer.Button(Drawer.Icon_Up, GUILayout.Width(46))) {
            Task.Run(() => {
                string target = StandaloneFileBrowser.SaveFilePanel(
                    Main.Lang.Get("EXPORT_IMAGE_CONFIG", "Export Image Config"),
                    Persistence.GetLastUsedFolder(),
                    $"{model.Name}.json", "json"
                );
                if(!string.IsNullOrWhiteSpace(target)) {
                    JObject node = model.Serialize() as JObject;
                    node["Type"] = "Image";
                    node["Reference"] = null;
                    File.WriteAllText(target, JsonConvert.SerializeObject(node, Formatting.Indented));
                }
            });
        }
        GUI.color = new Color(1f, 0.8f, 0.8f);
        if(Drawer.Button(Drawer.Icon_X, GUILayout.Width(46))) {
            image.Parent.ObjectManager.Destroy(image);
            Main.GUI.Skip(frames: 2);
            Main.GUI.Pop();
            return;
        }
        GUI.color = old;
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        if(changed) {
            image.ApplyConfig();
        }

        NeoDrawer.StaticInstance.UpdateFocused();
    }
}