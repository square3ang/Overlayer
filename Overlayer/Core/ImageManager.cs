using Overlayer.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Overlayer.Core;

public static class ImageManager {
    public static bool Initialized { get; private set; } = false;
    public static Sprite DefaultSprite {
        get {
            if(_defaultSprite is null) {
                CreateDefault();
            }
            return _defaultSprite;
        }

        private set => _defaultSprite = value;
    }
    static Sprite _defaultSprite;
    private static Dictionary<string, Sprite> Sprites;

    static void CreateDefault() {
        Texture2D tex = new(2, 2);
        tex.SetPixels([Color.clear, Color.clear, Color.clear, Color.clear]);
        tex.Apply();
        _defaultSprite = Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f));
    }

    public static Sprite GetSpriteSafe(string name) => string.IsNullOrEmpty(name) ? DefaultSprite : TryGetSprite(name, out Sprite sprite) ? sprite : DefaultSprite;
    public static Sprite GetSprite(string name) => TryGetSprite(name, out Sprite sprite) ? sprite : null;

    public static void SetSprite(string name, Sprite sprite) => Sprites[name] = sprite;

    public static bool TryGetSprite(string name, out Sprite sprite) {
        if(string.IsNullOrEmpty(name)) {
            sprite = DefaultSprite;
            return false;
        }

        name = name.Replace("{ModDir}", Main.Mod.Path);

        if(Sprites.TryGetValue(name, out Sprite cached)) {
            sprite = cached;
            return true;
        }

        if(File.Exists(name)) {
            byte[] bytes = File.ReadAllBytes(name);
            Texture2D tex = new(2, 2);
            if(tex.LoadImage(bytes)) {
                sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                Sprites.Add(name, sprite);
                return true;
            }
        }

        sprite = DefaultSprite;
        return false;
    }

    public static void CleanUp() {
        foreach(var pf in ProfileManager.Profiles.OfType<ImageConfig>()) {
            if(pf.Images == null) {
                continue;
            }

            pf.Images.RemoveAll(path => !TryGetSprite(path, out _));
        }
    }

    public static void Initialize() {
        if(Initialized) {
            return;
        }
        Sprites = [];
        Initialized = true;
    }

    public static void Release() {
        if(!Initialized) {
            return;
        }
        if(Sprites != null) {
            foreach(var sp in Sprites.Values) {
                if(sp != null) {
                    if(sp.texture != null) {
                        UnityEngine.Object.Destroy(sp.texture);
                    }
                    UnityEngine.Object.Destroy(sp);
                }
            }
            Sprites.Clear();
            Sprites = null;
        }

        if(DefaultSprite != null) {
            if(DefaultSprite.texture != null) {
                UnityEngine.Object.Destroy(DefaultSprite.texture);
            }
            UnityEngine.Object.Destroy(DefaultSprite);
            DefaultSprite = null;
        }

        Initialized = false;
    }
}