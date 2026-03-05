using Overlayer.Core;
using RapidGUI;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Overlayer.Wiki;

public class Wiki : MonoBehaviour {
    private Rect windowRect;
    public bool IsLoaded { get; private set; } = false;

    private List<WikiData> wikiDatas;
    private Vector2 leftScroll = Vector2.zero;
    private Vector2 rightScroll = Vector2.zero;
    private int selectedIndex = 0;
    private int currentPageIndex = 0;
    private bool WindowToFrontOnce = false;
    private string searchField = "";

    private void Start() {
        WikiIcons.Init();
        WikiLoader loader = new((data) => {
            wikiDatas = data;
            IsLoaded = true;
        });
        _ = loader.LoadAsync(Path.Combine(Main.Mod.Path, "wikidocs"));

        int initWidth = Mathf.Max(480, Screen.width / 2);
        int initHeight = Mathf.Max(320, Screen.height / 2);
        windowRect = new Rect(
            (Screen.width - initWidth) / 2f,
            (Screen.height - initHeight) / 2f,
            initWidth,
            initHeight
        );
    }

    private void OnGUI() {
        if(!IsLoaded) {
            return;
        }
        windowRect = GUI.Window(823, windowRect, DrawWindow, "Overlayer Wiki", RGUIStyle.darkWindow);
    }

    public void BringToFrontOnce() => WindowToFrontOnce = true;

    private void DrawWindow(int windowID) {
        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical(GUILayout.Width(180));
        searchField = GUILayout.TextField(searchField, 32, Drawer.myTextField, GUILayout.Height(22), GUILayout.Width(180));
        if(string.IsNullOrEmpty(searchField)) {
            Rect searchFieldRect = GUILayoutUtility.GetLastRect();
            searchFieldRect.x += 4;
            searchFieldRect.width -= 4;
            GUI.Label(searchFieldRect, "<i><color=#AAAAAA>Search...</color></i>");
            Rect labelRect = new(searchFieldRect.xMax - 22, searchFieldRect.y, 22, 22);
            GUI.DrawTexture(labelRect, WikiIcons.Search);
        }
        GUILayout.Space(8);
        leftScroll = GUILayout.BeginScrollView(leftScroll);
        for(int i = 0; i < wikiDatas.Count; i++) {
            bool highlight = i == selectedIndex;
            if(highlight) {
                GUI.color = Color.cyan;
            }
            if(Drawer.Button(wikiDatas[i].Title)) {
                currentPageIndex = 0;
                selectedIndex = i;
            }
            if(highlight) {
                GUI.color = Color.white;
            }
        }
        GUILayout.EndScrollView();
        GUILayout.EndVertical();

        GUILayout.BeginVertical();
        rightScroll = GUILayout.BeginScrollView(rightScroll);
        if(selectedIndex >= 0 && selectedIndex < wikiDatas.Count) {
            var data = wikiDatas[selectedIndex];
            GUILayout.Label($"<size=28><b>{data.Sections[currentPageIndex].Title}</b></size>", new GUIStyle(GUI.skin.label) { richText = true });
            Rect rect = GUILayoutUtility.GetLastRect();
            GUI.DrawTexture(new Rect(rect.x, rect.yMax + 6, rect.width, 2), Drawer.dulgray);
            GUILayout.Space(8);
            GUILayout.Label(data.Sections[currentPageIndex].Content);
        }
        GUILayout.EndScrollView();
        GUILayout.BeginHorizontal();
        if(selectedIndex >= 0 && selectedIndex < wikiDatas.Count && currentPageIndex > 0) {
            if(Drawer.Button("◀", GUILayout.Width(90))) {
                currentPageIndex--;
            }
        } else {
            GUILayout.Space(100);
        }
        GUILayout.FlexibleSpace();
        GUILayout.Label($"{currentPageIndex + 1} / {wikiDatas[selectedIndex].Sections.Count}");
        GUILayout.FlexibleSpace();
        if(selectedIndex >= 0 && selectedIndex < wikiDatas.Count && currentPageIndex < wikiDatas[selectedIndex].Sections.Count - 1) {
            if(Drawer.Button("▶", GUILayout.Width(90))) {
                currentPageIndex++;
            }
        } else {
            GUILayout.Space(95);
        }
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();

        if(WindowToFrontOnce) {
            WindowToFrontOnce = false;
            GUI.BringWindowToFront(windowID);
        }
        GUI.DragWindow();
    }
}
