using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Overlayer
{
    public class OverlayerTag
    {
        public string Name = "Tag";
        public Func<string, string> Action;
        public bool NotPlaying = false;
    }
}
