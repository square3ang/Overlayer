using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Overlayer
{
    public class TaggedText
    {
        public static Regex tagRegex = new Regex(@"\$\{(.*?)\}", RegexOptions.Compiled);
        public Dictionary<string, Func<string, string>> tagReplacer = new();


        private string _text = "";

        public string replacedText;

        public bool PlayingText = false;

        public void ParseTags()
        {
            tagReplacer.Clear();
            foreach (Match match in tagRegex.Matches(_text))
            {
                var actualTag = match.Value[2..^1].Split(':');
                if (tagReplacer.ContainsKey(match.Value)) continue; 
                if (!Main.Tags.TryGetValue(actualTag[0], out var tag)) continue;
                if (!tag.NotPlaying && !PlayingText) continue;
                var arg = actualTag.Length > 1 ? actualTag[1] : "";
                tagReplacer[match.Value] = a => a.Replace(match.Value, tag.Action(arg));
                
            }
        }

        public string text
        {
            get => _text;
            set
            {
                if (value == _text) return;
                _text = value;
                ParseTags();
            }
        }

        public void Update()
        {
            replacedText = _text;
            foreach (var replacer in tagReplacer)
            {
                replacedText = replacer.Value(replacedText);
            }
        }
    }
}
