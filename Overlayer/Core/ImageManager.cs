using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Overlayer.Core;

public static class ImageManager {
    private static Sprite DefaultSprite;
    private static Dictionary<string, Sprite> Sprites = new();
    public static bool Initialized { get; private set; }
    public static string[] OSImagePaths { get; private set; }

    public static Sprite GetSpriteSafe(string name) => TryGetSprite(name, out Sprite sprite) ? sprite : DefaultSprite;
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

        int index = Array.IndexOf(OSImagePaths, name);
        if(index != -1) {
            Texture2D tex = new Texture2D(2, 2);
            sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            Sprites.Add(name, sprite);
            return true;
        }

        sprite = DefaultSprite;
        return false;
    }

    public static void Initialize() {
        if(!Initialized) {
            Texture2D tex = new(2, 2);
            tex.SetPixels(new Color[4] { Color.clear, Color.clear, Color.clear, Color.clear });
            tex.Apply();
            DefaultSprite = Sprite.Create(tex, new Rect(0, 0, 100, 100), new Vector2(0.5f, 0.5f));

            OSImagePaths = Directory.GetFiles(Application.dataPath, "*.png", SearchOption.AllDirectories);
            Sprites = new Dictionary<string, Sprite>();
            Initialized = true;
        }
    }

    public static void Release() {
        DefaultSprite = null;
        Sprites = null;
        OSImagePaths = null;
        Initialized = false;
    }
}