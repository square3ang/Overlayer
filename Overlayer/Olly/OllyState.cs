using System;

namespace Overlayer.Olly {
    public class OllyState {
        public enum Eyebrow {
            None,
            Normal,
            Angry,
            Twist,
            Curious,
            Sad,
            AngryMore,
            Pity,
            NormalHigh
        }
        public enum Eye {
            None,
            Normal,
            Small
        }
        public enum EyeSpecial {
            None,
            Up,
            Down
        }
        public enum Mouth {
            None,
            Normal,
            Shift,
            CaretWide,
            HalfChewed,
            Smile,
            OpenSmall,
            OpenSmallHarf,
            OpenMicro,
            OpenCaret,
            Open,
            Disgust,
            OpenDisgust,
            Joker,
            SadSmall,
            Caret,
            Clenched,
            Mad,
            Surprise,
            SurpriseSmall,
            WideStretch
        }
        public enum Effect {
            None,
            Tear,
            Sweat,
            Blush
        }
        public enum EffectForward {
            None,
            Cloud,
            Tremble,
            Tendon,
            BlushBig,
        }

        [Flags]
        public enum EffectBit {
            None = 0,
            Tear = 1 << 0,
            Sweat = 2 << 0,
            Blush = 3 << 0,
        }
        [Flags]
        public enum EffectForwardBit {
            None = 0,
            Cloud = 1 << 0,
            Tremble = 2 << 0,
            Tendon = 3 << 0,
            BlushBig = 4 << 0,
        }
    }
}
