using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;

namespace Overlayer
{
    public class OverlayerText : OverlayerObject
    {
        public TextMeshProUGUI text;

        public TaggedText PlayingText = new();
        public TaggedText NotPlayingText = new();

        new void Awake()
        {
            base.Awake();
            PlayingText.PlayingText = true;
            PlayingText.text =
                "<color=#${FOHex}>${Overloads}</color> <color=#${TEHex}>${CTE}</color> <color=#${VEHex}>${CVE}</color> <color=#${EPHex}>${CEP}</color> <color=#${PHex}>${CP}</color> <color=#${LPHex}>${CLP}</color> <color=#${VLHex}>${CVL}</color> <color=#${TLHex}>${CTL}</color> <color=#${FMHex}>${MissCount}</color>";
        }

        // Update is called once per frame
        new void Update()
        {
            base.Update();
            if (Main.IsPlaying) PlayingText.Update();
            else NotPlayingText.Update();
            text.text = Main.IsPlaying ? PlayingText.replacedText : NotPlayingText.replacedText;
        }

        public override JObject ToJson()
        {
            var jo = base.ToJson();
            jo["PlayingText"] = PlayingText.text;
            jo["NotPlayingText"] = NotPlayingText.text;
            jo["FontSize"] = text.fontSize;
            jo["Color"] = ColorUtility.ToHtmlStringRGBA(text.color);
            jo["Align"] = (int)text.alignment;
            jo["Type"] = "Text";
            return jo;
        }

        public override void FromJson(JObject jo)
        {
            base.FromJson(jo);
            PlayingText.text = jo["PlayingText"].Value<string>();
            NotPlayingText.text = jo["NotPlayingText"].Value<string>();
            text.fontSize = jo["FontSize"].Value<float>();
            ColorUtility.TryParseHtmlString("#" + jo["Color"].Value<string>(), out var color);
            text.color = color;
            text.alignment = (TextAlignmentOptions)jo["Align"].Value<int>();
        }
    }
}