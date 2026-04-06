using System;
using Overlayer.Core;
using RapidGUI;
using UnityEngine;
using static Overlayer.Olly.OllyRender;
using static Overlayer.Olly.OllyState;

namespace Overlayer.Olly;

public partial class Olly : MonoBehaviour {
    private OllyRender renderer;

    public float TextSpeed = 40f;

    private OllyDialogue.Node currentNode;
    private string displayedText = "";
    private int charIndex;
    private float textTimer;
    private float newlineWait;

    private FaceShape face;
    internal bool FollowMouse;

    public ref bool GetFollowMouseRef() {
        return ref FollowMouse;
    }

    internal void StartDialogue(OllyDialogue.Node root) {
        currentNode = root;
        face.Eye = currentNode.Eye;
        face.Mouth = currentNode.Mouth;
        face.Eyebrow = currentNode.Eyebrow;
        face.EyeSpecial = currentNode.EyeSpecial;
        face.EffectBit = currentNode.EffectBit;
        face.EffectForwardBit = currentNode.EffectForwardBit;

        displayedText = "";
        charIndex = 0;
        textTimer = 0f;
    }

    public void EndDialogue() {
        currentNode = null;
    }

    public bool Inited { get; private set; }

    public bool Init() {
        if (Inited) return false;
        renderer = new OllyRender();
        OllyUtils.InitLanguage();
        StartDialogue(MakeDialogue());
        Inited = true;
        return true;
    }

    public void Release() {
        if (!Inited) return;
        EndDialogue();
        renderer = null;
        Inited = false;
    }

    private void Update() {
        if (currentNode == null || string.IsNullOrEmpty(currentNode.Text)) return;

        if (newlineWait > 0f) {
            newlineWait -= Time.deltaTime;
            if (newlineWait <= 0f)
                if (charIndex < currentNode.Text.Length) {
                    charIndex++;
                    displayedText = currentNode.Text.Substring(0, charIndex);
                }

            return;
        }

        if (charIndex < currentNode.Text.Length) {
            if (currentNode.Text[charIndex] == '\n') {
                newlineWait = 4f / TextSpeed;
                displayedText = currentNode.Text.Substring(0, charIndex + 1);
                charIndex++;
                return;
            }

            textTimer += Time.deltaTime * TextSpeed;
            var advance = (int)textTimer;
            if (advance > 0) {
                charIndex = Mathf.Min(charIndex + advance, currentNode.Text.Length);
                textTimer -= advance;
                displayedText = currentNode.Text.Substring(0, charIndex);
            }
        }
    }

    private Rect windowRect;

    private void Start() {
        var initWidth = 240f;
        var initHeight = 240f;
        windowRect = new Rect(
            (Screen.width - initWidth) / 2f,
            (Screen.height - initHeight) / 2f,
            initWidth,
            initHeight
        );
    }

    private void OnGUI() {
        if (!Inited || !OllyResources.Loaded || currentNode == null || !Main.IsShowGUI) return;

        windowRect = GUI.Window(812, windowRect, DrawWindow, "Olly", RGUIStyle.darkWindow);
    }

    private void DrawWindow(int windowID) {
        GUI.BringWindowToFront(windowID);

        var lines = string.IsNullOrEmpty(displayedText) ? [] : displayedText.Split('\n');

        var lineCount = lines.Length;
        if (lineCount > 0 && string.IsNullOrEmpty(lines[lineCount - 1])) lineCount--;

        float textHeight = 0;
        float maxLineWidth = 0;
        for (var i = 0; i < lineCount; i++) {
            var size = GUI.skin.label.CalcSize(new GUIContent(lines[i]));
            textHeight += size.y;
            if (size.x > maxLineWidth) maxLineWidth = size.x;
        }

        float portraitSize = Mathf.Max(OllyResources.Base.width, OllyResources.Base.height);

        var newWidth = Mathf.Max(portraitSize + 20, maxLineWidth + 20);
        var newHeight = 20 + portraitSize + 10 + textHeight + 10;

        Vector2 center = new(windowRect.x + windowRect.width / 2f, windowRect.y + windowRect.height / 2f);
        windowRect.width = newWidth;
        windowRect.height = newHeight;
        windowRect.x = center.x - newWidth / 2f;
        windowRect.y = center.y - newHeight / 2f;

        renderer.Draw(face, windowRect, FollowMouse);

        var textY = 20 + portraitSize + 10;
        for (var i = 0; i < lineCount; i++) {
            var size = GUI.skin.label.CalcSize(new GUIContent(lines[i]));
            var textX = (windowRect.width - size.x) / 2f;
            GUI.Label(new Rect(textX, textY, size.x, size.y), lines[i]);
            textY += size.y;
        }

        Rect dragRect = new(0, 0, windowRect.width, 20);
        GUI.DragWindow(dragRect);
    }

