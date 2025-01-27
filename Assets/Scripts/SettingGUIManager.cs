using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Overlayer
{
    public class SettingGUIManager : MonoBehaviour
    {
        public static SettingGUIManager Instance;
        //RenderTexture tex;
        public RenderTexture tex2;
        //public Camera cam;

        public RawImage rimg;
        void Awake()
        {
            Instance = this;
        }
#if UNITY_EDITOR
        public void Start()
        {
            Init();
            OverlayerSettings.open = true;
        }
#endif
        // Start is called before the first frame update
        public void Init()
        {
            /*tex = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            tex.Create();*/
            tex2 = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            tex2.Create();
            
            //cam.targetTexture = tex;
            rimg.texture = tex2;
        }

        // Update is called once per frame
        void Update()
        {
            if (!tex2) return;
            if (Screen.width == tex2.width && Screen.height == tex2.height) return;
            /*Destroy(tex);
            tex = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            tex.Create();*/
            Destroy(tex2);
            tex2 = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            tex2.Create();
            
            //cam.targetTexture = tex;
            rimg.texture = tex2;
        }
    }
}