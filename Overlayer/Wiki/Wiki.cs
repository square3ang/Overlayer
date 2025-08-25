using Overlayer.Core;
using Overlayer.Olly;
using RapidGUI;
using System.ComponentModel;
using UnityEngine;

namespace Overlayer.Wiki {
    public class Wiki : MonoBehaviour {
        private Rect windowRect;

        private void Start() {
            int initWidth = Screen.width / 2;
            int initHeight = Screen.height / 2;
            windowRect = new Rect(
                (Screen.width - initWidth) / 2f,
                (Screen.height - initHeight) / 2f,
                initWidth,
                initHeight
            );
        }

        private void OnGUI() {
            windowRect = GUI.Window(823, windowRect, DrawWindow, "Overlayer Wiki", RGUIStyle.darkWindow);
        }

        private void DrawWindow(int windowID) {


            GUI.DragWindow();
        }
    }
}
