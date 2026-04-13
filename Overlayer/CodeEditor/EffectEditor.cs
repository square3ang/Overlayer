using Overlayer;
using Overlayer.Core;
using Overlayer.Patches;
using RapidGUI;
using UnityEngine;

public abstract class EffectEditor : MonoBehaviour {
    protected NeoDrawer NeoDrawer = new();
    public string CodesBefore;
    public string CodesAfter;
    public string TargetTag;

    protected Rect WindowRect;
    protected Rect PreviewWindowRect;
    protected bool IsSpawn = false;

    protected virtual float WindowWidth => 300f;
    protected abstract string DefaultTargetTag { get; }
    protected virtual void OnInit(string tag) { }
    public void Init(string tag, string codesBefore, string codesAfter) {
        CodesBefore = codesBefore;
        CodesAfter = codesAfter;

        WindowRect.width = WindowWidth;

        BlockUMMClosing.Block = true;

        TargetTag = DefaultTargetTag;

        OnInit(tag);
    }

    protected virtual void OnGUI() {
        var fmt = string.Format(Main.Lang.Get("THIS_EDITOR", "{0} Editor"), GetEditorName());

        if(!IsSpawn && Event.current.type == EventType.Repaint) {
            WindowRect.x = (Screen.width - WindowRect.width) * 0.5f;
            WindowRect.y = (Screen.height - WindowRect.height) * 0.5f;
            IsSpawn = true;
        }

        WindowRect = GUILayout.Window(GetWindowID(), WindowRect, DrawWindowBase, fmt, RGUIStyle.darkWindow);

        PreviewWindowRect.x = WindowRect.x + WindowRect.width + 10;
        PreviewWindowRect.y = WindowRect.y;
        DrawPreviewWindow();
    }

    private void DrawWindowBase(int id) {
        NeoDrawer.FieldResetId();
        GUI.BringWindowToFront(id);
        GUILayout.BeginVertical();
        GUILayout.Space(10);

        GUILayout.BeginHorizontal();
        GUILayout.Label(Main.Lang.Get("TARGET_TAG", "Target Tag"));
        Drawer.DrawTags(ref TargetTag);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        DrawFields();

        NeoDrawer.UpdateFocused();

        if(Drawer.Button(Main.Lang.Get("DONE", "Done"))) {
            OnDone();
        }

        GUILayout.Space(10);
        GUILayout.EndVertical();
        GUI.DragWindow();
    }

    protected virtual void OnDone() {
        NeoDrawer = null;
        BlockUMMClosing.Block = false;
        Destroy(gameObject);
    }

    public void Update() => Tick();
    protected virtual void Tick() { }

    protected abstract void DrawFields();
    protected abstract void DrawPreviewWindow();
    protected abstract string GetEditorName();
    protected abstract int GetWindowID();
}