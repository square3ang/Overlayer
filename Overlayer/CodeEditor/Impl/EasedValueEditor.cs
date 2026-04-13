using DG.Tweening;
using Overlayer.Core;
using Overlayer.Tags;
using Overlayer.Utils;
using RapidGUI;
using UnityEngine;
using Vostok.Sys.Metrics.PerfCounters;

namespace Overlayer.CodeEditor.Impl;

public class EasedValueEditor : EffectEditor {
    private float _TestValue;

    public int Digits = 6;
    public double Speed = 1000;
    public Ease Ease = Ease.OutQuad;

    protected override string DefaultTargetTag => nameof(AccuracyStats.XAccuracy);

    protected override void OnInit(string tag) {
        if(tag.Contains("(")) {
            var arr = tag.Split('(')[1].Split(')')[0].Split(',');

            TargetTag = arr[0];
            Digits = int.Parse(arr[1]);
            Speed = double.Parse(arr[2]);
            Ease = EnumHelper<Ease>.Parse(arr[3]);
        }

        _TestValue = 100;
    }

    protected override void DrawFields() {
        NeoDrawer.DrawInt32(Main.Lang.Get("DIGITS", "Digits"), ref Digits);
        NeoDrawer.DrawDouble(Main.Lang.Get("SPEED", "Speed"), ref Speed);

        GUILayout.BeginHorizontal();
        GUILayout.Label(Main.Lang.Get("EASE", "Ease"));
        Drawer.DrawEase(ref Ease);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

    protected override void DrawPreviewWindow() {
        PreviewWindowRect.width = 320;
        PreviewWindowRect.height = 150;

        PreviewWindowRect = GUI.Window(GetWindowID() + 1000, PreviewWindowRect, id => {
            GUI.BringWindowToFront(id);

            GUILayout.Label(
                "<size=40>" +
                Effect.EasedValue("INTERNAL_TESTER_TAG_1234512345", Digits, Speed, Ease).ToString() +
                "</size>"
            );

            NeoDrawer.DrawSingleWithSlider("Value", ref _TestValue, 0, 100, 100, "testvalue");

            GUI.DragWindow();
        }, "", RGUIStyle.darkWindow);
    }

    protected override string GetEditorName() => nameof(Effect.EasedValue);
    protected override int GetWindowID() => 124;

    protected override void Tick() => TagManager.testerValue = _TestValue.ToString();
}