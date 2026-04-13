using DG.Tweening;
using Overlayer.Core;
using Overlayer.Tags;
using Overlayer.Utils;
using RapidGUI;
using UnityEngine;
using Time = UnityEngine.Time;

namespace Overlayer.CodeEditor.Impl;

public class MovingManEditor : EffectEditor {
    private int _TestValue;
    private float _Timer;

    public double StartSize = 30;
    public double EndSize = 80;
    public double DefaultSize = 30;
    public double Speed = 800;
    public bool Invert = false;
    public Ease Ease = Ease.OutExpo;

    protected override string DefaultTargetTag => TargetTag = nameof(ComboStats.Combo);

    protected override void OnInit(string tag) {
        if(tag.Contains("(")) {
            var arr = tag.Split('(')[1].Split(')')[0].Split(',');
            TargetTag = arr[0];
            StartSize = double.Parse(arr[1]);
            EndSize = double.Parse(arr[2]);
            DefaultSize = double.Parse(arr[3]);
            Speed = double.Parse(arr[4]);
            Invert = bool.Parse(arr[5]);
            Ease = EnumHelper<Ease>.Parse(arr[6]);
        }

        _TestValue = 0;
        _Timer = 0;
    }

    protected override void DrawFields() {
        NeoDrawer.DrawDouble(Main.Lang.Get("START_SIZE", "Start Size"), ref StartSize);
        NeoDrawer.DrawDouble(Main.Lang.Get("END_SIZE", "End Size"), ref EndSize);
        NeoDrawer.DrawDouble(Main.Lang.Get("DEFAULT_SIZE", "Default Size"), ref DefaultSize);
        NeoDrawer.DrawDouble(Main.Lang.Get("SPEED", "Speed"), ref Speed);

        Drawer.DrawBool(Main.Lang.Get("INVERT", "Invert"), ref Invert);

        GUILayout.BeginHorizontal();
        GUILayout.Label(Main.Lang.Get("EASE", "Ease"));
        Drawer.DrawEase(ref Ease);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

    protected override void DrawPreviewWindow() {
        var size = Effect.MovingMan(
            "INTERNAL_TESTER_TAG_1234512345",
            StartSize,
            EndSize,
            DefaultSize,
            Speed,
            Invert,
            Ease
        ) / 2f;

        PreviewWindowRect.width = 200;
        PreviewWindowRect.height = 120;

        PreviewWindowRect = GUI.Window(GetWindowID() + 1000, PreviewWindowRect, id => {
            GUI.BringWindowToFront(id);
            GUILayout.Label("<size=" + size + ">Test</size>");
            GUI.DragWindow();
        }, "", RGUIStyle.darkWindow);
    }

    protected override string GetEditorName() => nameof(Effect.MovingMan);
    protected override int GetWindowID() => 122;

    protected override void Tick() {
        _Timer += Time.deltaTime;

        if(_Timer >= Speed / 1000f) {
            _Timer -= (float)Speed / 1000f;

            _TestValue++;
            if(_TestValue > 100) {
                _TestValue = 0;
            }

            TagManager.testerValue = _TestValue.ToString();
        }
    }
}