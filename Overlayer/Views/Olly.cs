using Overlayer.Core;
using RapidGUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEngine;
using UnityModManagerNet;

namespace Overlayer.Views {
    public class Olly : MonoBehaviour {
        public enum Eye { Default, Surprised, Side }
        public enum Mouth { Default, Open, Disgust, Smile, Caret, Clenched, WideOpen, WideStretch }

        public static Texture2D Base;
        public static Dictionary<Eye, Texture2D> Eyes = new();
        public static Dictionary<Mouth, Texture2D> Mouths = new();

        public Action<int> OnChoice;

        public static bool Inited { get; private set; } = false;
        public static void Init(UnityModManager.ModEntry modEntry) {
            if (Inited) {
                return;
            }

            string path = Path.Combine(modEntry.Path, "eg.res");

            if(!File.Exists(path)) {
                throw new FileNotFoundException(path);
            }

            using var zip = ZipFile.OpenRead(path);

            foreach(var entry in zip.Entries) {
                if(entry.FullName.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase)) {
                    using var stream = entry.Open();
                    using var ms = new MemoryStream();
                    stream.CopyTo(ms);
                    var bytes = ms.ToArray();

                    var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                    if(ImageConversion.LoadImage(tex, bytes)) {
                        var fileNameWithoutExt = Path.GetFileNameWithoutExtension(entry.Name);

                        if(string.Equals(fileNameWithoutExt, "Base", System.StringComparison.OrdinalIgnoreCase)) {
                            Base = tex;
                        } else if(fileNameWithoutExt.StartsWith("E_", System.StringComparison.OrdinalIgnoreCase)) {
                            var enumName = fileNameWithoutExt.Substring(2);
                            if(Enum.TryParse(enumName, true, out Eye eye)) {
                                Eyes[eye] = tex;
                            } else {
                                DestroyImmediate(tex);
                            }
                        } else if(fileNameWithoutExt.StartsWith("M_", System.StringComparison.OrdinalIgnoreCase)) {
                            var enumName = fileNameWithoutExt.Substring(2);
                            if(Enum.TryParse(enumName, true, out Mouth mouth)) {
                                Mouths[mouth] = tex;
                            } else {
                                DestroyImmediate(tex);
                            }
                        } else {
                            DestroyImmediate(tex);
                        }
                    } else {
                        DestroyImmediate(tex);
                    }
                }
            }

            InitLanguage();
            Inited = Base != null && Eyes.Count > 0 && Mouths.Count > 0;
        }

        public static void Deinit() {
            if(!Inited) {
                return;
            }
            if(Base != null) {
                Destroy(Base);
                Base = null;
            }

            foreach(var tex in Eyes.Values) {
                if(tex != null) {
                    Destroy(tex);
                }
            }
            Eyes.Clear();

            foreach(var tex in Mouths.Values) {
                if(tex != null) {
                    Destroy(tex);
                }
            }
            Mouths.Clear();

            Inited = false;
        }

        public class DialogueNode {
            public string Text;
            public string[] Choices;
            public Dictionary<int, DialogueNode> Next = new();
            public Action<int> OnChoice;
            public Eye Eye;
            public Mouth Mouth;

            public DialogueNode(string text, string[] choices = null, Action<int> onChoice = null,
                                Eye eye = Eye.Default, Mouth mouth = Mouth.Default) {
                Text = text;
                Choices = choices;
                OnChoice = onChoice;
                Eye = eye;
                Mouth = mouth;
            }
        }

        private static float textSpeed = 20f;

        private DialogueNode currentNode;
        private string displayedText = "";
        private int charIndex = 0;
        private float textTimer = 0f;
        private float newlineWait = 0f;

        private Eye currentEye = Eye.Default;
        private Mouth currentMouth = Mouth.Default;

        public void StartDialogue(DialogueNode root) {
            currentNode = root;
            currentEye = root.Eye;
            currentMouth = root.Mouth;
            displayedText = "";
            charIndex = 0;
            textTimer = 0f;
        }

