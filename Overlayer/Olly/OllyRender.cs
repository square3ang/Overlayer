using System;
using UnityEngine;
using static Overlayer.Olly.OllyResources;
using static Overlayer.Olly.OllyState;

namespace Overlayer.Olly;

public class OllyRender {
    public static class Anchor {
        public static readonly Vector2 BGAnchor = new(24, 62);
        public static readonly Vector2 HairAnchor = new(11, 6);
        public static readonly Vector2 HairBGAnchor = new(7, 3);
        public static readonly Vector2 EyelidUpAnchor = new(53, 96);
        public static readonly Vector2 EyelidDownAnchor = new(63, 126);
        public static readonly Vector2 EyelidBGAnchor = new(53, 92);
        public static readonly Vector2 NoseAnchor = new(121, 134);

        public static readonly Vector2[] EyebrowAnchor = new Vector2[] {
            new(72, 76), // Normal
            new(75, 87), // Sad
            new(69, 74), // Angry
            new(78, 52), // AngryMore
            new(73, 68), // Curious
            new(62, 75), // Twist
            new(63, 66), // Pity
            new(65, 65)  // NormalHigh
        };

        public static readonly (Vector2, Vector2)[] EyesAnchor = new (Vector2, Vector2)[] {
            (new Vector2(81, 101), new Vector2(143, 101)), // Normal
            (new Vector2(82, 109), new Vector2(144, 108))  // Small
        };

        public static readonly Vector2[] EyeSpecialAnchor = new Vector2[] {
            new(59, 115), // Up
            new(55, 112)  // Down
        };

        public static readonly (Vector2, Vector2) EyeHighlightAnchor =
            (new Vector2(82, 108), new Vector2(146, 107));

        public static readonly Vector2[] MouthAnchor = new Vector2[] {
            new(119, 155), // Normal
            new(115, 152), // Shift
            new(117, 153), // CaretWide
            new(108, 137), // HalfChewed
            new(117, 154), // Smile
            new(115, 152), // OpenSmall
            new(115, 152), // OpenSmallHarf
            new(116, 154), // OpenMicro
            new(113, 150), // OpenCaret
            new(111, 150), // Open
            new(113, 150), // Disgust
            new(107, 149), // OpenDisgust
            new(109, 151), // Joker
            new(121, 153), // SadSmall
            new(122, 153), // Caret
            new(113, 150), // Clenched
            new(105, 148), // Mad
            new(102, 143), // Surprise
            new(107, 150), // SurpriseSmall
            new(110, 149)  // WideStretch
        };

        public static readonly Vector2[] EffectAnchor = new Vector2[] {
            new(61, 125), // Tear
            new(79, 111), // Sweat
            new(63, 129)  // Blush
        };

