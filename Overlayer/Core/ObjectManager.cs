using Overlayer.Models;
using Overlayer.Unity;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Overlayer.Core;

public class ObjectManager {
    public int Count => Objects.Count;

    public List<OverlayerObject> Objects = new();
    public OverlayerProfile ProfileCanvas;

    public ObjectManager(OverlayerProfile profileCanvas) {
        ProfileCanvas = profileCanvas;
    }

    public OverlayerText Create(TextConfig config) {
        if(string.IsNullOrEmpty(config.Name)) {
            config.Name = $"Text {Count + 1}";
        }

        var go = new GameObject($"OverlayerText_{Count + 1}");
        var obj = go.AddComponent<OverlayerText>();
        obj.Init(ProfileCanvas, config);

        Objects.Add(obj);
        return obj;
    }

    public OverlayerImage Create(ImageConfig config) {
        if(string.IsNullOrEmpty(config.Name)) {
            config.Name = $"Image {Count + 1}";
        }

        var go = new GameObject($"OverlayerImage_{Count + 1}");
        var obj = go.AddComponent<OverlayerImage>();
        obj.Init(ProfileCanvas, config);

        Objects.Add(obj);
        return obj;
    }

    public OverlayerObject Get(int index) {
        return (index >= 0 && index < Count) ? Objects[index] : null;
    }

    public bool OrderToIndex(int from, int to) {
        if(from < 0 || from >= Count || to < 0 || to >= Count || from == to) {
            return false;
        }

        var item = Objects[from];
        Objects.RemoveAt(from);
        Objects.Insert(to, item);

        item.gameObject.transform.SetSiblingIndex(to);

        return true;
    }
    public bool OrderUp(int index) => OrderToIndex(index, index - 1);
    public bool OrderDown(int index) => OrderToIndex(index, index + 1);
    public bool OrderToTop(int index) => OrderToIndex(index, 0);
    public bool OrderToBottom(int index) => OrderToIndex(index, Count - 1);
    public bool OrderByDrag(int fromIndex, int toIndex) {
        if(fromIndex < 0 || fromIndex >= Count) {
            return false;
        }

        toIndex = Mathf.Clamp(toIndex, 0, Count);

        if(fromIndex == toIndex || fromIndex == toIndex - 1) {
            return false;
        }

        var item = Objects[fromIndex];
        Objects.RemoveAt(fromIndex);

        if(fromIndex < toIndex) {
            toIndex--;
        }

        Objects.Insert(toIndex, item);
        item.gameObject.transform.SetSiblingIndex(toIndex);

        return true;
    }

    public void Import(List<ObjectConfig> configs) {
        if(configs == null) {
            return;
        }

        foreach(var config in configs) {
            if(config is TextConfig t) {
                Create(t);
            } else if(config is ImageConfig img) {
                Create(img);
            }
        }

        Refresh();
    }

    public List<ObjectConfig> Export() => Objects.Select(o => o.Config).ToList();

    public void Destroy(OverlayerObject obj) {
        Object.Destroy(obj.gameObject);
        Objects.Remove(obj);
        Refresh();
    }

    public void Refresh() {
        Objects.ForEach(o => o.ApplyConfig());
    }

    public void Release() {
        foreach(var o in Objects) {
            if(o) {
                Object.Destroy(o.gameObject);
            }
        }

        Objects.Clear();
    }
}