        public void EndDialogue() {
            currentNode = null;
            OnChoice = null;
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
                    newlineWait = 4f / textSpeed;
                    displayedText = currentNode.Text.Substring(0, charIndex + 1);
                    charIndex++;
                    return;
                }

                textTimer += Time.deltaTime * textSpeed;
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
            if(!Inited || currentNode == null || !Main.IsShowGUI) {
                return;
            }

            windowRect = GUI.Window(812, windowRect, DrawWindow, "Olly", RGUIStyle.darkWindow);
        }

        private void DrawWindow(int windowID) {
            if(!Inited) {
                return;
            }

            GUI.BringWindowToFront(windowID);

            float portraitSize = Mathf.Max(Base.width, Base.height);

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
                if(size.x > maxLineWidth)
                    maxLineWidth = size.x;
            }

            float newWidth = Mathf.Max(portraitSize + 20, maxLineWidth + 20);
            float newHeight = 20 + portraitSize + 10 + textHeight + 10;

            Vector2 center = new Vector2(windowRect.x + windowRect.width / 2f, windowRect.y + windowRect.height / 2f);
            windowRect.width = newWidth;
            windowRect.height = newHeight;
            windowRect.x = center.x - newWidth / 2f;
            windowRect.y = center.y - newHeight / 2f;

            float imageX = (windowRect.width - portraitSize) / 2f;
            GUI.DrawTexture(new Rect(imageX, 20, portraitSize, portraitSize), Base);
            GUI.DrawTexture(new Rect(imageX, 20, portraitSize, portraitSize), Eyes[currentEye]);
            GUI.DrawTexture(new Rect(imageX, 20, portraitSize, portraitSize), Mouths[currentMouth]);

            float textY = 20 + portraitSize + 10;
            for(int i = 0; i < lineCount; i++) {
                Vector2 size = GUI.skin.label.CalcSize(new GUIContent(lines[i]));
                float textX = (windowRect.width - size.x) / 2f;
                GUI.Label(new Rect(textX, textY, size.x, size.y), lines[i]);
                textY += size.y;
            }

            GUI.DragWindow();
        }

        public void DrawChoices() {
            if(!Inited || currentNode == null || currentNode.Choices == null || currentNode.Choices.Length == 0 || IsTalking()) {
                return;
            }

            float maxWidth = 0f;
            foreach(var choice in currentNode.Choices) {
                Vector2 size = GUI.skin.button.CalcSize(new GUIContent(choice));
                if(size.x > maxWidth)
                    maxWidth = size.x;
            }
            maxWidth += 20f;

            GUILayout.BeginHorizontal();
            try {
                for(int i = 0; i < currentNode.Choices.Length; i++) {
                    if(Drawer.Button(currentNode.Choices[i], GUILayout.Width(maxWidth))) {
                        currentNode.OnChoice?.Invoke(i);
                        if(currentNode.Next.TryGetValue(i, out var next)) {
                            currentNode = next;

                            currentEye = currentNode.Eye;
                            currentMouth = currentNode.Mouth;
                        } else {
                            currentNode = null;
                        }
                        OnChoice?.Invoke(i);

                        displayedText = "";
                        charIndex = 0;
                        textTimer = 0f;
                    }
                }
            } finally {
                GUILayout.EndHorizontal();
            }
        }

        public bool IsTalking() => currentNode != null && charIndex < currentNode.Text.Length;

        public static void InitLanguage() {
            isKorean = Main.Lang.CurrentLanguage == "한국어";
            if(!isKorean) {
                textSpeed = textSpeed * 1.8f;
            }
        }
        private static bool isKorean = false;
        public string Tr(string en, string ko) {
            return isKorean ? ko : en;
        }
        
        public void DialogueInit() {
            var node1 = new DialogueNode(
                "...",
                new[] { Tr("About you", "너에 관해"), Tr("Overlayer", "오버레이어") },
                eye: Eye.Default,
                mouth: Mouth.Default
            );

            var node2 = new DialogueNode(
                Tr("I'm Olly.\nThe name comes from words used in Overlayer.", "저는 올리에요.\nOverlayer에 들어간 단어로부터 이름을 지었다고 하네요."),
                new[] { Tr("Back", "뒤로가기") },
                eye: Eye.Default,
                mouth: Mouth.Open
            );

            var node3 = new DialogueNode(
                Tr("Originally it was a mod made by c3nb, but now mostly Kkitut is in charge.\nThey had a hard time rewriting c3nb's code.", "처음엔 c3nb가 만든 모드였지만, 이제는 대부분 Kkitut이 맞고 있어요.\nc3nb의 코드를 갈아치우느라 고생이 많았다고 했죠."),
                new[] { Tr("Back", "뒤로가기") },
                eye: Eye.Default,
                mouth: Mouth.Open
            );

            node1.Next[0] = node2;
            node1.Next[1] = node3;
            node2.Next[0] = node1;
            node3.Next[0] = node1;

            if(Main.Settings.isFirstEg) {
                var first1 = new DialogueNode(
                    Tr("..oh you found me?", "..날 찾았구나?"),
                    new[] { Tr("What is this??", "이게 뭐야??") },
                    eye: Eye.Default,
                    mouth: Mouth.Default
                );

                var first2 = new DialogueNode(
                    Tr("I’m Olly, a hidden little in Overlayer.", "저는 올리에요,\n그리고 숨겨진 요소이죠."),
                    new[] { Tr("Oh, I see...", "..그렇군") },
                    eye: Eye.Default,
                    mouth: Mouth.Open
                );

                var first3 = new DialogueNode(
                    Tr("How did you find me?", "근데 이건 어떻게 찾으셨어요?"),
                    new[] { Tr("Clicked the logo fast", "로고를 빠르게 클릭해보았지"), Tr("I figured it out", "방법을 알아왔어") },
                    eye: Eye.Default,
                    mouth: Mouth.Disgust
                );

                var first3_1 = new DialogueNode(
                    Tr("Amazing...\nWho would think to click that?", "신기하네요..\n누가 그걸 클릭해 볼 생각을 할까요..."),
                    new[] { Tr("Just bored", "그냥 심심해서"), Tr("Was playing around", "장난치다보니 나오던데") },
                    eye: Eye.Side,
                    mouth: Mouth.Caret
                );

                var first3_2 = new DialogueNode(
                    Tr("Tch, it's a bit of a shame if you just found it.", "쳇, 이걸 그냥 찾아와버리면 좀 아쉽잖아요."),
                    new[] { Tr("Well, I found it, so that's enough, right?", "찾았으니 된 게 아닐까?") },
                    eye: Eye.Side,
                    mouth: Mouth.WideStretch
                );

                var first4 = new DialogueNode(
                    Tr("Anyway, nice to meet you.\nCongratulations, you found a secret.", "뭐 어쨌든 반갑게 되었네요.\n축하해요, 당신은 비밀을 하나 찾았어요."),
                    new[] { Tr("..Ok", "..그래") },
                    eye: Eye.Default,
                    mouth: Mouth.Clenched
                );

                first1.Next[0] = first2;
                first2.Next[0] = first3;

                first3.Next[0] = first3_1; 
                first3.Next[1] = first3_2;

                first3_1.Next[0] = first4;
                first3_1.Next[1] = first4;

                first3_2.Next[0] = first4;
                first3_2.Next[1] = first4;

                first4.Next[0] = node1;
                first4.Next[1] = node1;

                Main.Settings.isFirstEg = false;
                
                StartDialogue(first1);
            } else {
                StartDialogue(node1);
            }
                
        }

        public void DrawDebugExpressions() {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Eyes:");
            foreach(Eye eye in Enum.GetValues(typeof(Eye))) {
                if(Drawer.Button(eye.ToString())) {
                    currentEye = eye;
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Mouths:");
            foreach(Mouth mouth in Enum.GetValues(typeof(Mouth))) {
                if(Drawer.Button(mouth.ToString())) {
                    currentMouth = mouth;
                }
            }
            GUILayout.EndHorizontal();
        }
    }
}