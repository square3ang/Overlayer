using Overlayer.Models;
using Overlayer.Unity;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Overlayer.Core;

public class TextManager {
    public int Count => Texts.Count;

    public List<OverlayerText> Texts = new();
    public OverlayerProfile ProfileCanvas;

    public TextManager(OverlayerProfile profileCanvas) => ProfileCanvas = profileCanvas;

    public OverlayerText Create(TextConfig config) {
        if(string.IsNullOrEmpty(config.Name)) {
            config.Name = $"Text {Count + 1}";
        }

        var go = new GameObject($"OverlayerText_{Count + 1}");
        var text = go.AddComponent<OverlayerText>();
        text.Init(ProfileCanvas, config);
        Texts.Add(text);
        return text;
    }

    public OverlayerText Get(int index) => (index >= 0 && index < Count) ? Texts[index] : null;

    public bool OrderToIndex(int from, int to) {
        if(from < 0 || from >= Count || to < 0 || to >= Count || from == to) {
            return false;
        }

        var item = Texts[from];
        Texts.RemoveAt(from);
        Texts.Insert(to, item);

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

        var item = Texts[fromIndex];
        Texts.RemoveAt(fromIndex);

        if(fromIndex < toIndex) {
            toIndex--;
        }

        Texts.Insert(toIndex, item);
        item.gameObject.transform.SetSiblingIndex(toIndex);

        return true;
    }

    public void Import(List<TextConfig> configs) {
        if(configs == null) {
            return;
        }
        foreach(var config in configs) {
            Create(config);
        }
        Refresh();
    }

    public List<TextConfig> Export() => Texts.Select(t => t.Config).ToList();

    public void Destroy(OverlayerText text) {
        Object.Destroy(text.gameObject);
        Texts.Remove(text);
        Refresh();

        try {
            Texts?.Remove(text);
        } catch { }
    }

    public void Refresh() => Texts.ForEach(t => t.ApplyConfig());

    public void Release() {
        foreach(var t in Texts) {
            if(t) {
                Object.Destroy(t.gameObject);
            }
        }
        Texts.Clear();
    }
}