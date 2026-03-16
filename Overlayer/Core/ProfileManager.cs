using Newtonsoft.Json.Linq;
using Overlayer.Models;
using Overlayer.Unity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Overlayer.Core;

public static class ProfileManager {
    public static bool Initialized { get; private set; }
    public static List<OverlayerProfile> Profiles;
    public static int Count => Profiles.Count;

    public static void Initialize() {
        if(Initialized) {
            return;
        }

        Profiles = new List<OverlayerProfile>();
        if(!Directory.Exists(Main.ProfilePath)) {
            Directory.CreateDirectory(Main.ProfilePath);
        }

        foreach(var file in Directory.GetFiles(Main.ProfilePath, "*.json")) {
            var content = File.ReadAllText(file);
            if(string.IsNullOrWhiteSpace(content)) {
                continue;
            }

            var token = JToken.Parse(content);
            var cfg = new ProfileConfig();
            cfg.Deserialize(token);
            cfg.Path = file;
            cfg.Name = Path.GetFileNameWithoutExtension(file);

            var profileGO = new GameObject(cfg.Name ?? "Profile");
            var profile = profileGO.AddComponent<OverlayerProfile>();
            profile.Config = cfg;
            profile.Init(cfg.Name);
            profile.ObjectManager.Import(cfg.Objects);

            Profiles.Add(profile);
        }

        Initialized = true;
    }

    public static OverlayerProfile Create(ProfileConfig config) {
        if(string.IsNullOrWhiteSpace(config.Name) || Exists(config.Name)) {
            Debug.LogWarning("Profile name is invalid or already exists.");
            return null;
        }

        var cfg = new ProfileConfig {
            Name = config.Name,
            Path = Path.Combine(Main.ProfilePath, config.Name + ".json")
        };

        var profileGO = new GameObject(config.Name);
        var profile = profileGO.AddComponent<OverlayerProfile>();
        profile.Config = cfg;
        profile.Init(config.Name);
        profile.ObjectManager.Import(cfg.Objects);

        Profiles.Add(profile);

        return profile;
    }

    public static OverlayerProfile Get(int index) => (index >= 0 && index < Count) ? Profiles[index] : null;

    public static bool OrderToIndex(int from, int to) {
        if(from < 0 || from >= Count || to < 0 || to >= Count || from == to) {
            return false;
        }

        var item = Profiles[from];
        Profiles.RemoveAt(from);
        Profiles.Insert(to, item);

        item.gameObject.transform.SetSiblingIndex(to);

        return true;
    }
    public static bool OrderUp(int index) => OrderToIndex(index, index - 1);
    public static bool OrderDown(int index) => OrderToIndex(index, index + 1);
    public static bool OrderToTop(int index) => OrderToIndex(index, 0);
    public static bool OrderToBottom(int index) => OrderToIndex(index, Count - 1);
    public static bool OrderByDrag(int from, int to) {
        if(from < 0 || from >= Count) {
            return false;
        }

        to = Mathf.Clamp(to, 0, Count);

        if(from == to || from == to - 1) {
            return false;
        }

        var item = Profiles[from];
        Profiles.RemoveAt(from);

        if(from < to) {
            to--;
        }

        Profiles.Insert(to, item);
        item.gameObject.transform.SetSiblingIndex(to);

        return true;
    }

    public static void Destroy(OverlayerProfile profile) {
        if(profile == null || !Profiles.Contains(profile)) {
            return;
        }

        try {
            string filePath = profile.Config?.Path ?? string.Empty;
            if(!string.IsNullOrEmpty(filePath) && !Path.IsPathRooted(filePath)) {
                filePath = Path.Combine(Main.ProfilePath, filePath);
            }
            if(File.Exists(filePath)) {
                File.Delete(filePath);
            }
        } catch {
        }

        profile.ObjectManager.Release();
        Profiles.Remove(profile);
        UnityEngine.Object.Destroy(profile.gameObject);
    }

    public static void Save() {
        foreach(var profile in Profiles) {
            profile.Config.Objects = profile.ObjectManager.Export();
            JToken jsonNode = profile.Config.Serialize();

            string filePath = profile.Config.Path;
            if(string.IsNullOrEmpty(filePath)) {
                filePath = Path.Combine(Main.ProfilePath, profile.Config.Name + ".json");
                profile.Config.Path = filePath;
            } else if(!Path.IsPathRooted(filePath)) {
                filePath = Path.Combine(Main.ProfilePath, filePath);
                profile.Config.Path = filePath;
            }

            File.WriteAllText(filePath, jsonNode.ToString(Newtonsoft.Json.Formatting.Indented));
        }
    }

    public static bool Exists(string name)
        => Profiles.Any(p => string.Equals(p.Config.Name, name, StringComparison.Ordinal));

    public static bool Rename(OverlayerProfile profile, string newName) {
        if(profile == null || string.IsNullOrWhiteSpace(newName) || Profiles.Any(p => p != profile && string.Equals(p.Config.Name, newName, StringComparison.OrdinalIgnoreCase))) {
            return false;
        }

        try {
            string oldPath = profile.Config.Path;
            if(!Path.IsPathRooted(oldPath)) {
                oldPath = Path.Combine(Main.ProfilePath, oldPath);
            }

            string newPath = Path.Combine(Main.ProfilePath, newName + ".json");

            if(File.Exists(oldPath)) {
                File.Move(oldPath, newPath);
            }

            profile.Config.Name = newName;
            profile.Config.Path = newPath;
            profile.gameObject.name = newName;
            return true;
        } catch(Exception e) {
            Debug.LogError("Failed to rename profile: " + e);
            return false;
        }
    }

    public static void Refresh() {
        foreach(var profile in Profiles) {
            profile.ObjectManager.Refresh();
        }
    }

    public static void Release() {
        if(!Initialized) {
            return;
        }

        Save();

        if(Profiles != null) {
            foreach(var profile in Profiles) {
                profile.ObjectManager.Release();
            }
            Profiles.Clear();
        }

        Initialized = false;
    }
}