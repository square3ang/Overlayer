using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using HarmonyLib;
using Newtonsoft.Json.Linq;
using UImGui;
using UImGui.Assets;
using UImGui.Texture;
using UnityEngine;
using UnityEngine.UI;
using UnityModManagerNet;
using Object = UnityEngine.Object;
using Random = System.Random;
using Assembly = System.Reflection.Assembly;
using Lua;
using Lua.Standard;

namespace Overlayer
{
    public class Main
    {
        public static UnityModManager.ModEntry ModEntry;

        public static Harmony HarmonyInstance;

        private static List<IntPtr> libraries = new List<IntPtr>();

        public static AssetBundle ab;
        
        private static Canvas canv;

        private static GameObject ImGUIManager;

        public static LuaState lua;

        public static Dictionary<string, OverlayerTag> Tags = new();

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr LoadLibrary(string lpLibFileName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FreeLibrary(IntPtr hModule);

        public static bool IsPlaying
        {
            get
            {
                if (Application.isEditor) return true;
                if (OverlayerSettings.open && OverlayerSettings.sim_play) return true;
                var ctrl = scrController.instance;
                var cdt = scrConductor.instance;
                if (ctrl && cdt)
                    return !ctrl.paused && cdt.isGameWorld;
                return false;
            }
        }


        static void LD(string nam)
        {
            var lib = LoadLibrary(Path.Combine(ModEntry.Path, "Native", nam));
            if (lib == IntPtr.Zero)
            {
                ModEntry.Logger.Log($"Failed to load {nam}");
                return;
            }

            libraries.Add(lib);
        }

        static IEnumerator later()
        {
            yield return null;
        }

        private static string savePath = "";
        private static string scriptPath = "";

        public static void RegisterTag(OverlayerTag tag)
        {
            Tags.Add(tag.Name, tag);
            ModEntry.Logger.Log("Registered tag " + tag.Name);
        }

        public static void UnregisterTag(string name)
        {
            Tags.Remove(name);
            ModEntry.Logger.Log("Unregistered tag " + name);
        }

        public static int IntTryParseOrZero(string str)
        {
            return int.TryParse(str, out var i) ? i : 0;
        }
        
        public static void OnApplicationQuit()
        {
            Save();
        }

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            UImGui.UImGui.modFolder = modEntry.Path;
            ModEntry = modEntry;
            savePath = Path.Combine(modEntry.Path, "Objs.json");
            scriptPath = Path.Combine(modEntry.Path, "Scripts");

            modEntry.OnToggle = (_, val) =>
            {
                if (val)
                {
                    HarmonyInstance = new Harmony(modEntry.Info.Id);
                    HarmonyInstance.PatchAll();
                    LD("cimgui.dll");
                    ab = AssetBundle.LoadFromFile(Path.Combine(ModEntry.Path, "overlayer"));
                    canv = Object.Instantiate(ab.LoadAsset<GameObject>("OverlayerCanvas")).GetComponent<Canvas>();
                    ImGUIManager = Object.Instantiate(ab.LoadAsset<GameObject>("IMGUI"));
                    ImGUIManager.GetComponentInChildren<UImGui.UImGui>().Reload();
                    /*ImGUIManager.AddComponent<SettingGUIManager>().cam =
                        ImGUIManager.transform.Find("SettingCam").GetComponent<Camera>();*/
                    
                    ImGUIManager.AddComponent<SettingGUIManager>().rimg =
                        canv.transform.Find("RawImage").GetComponent<RawImage>();
                    ImGUIManager.GetComponent<SettingGUIManager>().Init();

                    foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
                    {
                        foreach (var fld in type.GetFields())
                        {
                            var attr = fld.GetCustomAttribute<TagAttribute>();
                            if (attr == null) continue;

                            OverlayerTag tag = null;

                            if (string.IsNullOrEmpty(attr.Name))
                            {
                                attr.Name = fld.Name;
                            }

                            if (fld.FieldType == typeof(float))
                            {
                                tag = new OverlayerTag()
                                {
                                    Name = attr.Name,
                                    //Description = attr.Description,
                                    Action = a =>
                                    {
                                        var shouldDummy = OverlayerSettings.open && attr.Dummy != null || !IsPlaying && !attr.NotPlaying; 
                                        if (string.IsNullOrEmpty(a))
                                        {
                                            return (shouldDummy ? (float)attr.Dummy : (float)fld.GetValue(null)).ToString();
                                        }

                                        var fix = false;
                                        if (a.EndsWith("|"))
                                        {
                                            fix = true;
                                            a = a.TrimEnd('|');
                                        }

                                        var parsed = IntTryParseOrZero(a);
                                        return (shouldDummy ? (float)attr.Dummy : (float)fld.GetValue(null))
                                            .ToString(parsed == 0
                                                ? "0"
                                                : "0." + new string(fix ? '0' : '#', parsed));
                                    },
                                    NotPlaying = attr.NotPlaying
                                };
                            }
                            else if (fld.FieldType == typeof(double))
                            {
                                tag = new OverlayerTag()
                                {
                                    Name = attr.Name,
                                    //Description = attr.Description,
                                    Action = a =>
                                    {
                                        var shouldDummy = OverlayerSettings.open && attr.Dummy != null || !IsPlaying && !attr.NotPlaying;
                                        if (string.IsNullOrEmpty(a))
                                        {
                                            return (shouldDummy ? (double)attr.Dummy : (double)fld.GetValue(null)).ToString();
                                        }

                                        var fix = false;
                                        if (a.EndsWith("|"))
                                        {
                                            fix = true;
                                            a = a.TrimEnd('|');
                                        }

                                        var parsed = IntTryParseOrZero(a);
                                        return (shouldDummy ? (double)attr.Dummy : (double)fld.GetValue(null))
                                            .ToString(parsed == 0
                                                ? "0"
                                                : "0." + new string(fix ? '0' : '#', parsed));
                                    },
                                    NotPlaying = attr.NotPlaying
                                };
                            }
                            else
                            {
                                tag = new OverlayerTag()
                                {
                                    Name = attr.Name,
                                    //Description = attr.Description,
                                    Action = a =>
                                    {
                                        var shouldDummy = OverlayerSettings.open && attr.Dummy != null || !IsPlaying && !attr.NotPlaying;
                                        return (shouldDummy ? attr.Dummy : fld.GetValue(null)).ToString();
                                    },
                                    NotPlaying = attr.NotPlaying
                                };
                            }

                            RegisterTag(tag);
                        }

                        foreach (var mtd in type.GetMethods())
                        {
                            var attr = mtd.GetCustomAttribute<TagAttribute>();
                            if (attr == null) continue;

                            

                            OverlayerTag tag = null;

                            if (string.IsNullOrEmpty(attr.Name))
                            {
                                attr.Name = mtd.Name;
                            }

                            if (mtd.ReturnType == typeof(string))
                            {
                                tag = new OverlayerTag()
                                {
                                    Name = attr.Name,
                                    //Description = attr.Description,
                                    Action = a =>
                                    {
                                        try
                                        {
                                            var shouldDummy = OverlayerSettings.open && attr.Dummy != null || !IsPlaying && !attr.NotPlaying;
                                            var args = a.Split(',');
                                            var idx = 0;
                                            var realArgs = new object[mtd.GetParameters().Length];
                                            foreach (var p in mtd.GetParameters())
                                            {
                                                //ModEntry.Logger.Log("Parsing " + p.Name + " " + p.ParameterType);
                                                if (idx >= args.Length || string.IsNullOrEmpty(a))
                                                {
                                                    if (p.DefaultValue == null)
                                                    {
                                                        ModEntry.Logger.Error("Method " + mtd.Name + " expected " +
                                                                              mtd.GetParameters().Length +
                                                                              " arguments, but got " +
                                                                              args.Length + " and DefaultValue is null!");
                                                        return "Error";
                                                    }
                                                    realArgs[idx] = p.DefaultValue;
                                                    //ModEntry.Logger.Log("Parsed " + p.Name + " into " + realArgs[idx]);
                                                    //ModEntry.Logger.Log("Idx is " + idx + " and realArgs length is " + realArgs.Length);
                                                    idx++;
                                                    continue;
                                                }

                                                if (p.ParameterType == typeof(int))
                                                {
                                                    realArgs[idx] = int.Parse(args[idx]);
                                                }
                                                else if (p.ParameterType == typeof(double))
                                                {
                                                    realArgs[idx] = double.Parse(args[idx]);
                                                }
                                                else if (p.ParameterType == typeof(float))
                                                {
                                                    realArgs[idx] = float.Parse(args[idx]);
                                                }
                                                else if (p.ParameterType == typeof(string))
                                                {
                                                    realArgs[idx] = args[idx];
                                                }
                                                else if (p.ParameterType == typeof(bool))
                                                {
                                                    realArgs[idx] = bool.Parse(args[idx]);
                                                }
                                                else if (p.ParameterType.IsEnum)
                                                {
                                                    realArgs[idx] = Enum.Parse(p.ParameterType, args[idx]);
                                                }
                                                else
                                                {
                                                    ModEntry.Logger.Error("Unsupported parameter type " +
                                                                          p.ParameterType);
                                                    return "Error";
                                                }
                                                //ModEntry.Logger.Log("Parsed " + p.Name + " into " + realArgs[idx]);
                                                /*ModEntry.Logger.Log("Idx is " + idx + " and realArgs length is " +
                                                                    realArgs.Length);*/
                                                
                                                idx++;
                                            }

                                            if (mtd.GetParameters().Length != realArgs.Length)
                                            {
                                                /*ModEntry.Logger.Error("Method " + mtd.Name + " expected " +
                                                                      mtd.GetParameters().Length +
                                                                      " arguments, but got " +
                                                                      realArgs.Length);*/
                                                return "Error";
                                            }

                                            try
                                            {

                                                return shouldDummy
                                                    ? attr.Dummy.ToString()
                                                    : (string)mtd.Invoke(null, realArgs);
                                            }
                                            catch (Exception ex)
                                            {
                                                /*ModEntry.Logger.Error("Error invoking method " + mtd.Name + ": " + ex);
                                                var str = "";
                                                foreach (var aa in realArgs)
                                                {
                                                    if (aa == null)
                                                    {
                                                        str += "null | ";
                                                        continue;
                                                    }
                                                    str += aa + " " + aa.GetType().Name + " | ";
                                                }
                                                ModEntry.Logger.Log("Arguments: " + a + " and parsed: " + str);*/
                                                return "Error";
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            /*ModEntry.Logger.Error("Error parsing arguments for " + mtd.Name + ": " + ex);
                                            ModEntry.Logger.Log("Arguments: " + a);*/
                                            return "Error";
                                        }
                                    },
                                    NotPlaying = attr.NotPlaying
                                };
                            }
                            else if (mtd.ReturnType == typeof(float))
                            {
                                tag = new OverlayerTag()
                                {
                                    Name = attr.Name,
                                    //Description = attr.Description,
                                    Action = a =>
                                    {
                                        var shouldDummy = OverlayerSettings.open && attr.Dummy != null || !IsPlaying && !attr.NotPlaying;
                                        if (string.IsNullOrEmpty(a))
                                        {
                                            return (shouldDummy ? (float)attr.Dummy : (float)mtd.Invoke(null, null)).ToString();
                                        }

                                        var fix = false;
                                        if (a.EndsWith("|"))
                                        {
                                            fix = true;
                                            a = a.TrimEnd('|');
                                        }

                                        var parsed = IntTryParseOrZero(a);
                                        return (shouldDummy ? (float)attr.Dummy : (float)mtd.Invoke(null, null))
                                            .ToString(parsed == 0 ? "0" : "0." + new string(fix ? '0' : '#', parsed));
                                    },
                                    NotPlaying = attr.NotPlaying
                                };
                            }
                            else if (mtd.ReturnType == typeof(double))
                            {
                                tag = new OverlayerTag()
                                {
                                    Name = attr.Name,
                                    //Description = attr.Description,
                                    Action = a =>
                                    {
                                        var shouldDummy = OverlayerSettings.open && attr.Dummy != null || !IsPlaying && !attr.NotPlaying;
                                        if (string.IsNullOrEmpty(a))
                                        {
                                            return (shouldDummy ? (double)attr.Dummy : (double)mtd.Invoke(null, null)).ToString();
                                        }

                                        var fix = false;
                                        if (a.EndsWith("|"))
                                        {
                                            fix = true;
                                            a = a.TrimEnd('|');
                                        }

                                        var parsed = IntTryParseOrZero(a);
                                        return (shouldDummy ? (double)attr.Dummy : (double)mtd.Invoke(null, null))
                                            .ToString(parsed == 0 ? "0" : "0." + new string(fix ? '0' : '#', parsed));
                                    },
                                    NotPlaying = attr.NotPlaying
                                };
                            }
                            else
                            {
                               
                                tag = new OverlayerTag()
                                {
                                    Name = attr.Name,
                                    //Description = attr.Description,
                                    Action = a =>
                                    {
                                        try
                                        {
                                            var shouldDummy = OverlayerSettings.open && attr.Dummy != null || !IsPlaying && !attr.NotPlaying;
                                            return (shouldDummy ? attr.Dummy : mtd.Invoke(null, null)).ToString();
                                        }
                                        catch
                                        {
                                            ModEntry.Logger.Log("name " + mtd.Name);
                                            return "Error";
                                        }
                                    },
                                    NotPlaying = attr.NotPlaying
                                };
                            }

                            RegisterTag(tag);
                        }
                    }

                    lua = LuaState.Create();
                    lua.OpenStandardLibraries();
                    //lua.Environment["TagMgr"] = new LuaValue(new LuaTagMgr());
                    lua.Environment["GetTagValue"] = new LuaFunction((a, b, c) =>
                    {
                        var tag = a.GetArgument(0).ToString();
                        var arg = a.GetArgument(1).ToString();
                        Main.Tags.TryGetValue(tag, out var t);
                        b.Span[0] = new LuaValue(t?.Action(arg) ?? "");
                        return new(1);
                    });
                    lua.Environment["RegisterTag"] = new LuaFunction((a, b, c) =>
                    {
                        var name = a.GetArgument<string>(0);
                        var action = a.GetArgument<LuaFunction>(1);
                        var notPlaying = a.GetArgument<bool>(2);
                        RegisterTag(new OverlayerTag()
                        {
                            Name = name,
                            //Description = desc,
                            Action = a =>
                            {
                                var task = action.InvokeAsync(Main.lua, new[] { new LuaValue(a) }, cancellationToken: c).AsTask();
                                task.Wait(c);
                                return task.Result[0].ToString();
                            },
                            NotPlaying = notPlaying
                        });
                        return new(1);
                    });
                    lua.Environment["UnregisterTag"] = new LuaFunction((a, b, c) =>
                    {
                        var name = a.GetArgument(0).ToString();
                        UnregisterTag(name);
                        return new(0);
                    });
                    if (!Directory.Exists(scriptPath)) Directory.CreateDirectory(scriptPath);
                    foreach (var file in Directory.GetFiles(scriptPath))
                    {
                        modEntry.Logger.Log("Loading " + file);
                        lua.DoFileAsync(file).AsTask().ContinueWith(t =>
                        {
                            foreach (var obj in OverlayerSettings.objects)
                            {
                                if (obj is not OverlayerText txt) continue;
                                txt.PlayingText.ParseTags();
                                txt.NotPlayingText.ParseTags();
                            };
                        });
                    }

                    if (File.Exists(savePath))
                    {
                        var ja = JArray.Parse(File.ReadAllText(savePath));
                        foreach (var j in ja)
                        {
                            switch (j["Type"].Value<string>())
                            {
                                case "Text":
                                    var txt_ = Object.Instantiate(OverlayerSettings.Instance.TextTemplate,
                                        SettingGUIManager.Instance.rimg.transform.parent);
                                    txt_.transform.SetAsFirstSibling();
                                    var txt = txt_.GetComponent<OverlayerText>();
                                    txt.FromJson(j.Value<JObject>());
                                    OverlayerSettings.objects.Add(txt);
                                    break;
                            }
                        }
                    }

                    Object.DontDestroyOnLoad(canv.gameObject);
                    Object.DontDestroyOnLoad(ImGUIManager);
                    
                    Application.quitting += OnApplicationQuit;
                    
                    ADOStartup.ModWasAdded(modEntry.Info.Id);
                }
                else
                {
                    HarmonyInstance.UnpatchAll(modEntry.Info.Id);
                    foreach (var lib in libraries)
                    {
                        FreeLibrary(lib);
                    }

                    Tags.Clear();

                    Object.Destroy(ImGUIManager);
                    Object.Destroy(canv.gameObject);
                    lua = null;
                    
                    
                    Application.quitting -= OnApplicationQuit;
                    
                    ADOStartup.addedMods.Remove(modEntry.Info.Id);
                    
                    GC.Collect();
                }

                return true;
            };
            modEntry.OnGUI = _ =>
            {
                GUILayout.Label("<size=50>Overlayer v" + modEntry.Info.Version + "</size>");
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Open Settings"))
                {
                    OverlayerSettings.open = true;
                }

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            };
            modEntry.OnSaveGUI = _ => { Save(); };
            return true;
        }

        public static void Save()
        {
            ModEntry.Logger.Log("Saving...");
            var ja = new JArray();
            foreach (var obj in OverlayerSettings.objects)
            {
                ja.Add(obj.ToJson());
            }

            File.WriteAllText(savePath, ja.ToString());
            
            ModEntry.Logger.Log("Saved!");
        }

        [HarmonyPatch(typeof(scrController), "CountValidKeysPressed")]
        public static class KeyBlock
        {
            public static bool Prefix(ref int __result)
            {
                if (!OverlayerSettings.open) return true;
                __result = 0;
                return false;
            }
        }

        [HarmonyPatch(typeof(scnLevelSelect), "JumpAndWipeWithKey")]
        public static class JumpBlock
        {
            public static bool Prefix()
            {
                return !OverlayerSettings.open;
            }
        }
    }
}