    public void DrawChoices() {
#if DEBUG
        DrawDebugFace();
#endif
        if (currentNode == null || currentNode.Choices == null || currentNode.Choices.Length == 0 || IsTalking) return;

        var maxWidth = 0f;
        foreach (var choice in currentNode.Choices) {
            var size = GUI.skin.button.CalcSize(new GUIContent(choice));
            if (size.x > maxWidth) maxWidth = size.x;
        }

        maxWidth += 20f;

        GUILayout.BeginHorizontal();
        try {
            for (var i = 0; i < currentNode.Choices.Length; i++)
                if (Drawer.Button(currentNode.Choices[i], GUILayout.Width(maxWidth))) {
                    currentNode.OnChoice?.Invoke(i);
                    if (currentNode.Next.TryGetValue(i, out var next)) {
                        currentNode = next;

                        face.Eye = currentNode.Eye;
                        face.Mouth = currentNode.Mouth;
                        face.Eyebrow = currentNode.Eyebrow;
                        face.EyeSpecial = currentNode.EyeSpecial;
                        face.EffectBit = currentNode.EffectBit;
                        face.EffectForwardBit = currentNode.EffectForwardBit;
                    }
                    else {
                        currentNode = null;
                    }

                    displayedText = "";
                    charIndex = 0;
                    textTimer = 0f;
                }
        }
        finally {
            GUILayout.EndHorizontal();
        }
    }

    public bool IsTalking => currentNode != null && charIndex < currentNode.Text.Length;

#if DEBUG
    public void DrawDebugFace() {
        GUILayout.BeginHorizontal();

        Drawer.DrawEnum(ref face.Eye);
        Drawer.DrawEnum(ref face.Mouth);
        Drawer.DrawEnum(ref face.Eyebrow);
        Drawer.DrawEnum(ref face.EyeSpecial);
        var feb = (uint)face.EffectBit;
        DrawEffectButtons("Effects", ref feb);
        face.EffectBit = (EffectBit)feb;
        var fefb = (uint)face.EffectForwardBit;
        DrawEffectForwardButtons("Forwards", ref fefb);
        face.EffectForwardBit = (EffectForwardBit)fefb;


        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

    private void DrawEffectButtons(string label, ref uint currentBit) {
        GUILayout.BeginVertical(RGUIStyle.darkWindow);
        GUILayout.Label($"<b>{label}</b>");

        foreach (EffectBit bitValue in Enum.GetValues(typeof(EffectBit))) {
            if (bitValue == EffectBit.None)
                continue;

            var bit = (uint)bitValue;
            var isActive = (currentBit & bit) != 0;

            if (Drawer.Button($"{(isActive ? "●" : "○")} {bitValue}")) currentBit ^= bit;
        }

        GUILayout.EndVertical();
    }

    private void DrawEffectForwardButtons(string label, ref uint currentBit) {
        GUILayout.BeginVertical(RGUIStyle.darkWindow);
        GUILayout.Label($"<b>{label}</b>");

        foreach (EffectForwardBit bitValue in Enum.GetValues(typeof(EffectForwardBit))) {
            if (bitValue == EffectForwardBit.None)
                continue;

            var bit = (uint)bitValue;
            var isActive = (currentBit & bit) != 0;

            if (Drawer.Button($"{(isActive ? "■" : "□")} {bitValue}")) currentBit ^= bit;
        }

        GUILayout.EndVertical();
    }
#endif
}