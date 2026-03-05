using DG.Tweening;
using Overlayer.Core;
using Overlayer.Patches;
using Overlayer.Tags;
using RapidGUI;
using UnityEngine;

namespace Overlayer.Utils;

internal class ColorRangeEditor : MonoBehaviour {
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

    public string targetTag = nameof(ComboStats.Combo);
    public double valueMin = 0;
    public double valueMax = 100;
    public Color colorMin = Color.black;
    public Color colorMax = Color.white;
    public Ease ease = Ease.OutExpo;
    public int maxLength = -1;

    private NeoDrawer neoDrawer;

    public void Initialize(string tag, string codesBefore, string codesAfter) {
        if(tag.Contains("(")) {
            var arr = tag.Split('(')[1].Split(')')[0].Split(',');
            targetTag = arr[0];
            valueMin = double.Parse(arr[1]);
            valueMax = double.Parse(arr[2]);
            ColorUtility.TryParseHtmlString("#" + arr[3], out colorMin);
            ColorUtility.TryParseHtmlString("#" + arr[4], out colorMax);
            ease = EnumHelper<Ease>.Parse(arr[5]);
            if(arr.Length > 6) {
                maxLength = int.Parse(arr[6]);
            }
        }

        testvalue = (float)valueMax;
        windowRect.width = 400;
        isInitaialize = true;
        this.codesBefore = codesBefore;
        this.codesAfter = codesAfter;
        BlockUMMClosing.Block = true;

        neoDrawer = new NeoDrawer();
    }

    public void OnGUI() {
        if(isInitaialize) {
            var fmt = string.Format(Main.Lang.Get("THIS_EDITOR", "{0} Editor"), "ColorRange");

            if(!isSpawn && Event.current.type == EventType.Repaint) {
                windowRect = GUILayout.Window(123, windowRect, DrawWindow, fmt, RGUIStyle.darkWindow);
                windowRect.x = (int)((Screen.width * 0.5f) - (windowRect.width * 0.5f));
                windowRect.y = (int)((Screen.height * 0.5f) - (windowRect.height * 0.5f));

                isSpawn = true;
            }

            windowRect = GUILayout.Window(123, windowRect, DrawWindow, fmt, RGUIStyle.darkWindow);

            //var sz = GUI.skin.label.CalcSize(new GUIContent("<size=40>Test</size>"));
            previewWindowRect.x = windowRect.x + windowRect.width + 10;
            previewWindowRect.y = windowRect.y;
            previewWindowRect.width = 280;
            previewWindowRect.height = 150;
            previewWindowRect = GUI.Window(1123, previewWindowRect, PreviewWindow, "",
                RGUIStyle.darkWindow);
        }
    }

    private void PreviewWindow(int windowID) {
        TagManager.testerValue = testvalue.ToString();
        GUI.BringWindowToFront(windowID);
        var col = Effect.ColorRange("INTERNAL_TESTER_TAG_1234512345_" + targetTag, valueMin, valueMax,
            ColorUtility.ToHtmlStringRGBA(colorMin), ColorUtility.ToHtmlStringRGBA(colorMax), ease,
            maxLength);
        col = col.Replace(".", "F").Replace("(", "F").Replace(")", "F");
        neoDrawer.DrawSingleWithSlider("Value", ref testvalue, (float)valueMin, (float)valueMax, 100, "Pre");
        GUILayout.Label("<size=40><color=#" + col +
                       ">Test</color></size>");
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
        neoDrawer.DrawDouble(Main.Lang.Get("VALUE_MIN", "Min Value"), ref valueMin);
        neoDrawer.DrawDouble(Main.Lang.Get("VALUE_MAX", "Max Value"), ref valueMax);
        neoDrawer.DrawColor(Main.Lang.Get("COLOR_MIN", "Min Color"), ref colorMin, 180f);
        neoDrawer.DrawColor(Main.Lang.Get("COLOR_MAX", "Max Color"), ref colorMax, 180f);

        GUILayout.BeginHorizontal();
        GUILayout.Label(Main.Lang.Get("EASE", "Ease"));
        Drawer.DrawEase(ref ease);

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        neoDrawer.DrawInt32("maxLength", ref maxLength);

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