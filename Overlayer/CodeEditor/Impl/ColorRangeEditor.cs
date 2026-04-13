using DG.Tweening;
using Overlayer.Core;
using Overlayer.Tags;
using Overlayer.Utils;
using RapidGUI;
using UnityEngine;

namespace Overlayer.CodeEditor.Impl;

public class ColorRangeEditor : EffectEditor {
    private float _TestValue;

    public double ValueMin = 0;
    public double ValueMax = 100;
    public Color ColorMin = Color.black;
    public Color ColorMax = Color.white;
    public Ease Ease = Ease.OutExpo;
    public int MaxLength = -1;

    protected override string DefaultTargetTag => nameof(ComboStats.Combo);

    protected override float WindowWidth => 400f;
    protected override void OnInit(string tag) {
        if(tag.Contains("(")) {
            var arr = tag.Split('(')[1].Split(')')[0].Split(',');

            TargetTag = arr[0];
            ValueMin = double.Parse(arr[1]);
            ValueMax = double.Parse(arr[2]);
            ColorUtility.TryParseHtmlString("#" + arr[3], out ColorMin);
            ColorUtility.TryParseHtmlString("#" + arr[4], out ColorMax);
            Ease = EnumHelper<Ease>.Parse(arr[5]);

            if(arr.Length > 6) {
                MaxLength = int.Parse(arr[6]);
            }
        }

        _TestValue = (float)ValueMax;
    }

    protected override void DrawFields() {
        NeoDrawer.DrawDouble(Main.Lang.Get("VALUE_MIN", "Min Value"), ref ValueMin);
        NeoDrawer.DrawDouble(Main.Lang.Get("VALUE_MAX", "Max Value"), ref ValueMax);
        NeoDrawer.DrawColor(Main.Lang.Get("COLOR_MIN", "Min Color"), ref ColorMin, 180f);
        NeoDrawer.DrawColor(Main.Lang.Get("COLOR_MAX", "Max Color"), ref ColorMax, 180f);

        GUILayout.BeginHorizontal();
        GUILayout.Label(Main.Lang.Get("EASE", "Ease"));
        Drawer.DrawEase(ref Ease);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        NeoDrawer.DrawInt32("maxLength", ref MaxLength);
    }

    protected override void DrawPreviewWindow() {
        PreviewWindowRect.width = 280;
        PreviewWindowRect.height = 150;

        PreviewWindowRect = GUI.Window(GetWindowID() + 1000, PreviewWindowRect, id => {
            GUI.BringWindowToFront(id);

            var col = Effect.ColorRange(
                "INTERNAL_TESTER_TAG_1234512345_" + TargetTag,
                ValueMin,
                ValueMax,
                ColorUtility.ToHtmlStringRGBA(ColorMin),
                ColorUtility.ToHtmlStringRGBA(ColorMax),
                Ease,
                MaxLength
            );

            col = col.Replace(".", "F").Replace("(", "F").Replace(")", "F");

            NeoDrawer.DrawSingleWithSlider(
                "Value",
                ref _TestValue,
                (float)ValueMin,
                (float)ValueMax,
                100,
                "Pre"
            );

            GUILayout.Label("<size=40><color=#" + col + ">Test</color></size>");

            GUI.DragWindow();
        }, "", RGUIStyle.darkWindow);
    }

    protected override string GetEditorName() => nameof(Effect.ColorRange);

    protected override int GetWindowID() => 123;

    protected override void Tick() => TagManager.testerValue = _TestValue.ToString();
}