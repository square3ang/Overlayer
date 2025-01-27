using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Overlayer
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method)]
    public class TagAttribute : Attribute
    {
        public string Name;
        //public string Description;
        public object Dummy;
        public bool NotPlaying;

        /*public TagAttribute(string name, string description)
        {
            Name = name;
            Description = description;
        }*/

        public TagAttribute(string name)
        {
            Name = name;
        }

        public TagAttribute()
        {
            
        }
    }
}
