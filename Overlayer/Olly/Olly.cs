using Overlayer.Core;
using RapidGUI;
using System;
using UnityEngine;
using static Overlayer.Olly.OllyRender;

namespace Overlayer.Olly;

public partial class Olly : MonoBehaviour {
    private OllyRender renderer;

    public float TextSpeed = 40f;

    private OllyDialogue.Node currentNode;
    private string displayedText = "";
    private int charIndex = 0;
    private float textTimer = 0f;
    private float newlineWait = 0f;

    private FaceShape face = new();
    internal bool FollowMouse = false;
    public ref bool GetFollowMouseRef() => ref FollowMouse;

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

    public void EndDialogue() => currentNode = null;

    public bool Inited { get; private set; }
    public bool Init() {
        if(Inited) {
            return false;
        }
        renderer = new OllyRender();
        OllyUtils.InitLanguage();
        StartDialogue(MakeDialogue());
        Inited = true;
        return true;
    }

    public void Release() {
        if(!Inited) {
            return;
        }
        EndDialogue();
        renderer = null;
        Inited = false;
    }

    private void Update() {
        if(currentNode == null || string.IsNullOrEmpty(currentNode.Text)) {
            return;
        }

        if(newlineWait > 0f) {
            newlineWait -= Time.deltaTime;
            if(newlineWait <= 0f) {
                if(charIndex < currentNode.Text.Length) {
                    charIndex++;
                    displayedText = currentNode.Text.Substring(0, charIndex);
                }
            }
            return;
        }

        if(charIndex < currentNode.Text.Length) {
            if(currentNode.Text[charIndex] == '\n') {
                newlineWait = 4f / TextSpeed;
                displayedText = currentNode.Text.Substring(0, charIndex + 1);
                charIndex++;
                return;
            }

            textTimer += Time.deltaTime * TextSpeed;
            int advance = (int)textTimer;
            if(advance > 0) {
                charIndex = Mathf.Min(charIndex + advance, currentNode.Text.Length);
                textTimer -= advance;
                displayedText = currentNode.Text.Substring(0, charIndex);
            }
        }
    }

    private Rect windowRect;
    private void Start() {
        float initWidth = 240f;
        float initHeight = 240f;
        windowRect = new Rect(
            (Screen.width - initWidth) / 2f,
            (Screen.height - initHeight) / 2f,
            initWidth,
            initHeight
        );
    }
    private void OnGUI() {
        if(!Inited || !OllyResources.Loaded || currentNode == null || !Main.IsShowGUI) {
            return;
        }

        windowRect = GUI.Window(812, windowRect, DrawWindow, "Olly", RGUIStyle.darkWindow);
    }

    private void DrawWindow(int windowID) {
        GUI.BringWindowToFront(windowID);

        string[] lines = string.IsNullOrEmpty(displayedText) ? Array.Empty<string>() : displayedText.Split('\n');

        int lineCount = lines.Length;
        if(lineCount > 0 && string.IsNullOrEmpty(lines[lineCount - 1])) {
            lineCount--;
        }

        float textHeight = 0;
        float maxLineWidth = 0;
        for(int i = 0; i < lineCount; i++) {
            Vector2 size = GUI.skin.label.CalcSize(new GUIContent(lines[i]));
            textHeight += size.y;
            if(size.x > maxLineWidth) {
                maxLineWidth = size.x;
            }
        }

        float portraitSize = Mathf.Max(OllyResources.Base.width, OllyResources.Base.height);

        float newWidth = Mathf.Max(portraitSize + 20, maxLineWidth + 20);
        float newHeight = 20 + portraitSize + 10 + textHeight + 10;

        Vector2 center = new(windowRect.x + (windowRect.width / 2f), windowRect.y + (windowRect.height / 2f));
        windowRect.width = newWidth;
        windowRect.height = newHeight;
        windowRect.x = center.x - (newWidth / 2f);
        windowRect.y = center.y - (newHeight / 2f);

        renderer.Draw(face, windowRect, FollowMouse);

        float textY = 20 + portraitSize + 10;
        for(int i = 0; i < lineCount; i++) {
            Vector2 size = GUI.skin.label.CalcSize(new GUIContent(lines[i]));
            float textX = (windowRect.width - size.x) / 2f;
            GUI.Label(new Rect(textX, textY, size.x, size.y), lines[i]);
            textY += size.y;
        }

        Rect dragRect = new(0, 0, windowRect.width, 20);
        GUI.DragWindow(dragRect);
    }

    public void DrawChoices() {
        if(currentNode == null || currentNode.Choices == null || currentNode.Choices.Length == 0 || IsTalking) {
            return;
        }

        float maxWidth = 0f;
        foreach(var choice in currentNode.Choices) {
            Vector2 size = GUI.skin.button.CalcSize(new GUIContent(choice));
            if(size.x > maxWidth) {
                maxWidth = size.x;
            }
        }
        maxWidth += 20f;

        GUILayout.BeginHorizontal();
        try {
            for(int i = 0; i < currentNode.Choices.Length; i++) {
                if(Drawer.Button(currentNode.Choices[i], GUILayout.Width(maxWidth))) {
                    currentNode.OnChoice?.Invoke(i);
                    if(currentNode.Next.TryGetValue(i, out var next)) {
                        currentNode = next;

                        face.Eye = currentNode.Eye;
                        face.Mouth = currentNode.Mouth;
                        face.Eyebrow = currentNode.Eyebrow;
                        face.EyeSpecial = currentNode.EyeSpecial;
                        face.EffectBit = currentNode.EffectBit;
                        face.EffectForwardBit = currentNode.EffectForwardBit;
                    } else {
                        currentNode = null;
                    }

                    displayedText = "";
                    charIndex = 0;
                    textTimer = 0f;
                }
            }
        } finally {
            GUILayout.EndHorizontal();
        }
    }

    public bool IsTalking => currentNode != null && charIndex < currentNode.Text.Length;
}