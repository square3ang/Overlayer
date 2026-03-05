using System.Collections.Generic;
using UnityEngine;

namespace RapidGUI {
    public class IMColorPreset : ScriptableObject {

        public List<Color> Colors {
            get {
                return colors;
            }
        }

        [SerializeField] List<Color> colors = new();

        public void Save(Color color) {
            colors.Add(color);
        }

        public void Remove(int index) {
            colors.RemoveAt(index);
        }

    }

}

