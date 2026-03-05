using DG.Tweening;
using Overlayer.Core;
using Overlayer.Patches;
using Overlayer.Tags;
using RapidGUI;
using UnityEngine;

namespace Overlayer.Utils;

internal class EasedValueEditor : MonoBehaviour {
    public string codesBefore;
    public string codesAfter;
    public string matchValue;
    private Rect windowRect;
    private Rect previewWindowRect;
    private string[] contentLines;
    private bool isInitaialize = false;
    private bool isAnimating = false;
    private bool isSpawn = false;
    private float testvalue;

    public string targetTag = nameof(AccuracyStats.XAccuracy);
    public int digits = 6;
    public double speed = 1000;

    public Ease ease = Ease.OutQuad;

    private NeoDrawer neoDrawer;
    public void Initialize(string tag, string codesBefore, string codesAfter) {
        if(tag.Contains("(")) {
            var arr = tag.Split('(')[1].Split(')')[0].Split(',');
            targetTag = arr[0];
            digits = int.Parse(arr[1]);
            speed = double.Parse(arr[2]);
            ease = EnumHelper<Ease>.Parse(arr[3]);
        }

        windowRect.width = 300;

        isInitaialize = true;
        this.codesBefore = codesBefore;
        this.codesAfter = codesAfter;
        BlockUMMClosing.Block = true;
        TagManager.testerValue = "100";

        neoDrawer = new NeoDrawer();
    }

    public void Update() => TagManager.testerValue = testvalue.ToString();

    public void OnGUI() {
        if(isInitaialize) {
            var fmt = string.Format(Main.Lang.Get("THIS_EDITOR", "{0} Editor"), "EasedValue");
            if(!isSpawn && Event.current.type == EventType.Repaint) {
                windowRect = GUILayout.Window(124, windowRect, DrawWindow, fmt, RGUIStyle.darkWindow);
                windowRect.x = (int)((Screen.width * 0.5f) - (windowRect.width * 0.5f));
                windowRect.y = (int)((Screen.height * 0.5f) - (windowRect.height * 0.5f));

                isSpawn = true;
            }

            windowRect = GUILayout.Window(124, windowRect, DrawWindow, fmt, RGUIStyle.darkWindow);
            previewWindowRect.x = windowRect.x + windowRect.width + 10;
            previewWindowRect.y = windowRect.y;
            previewWindowRect.width = 320;
            previewWindowRect.height = 150;
            previewWindowRect = GUI.Window(1124, previewWindowRect, PreviewWindow, "",
                RGUIStyle.darkWindow);
        }
    }

    private void PreviewWindow(int windowID) {
        GUI.BringWindowToFront(windowID);
        GUILayout.Label("<size=40>" + Effect.EasedValue("INTERNAL_TESTER_TAG_1234512345", digits, speed, ease).ToString() + "</size>");
        neoDrawer.DrawSingleWithSlider("Value", ref testvalue, 0, 100, 100, "testvalue");
    }

    private void DrawWindow(int windowID) {
        neoDrawer.FieldResetId();

        GUI.BringWindowToFront(windowID);

        GUILayout.BeginVertical();
        GUILayout.Space(10);

        GUILayout.BeginHorizontal();
        GUILayout.Label(Main.Lang.Get("TARGET_TAG", "Target Tag"));
        Drawer.DrawTags(ref targetTag);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        neoDrawer.DrawInt32(Main.Lang.Get("DIGITS", "Digits"), ref digits);
        neoDrawer.DrawDouble(Main.Lang.Get("SPEED", "Speed"), ref speed);
        GUILayout.BeginHorizontal();
        GUILayout.Label(Main.Lang.Get("EASE", "Ease"));
        Drawer.DrawEase(ref ease);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        neoDrawer.UpdateFocused();

        if(Drawer.Button(Main.Lang.Get("DONE", "Done"))) {
            neoDrawer = null;
            BlockUMMClosing.Block = false;
            Destroy(gameObject);
        }

        GUILayout.Space(10);
        GUILayout.EndVertical();
    }
}
