using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using UnityEngine;
using UnityEngine.UI.Extensions;

namespace RapidGUI
{
    public enum MouseCursor
    {
        Default,
        ResizeHorizontal,
        ResizeVertical,
        ResizeUpLeft,
    }

    public static partial class RGUIUtility
    {
        static readonly Dictionary<MouseCursor, CursorData.Data> cursorTable;
        static byte[] ConvertBitmapToByteArray(Bitmap bitmap) { byte[] result = null; if (bitmap != null) { MemoryStream stream = new MemoryStream(); bitmap.Save(stream, bitmap.RawFormat); result = stream.ToArray(); } else { Console.WriteLine("Bitmap is null."); } return result; }

        static RGUIUtility()
        {
            //var data = Resources.Load<CursorData>("cursorData");
            /*var resizeHorizontaltex = new Texture2D(2, 2);
            resizeHorizontaltex.LoadImage(ConvertBitmapToByteArray(Displayer.Properties.Resources.cursor_ew));
            var resizeVerticaltex = new Texture2D(2, 2);
            resizeVerticaltex.LoadImage(ConvertBitmapToByteArray(Displayer.Properties.Resources.cursor_ns));
            var resizeUpLefttex = new Texture2D(2, 2);
            resizeUpLefttex.LoadImage(ConvertBitmapToByteArray(Displayer.Properties.Resources.cursor_nwse));

            var resizeHorizontal = new CursorData.Data();
            resizeHorizontal.tex = resizeHorizontaltex;
            resizeHorizontal.hotspot = new Vector2Int(18, 8);

            var resizeVertical = new CursorData.Data();
            resizeVertical.tex = resizeVerticaltex;
            resizeVertical.hotspot = new Vector2Int(8, 18);

            var resizeUpLeft = new CursorData.Data();
            resizeUpLeft.tex = resizeUpLefttex;
            resizeUpLeft.hotspot = new Vector2Int(13, 13);*/


            cursorTable = new Dictionary<MouseCursor, CursorData.Data>()
            {
                { MouseCursor.Default, null},
                { MouseCursor.ResizeHorizontal, null},
                { MouseCursor.ResizeVertical, null},
                { MouseCursor.ResizeUpLeft, null},
            };

            RapidGUIBehaviour.Instance.StartCoroutine(UpdateCursor());
        }


        static float cursorLimitTime;
        static float GetCursorTime() => Time.realtimeSinceStartup;

        public static void SetCursor(MouseCursor cursor, float life = 0.1f)
        {
            if (cursor == MouseCursor.Default)
            {
                SetCursorDefault();
            }
            else
            {
                var data = cursorTable[cursor];

                Cursor.SetCursor(data.tex, data.hotspot, CursorMode.Auto);
                cursorLimitTime = GetCursorTime() + life;
            }
        }

        public static void SetCursorDefault()
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            cursorLimitTime = float.MaxValue;
        }


        static IEnumerator UpdateCursor()
        {
            while (true)
            {
                yield return new WaitUntil(() => GetCursorTime() > cursorLimitTime);
                SetCursorDefault();
            }
        }
    }
}