        public static readonly Vector2[] EffectForwardAnchor = new Vector2[] {
            new(3, 186),  // Cloud
            new(16, 8),   // Tremble
            new(65, 39),  // Tendon
            new(50, 96)   // BlushBig
        };
    }
    public struct FaceShape {
        public Eyebrow Eyebrow;
        public Eye Eye;
        public EyeSpecial EyeSpecial;
        public Mouth Mouth;
        public EffectBit EffectBit;
        public EffectForwardBit EffectForwardBit;
    }
    public bool touching;
    public float eyeBlinkTimer;
    public float eyeBlinkInterval;
    public Vector2 eyeOffset;
    public Vector2 eyeVelocity;
    public Vector2 hairOffset;
    public Vector2 hairVelocity;
    public void Draw(FaceShape face, Rect windowRect, bool followMouse) {
        float portraitSize = Mathf.Max(Base.width, Base.height);
        Vector2 mousePos = Event.current.mousePosition;

        eyeBlinkTimer += Time.deltaTime;
        bool canBlink = true;
        if(followMouse) {
            Rect touchArea = new(33, 38, 208, 108);
            if(Event.current.button == 0 && Event.current.type == EventType.MouseDown && touchArea.Contains(mousePos)) {
                canBlink = false;
                touching = true;
                eyeBlinkInterval = 0;
                eyeBlinkTimer = 0;
                face.Mouth = Mouth.OpenMicro;
            } else if(touching) {
                if(touchArea.Contains(mousePos) && Input.GetMouseButton(0)) {
                    canBlink = false;
                    touching = true;
                    if(eyeBlinkTimer > eyeBlinkInterval + 0.3f) {
                        face.EyeSpecial = EyeSpecial.Down;
                        face.Eye = Eye.None;
                    }
                    face.Mouth = Mouth.OpenMicro;
                } else if(!Input.GetMouseButton(0) || !touchArea.Contains(mousePos)) {
                    canBlink = false;
                    touching = false;
                    eyeBlinkInterval = UnityEngine.Random.Range(7.8f, 17.2f);
                    eyeBlinkTimer = 0f;
                }
            }
        }

        if(canBlink) {
            if(eyeBlinkTimer > eyeBlinkInterval + 0.3f) {
                face.EyeSpecial = EyeSpecial.Down;
                face.Eye = Eye.None;
                if(eyeBlinkTimer > eyeBlinkInterval + 0.4f) {
                    eyeBlinkInterval = UnityEngine.Random.Range(7.8f, 17.2f);
                    eyeBlinkTimer = 0f;
                    face.EyeSpecial = EyeSpecial.None;
                }
            }
        }

        if(followMouse) {
            Vector2 left = new(
                Anchor.EyesAnchor[(int)Eye.Normal - 1].Item1.x + (Eyes[(int)Eye.Normal - 1].left.width / 2f),
                Anchor.EyesAnchor[(int)Eye.Normal - 1].Item1.y + (Eyes[(int)Eye.Normal - 1].left.height / 2f)
            );

            Vector2 right = new(
                Anchor.EyesAnchor[(int)Eye.Normal - 1].Item2.x + (Eyes[(int)Eye.Normal - 1].right.width / 2f),
                Anchor.EyesAnchor[(int)Eye.Normal - 1].Item2.y + (Eyes[(int)Eye.Normal - 1].right.height / 2f)
            );

            Vector2 pivot = (left + right) * 0.5f;

            Vector2 delta = new(mousePos.x - pivot.x, mousePos.y - pivot.y);
            float distance = delta.magnitude;
            float maxDistance = 100f;
            float maxOffset = 3.6f;

            float t = Mathf.Clamp01(distance / maxDistance);
            float eased = Mathf.Sin(t * Mathf.PI * 0.5f);

            Vector2 targetOffset = delta.normalized * (eased * maxOffset);
            targetOffset.x = targetOffset.x < 0 ? targetOffset.x * (touching ? 1.2f : 2.6f) : targetOffset.x * 0.8f;
            targetOffset.y = targetOffset.y < 0 ? targetOffset.y * 0.8f : targetOffset.y * 2.0f;
            if(touching && eyeBlinkTimer > eyeBlinkInterval + 0.3f) {
                targetOffset *= 3f;
            }
            eyeOffset = Vector2.SmoothDamp(eyeOffset, targetOffset, ref eyeVelocity, 0.2f);
        } else if(eyeOffset != Vector2.zero) {
            eyeOffset = Vector2.SmoothDamp(eyeOffset, Vector2.zero, ref eyeVelocity, 0.2f);
        }

        float imageX = (windowRect.width - portraitSize) / 2f;
        GUI.DrawTexture(new Rect(imageX + Anchor.BGAnchor.x,
            20 + Anchor.BGAnchor.y, BG.width, BG.height), BG);
        if(followMouse) {
            float xPivot = Anchor.HairAnchor.x + (Hair.width / 2);
            float yPivot = Anchor.HairAnchor.y + (Hair.height / 2);

            Vector2 delta = new(mousePos.x - xPivot, mousePos.y - yPivot);
            float distance = delta.magnitude;
            float maxDistance = 100f;
            float maxOffset = 1.7f;

            float t = Mathf.Clamp01(distance / maxDistance);
            float eased = Mathf.Sin(t * Mathf.PI * 0.5f);

            Vector2 targetOffset = delta.normalized * (eased * maxOffset);
            hairOffset = Vector2.SmoothDamp(hairOffset, targetOffset, ref hairVelocity, 0.2f);
        } else if(hairOffset != Vector2.zero) {
            hairOffset = Vector2.SmoothDamp(hairOffset, Vector2.zero, ref hairVelocity, 0.2f);
        }

        GUI.DrawTexture(new Rect(
            imageX + Anchor.HairBGAnchor.x + hairOffset.x,
            20 + Anchor.HairBGAnchor.y + hairOffset.y,
            HairBG.width, HairBG.height), HairBG);

        GUI.DrawTexture(new Rect(imageX, 20, Base.width, Base.height), Base);
        Vector2 faceOffset = Vector2.zero;
        if(followMouse) {
            faceOffset = eyeOffset * 0.5f;
        }
        if(face.Mouth != Mouth.None) {
            GUI.DrawTexture(new Rect(imageX + Anchor.MouthAnchor[(int)face.Mouth - 1].x + faceOffset.x,
                20 + Anchor.MouthAnchor[(int)face.Mouth - 1].y + faceOffset.y,
                Mouths[(int)face.Mouth - 1].width, Mouths[(int)face.Mouth - 1].height), Mouths[(int)face.Mouth - 1]);
        }
        GUI.DrawTexture(new Rect(imageX + Anchor.NoseAnchor.x + faceOffset.x,
            20 + Anchor.NoseAnchor.y + faceOffset.y,
            Nose.width, Nose.height), Nose);
        if(face.Eye != Eye.None && face.EyeSpecial == EyeSpecial.None) {
            GUI.DrawTexture(new Rect(
                imageX + Anchor.EyesAnchor[(int)face.Eye - 1].Item2.x + eyeOffset.x,
                20 + Anchor.EyesAnchor[(int)face.Eye - 1].Item2.y + eyeOffset.y,
                Eyes[(int)face.Eye - 1].right.width, Eyes[(int)face.Eye - 1].right.height), Eyes[(int)face.Eye - 1].right
            );
            GUI.DrawTexture(new Rect(
                imageX + Anchor.EyesAnchor[(int)face.Eye - 1].Item1.x + eyeOffset.x,
                20 + Anchor.EyesAnchor[(int)face.Eye - 1].Item1.y + eyeOffset.y,
                Eyes[(int)face.Eye - 1].left.width, Eyes[(int)face.Eye - 1].left.height), Eyes[(int)face.Eye - 1].left);
        }
        if(face.EyeSpecial == EyeSpecial.None) {
            Vector2 eyelidOffset = Vector2.zero;
            if(followMouse) {
                eyelidOffset = eyeOffset * 0.4f;
            }
            if(eyeBlinkTimer > eyeBlinkInterval) {
                eyelidOffset.y += (eyeBlinkInterval - eyeBlinkTimer) * 6f;
            }
            GUI.DrawTexture(new Rect(imageX + Anchor.EyelidDownAnchor.x + eyelidOffset.x,
                20 + Anchor.EyelidDownAnchor.y + eyelidOffset.y,
                EyelidDown.width, EyelidDown.height), EyelidDown);
        }
        if(face.EffectBit != EffectBit.None) {
            foreach(EffectBit effect in Enum.GetValues(typeof(EffectBit))) {
                if((face.EffectBit & effect) == 0) {
                    continue;
                }

                float effectOffsetY = 0f;
                if(effect == EffectBit.Tear && eyeBlinkTimer > eyeBlinkInterval) {
                    effectOffsetY += (eyeBlinkInterval - eyeBlinkTimer) * 6f;
                }

                int idx = OllyUtils.BitIndex((int)effect) - 1;
                var anchor = Anchor.EffectAnchor[idx];
                var tex = Effects[idx];

                GUI.DrawTexture(
                    new Rect(imageX + anchor.x, 20 + anchor.y,
                    tex.width, tex.height), tex
                );
            }
        }
        if(face.EyeSpecial == EyeSpecial.None) {
            Vector2 leftEyeOffset = Vector2.zero;
            Vector2 rightEyeOffset = Vector2.zero;
            Vector2 eyelidOffset = Vector2.zero;
            if(face.Eye == Eye.Small) {
                leftEyeOffset = new Vector2(4f, 6f);
                rightEyeOffset = new Vector2(0, 5f);
            }
            leftEyeOffset += eyeOffset;
            rightEyeOffset += eyeOffset;
            if(followMouse) {
                eyelidOffset = eyeOffset * 0.4f;
            }
            if(eyeBlinkTimer > eyeBlinkInterval) {
                eyelidOffset.y += (eyeBlinkTimer - eyeBlinkInterval) * 22f;
            }
            GUI.DrawTexture(new Rect(imageX + Anchor.EyelidBGAnchor.x + eyelidOffset.x,
                20 + Anchor.EyelidBGAnchor.y + eyelidOffset.y,
                EyelidBG.width, EyelidBG.height), EyelidBG);
            GUI.DrawTexture(new Rect(imageX + Anchor.EyelidUpAnchor.x + eyelidOffset.x,
                20 + Anchor.EyelidUpAnchor.y + eyelidOffset.y,
                EyelidUp.width, EyelidUp.height), EyelidUp);

            GUI.DrawTexture(new Rect(imageX + Anchor.EyeHighlightAnchor.Item1.x + leftEyeOffset.x,
                20 + Anchor.EyeHighlightAnchor.Item1.y + leftEyeOffset.y,
                EyeHighlightLeft.width, EyeHighlightLeft.height), EyeHighlightLeft);
            GUI.DrawTexture(new Rect(imageX + Anchor.EyeHighlightAnchor.Item2.x + rightEyeOffset.x,
                20 + Anchor.EyeHighlightAnchor.Item2.y + rightEyeOffset.y,
                EyeHighlightRight.width, EyeHighlightRight.height), EyeHighlightRight);
        } else {
            Vector2 eyeSpacialOffset = Vector2.zero;
            if(followMouse) {
                eyeSpacialOffset = eyeOffset * 0.5f;
            }
            GUI.DrawTexture(new Rect(imageX + Anchor.EyeSpecialAnchor[(int)face.EyeSpecial - 1].x + eyeSpacialOffset.x,
                20 + Anchor.EyeSpecialAnchor[(int)face.EyeSpecial - 1].y + eyeSpacialOffset.y,
                EyeSpecials[(int)face.EyeSpecial - 1].width, EyeSpecials[(int)face.EyeSpecial - 1].height), EyeSpecials[(int)face.EyeSpecial - 1]);
        }
        if(face.Eyebrow != Eyebrow.None) {
            Vector2 eyebrowOffset = Vector2.zero;
            if(followMouse) {
                eyebrowOffset = eyeOffset * 0.23f;
            }

            GUI.DrawTexture(new Rect(imageX + Anchor.EyebrowAnchor[(int)face.Eyebrow - 1].x + eyebrowOffset.x,
            20 + Anchor.EyebrowAnchor[(int)face.Eyebrow - 1].y + eyebrowOffset.y,
            Eyebrows[(int)face.Eyebrow - 1].width, Eyebrows[(int)face.Eyebrow - 1].height), Eyebrows[(int)face.Eyebrow - 1]);

        }

        if(face.EffectForwardBit != EffectForwardBit.None) {
            foreach(EffectForwardBit effect in Enum.GetValues(typeof(EffectBit))) {
                if((face.EffectForwardBit & effect) == 0) {
                    continue;
                }

                int idx = OllyUtils.BitIndex((int)effect) - 1;
                var anchor = Anchor.EffectForwardAnchor[idx];
                var tex = EffectForwards[idx];

                GUI.DrawTexture(
                    new Rect(imageX + anchor.x, 20 + anchor.y,
                    tex.width, tex.height), tex
                );
            }
        }
        GUI.DrawTexture(new Rect(
            imageX + Anchor.HairAnchor.x + hairOffset.x,
            20 + Anchor.HairAnchor.y + hairOffset.y,
            Hair.width, Hair.height), Hair);
    }
}
