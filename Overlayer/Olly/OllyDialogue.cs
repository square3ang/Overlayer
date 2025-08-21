using System;
using System.Collections.Generic;
using static Overlayer.Olly.OllyState;

namespace Overlayer.Olly {
    public class OllyDialogue {
        public class Node {
            public string Text;
            public string[] Choices;
            public Dictionary<int, Node> Next = new();
            public Action<int> OnChoice;
            public Eyebrow Eyebrow;
            public Eye Eye;
            public EyeSpecial EyeSpecial;
            public Mouth Mouth;
            public Effect Effect;
            public EffectForward EffectForward;

            public Node(string text, string[] choices = null, Action<int> onChoice = null,
                                Eye eye = Eye.Normal, Mouth mouth = Mouth.Normal, Eyebrow eyebrow = default, EyeSpecial eyeSpecial = default, Effect effect = default, EffectForward effectForward = default) {
                Text = text;
                Choices = choices;
                OnChoice = onChoice;
                Eye = eye;
                Mouth = mouth;
                Eyebrow = eyebrow;
                EyeSpecial = eyeSpecial;
                Effect = effect;
                EffectForward = effectForward;
            }
        }
    }
}
