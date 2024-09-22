using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Overlayer.Unity
{
    public class Capturer : MonoBehaviour
    {
        //public static Capturer Instance
        //{
        //    get
        //    {
        //        if (instance == null && (instance = FindObjectOfType<Capturer>()) == null)
        //        {
        //            var go = new GameObject("Overlayer Capturer");
        //            instance = go.AddComponent<Capturer>();
        //            DontDestroyOnLoad(go);
        //        }
        //        return instance;
        //    }
        //}
        //private static Capturer instance;

        //private RenderTexture rt;
        //private Vector2Int size;

        //public Vector2Int Size
        //{
        //    get => rt != null ? size = new Vector2Int(rt.width, rt.height) : Vector2Int.zero;
        //    set => size = value;
        //}
        
        //public void Apply(Camera target)
        //{
        //    rt?.Release();
        //    rt = new RenderTexture(size.x, size.y, 16, RenderTextureFormat.ARGB32);
        //    rt.Create();
        //    target.targetTexture = rt;
        //}

        //void OnDestroy()
        //{
        //    rt?.Release();
        //}
    }
}
