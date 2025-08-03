using Overlayer.Core;
using Overlayer.Core.TextReplacing;
using Overlayer.Models;
using Overlayer.Patches;
using Overlayer.Tags;
using Overlayer.Utils;
using System;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.Random;

namespace Overlayer.Unity
{
    public class OverlayerText : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        public static event Action<OverlayerText> OnApplyConfig = delegate { };
        public bool Initialized { get; private set; }
        public TextConfig Config;
        public Replacer PlayingReplacer;
        public Replacer NotPlayingReplacer;
        public TextMeshProUGUI Text;
        public static GameObject DragObj;
        public static Image DragImage;
        public static Outline DragOutline;

        public bool isFrontDragging = false;
        private bool isDragging = false;
        private Vector2 initialPointerPosition;
        private Vector2 initialObjectPosition;


        #region Statics
        public static GameObject PCanvasObj;
        public static Canvas PublicCanvas;
        public static Shader sr_msdf;
        static OverlayerText()
        {
            sr_msdf = (Shader)typeof(ShaderUtilities).GetProperty("ShaderRef_MobileSDF", (BindingFlags)15420).GetValue(null);
        }
        #endregion
        public void Init(TextConfig config)
        {
            if (Initialized) return;
            Config = config;
            if (string.IsNullOrEmpty(config.Name))
                config.Name = $"Text {TextManager.Count + 1}";
            PlayingReplacer = new Replacer(config.PlayingText, TagManager.All.Select(ot => ot.Tag));
            NotPlayingReplacer = new Replacer(config.NotPlayingText, TagManager.NP.Select(ot => ot.Tag));
            DontDestroyOnLoad(gameObject);
            PublicCanvasInit();
            DragInit();
            GameObject mainObject = gameObject;
            mainObject.transform.SetParent(PublicCanvas.transform);
            mainObject.MakeFlexible();
            Text = mainObject.AddComponent<TextMeshProUGUI>();
            Text.enableVertexGradient = true;
            Text.color = Color.white;
            Text.colorGradient = config.TextColor;
            var rt = Text.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = config.Pivot;
            rt.localScale = config.Scale;
            Text.enableAutoSizing = false;
            Text.lineSpacing = config.LineSpacing;
            Text.lineSpacingAdjustment = config.LineSpacingAdj;
            rt.eulerAngles = config.Rotation;
            SetFont();
            Material[] sharedMaterials = Text.fontSharedMaterials;
            for (int i = 0; i < sharedMaterials.Length; i++)
            {
                var mat = new Material(sharedMaterials[i]);
                InitMaterial(mat);
                ApplyMaterial(mat);
                sharedMaterials[i] = mat;
            }
            Text.fontSharedMaterials = sharedMaterials;
            config.OnDragChanged += (state) => {
                Text.raycastTarget = state;
            };
            Text.raycastTarget = config.Drag;
            Text.gameObject.SetActive(config.Active);

            Initialized = true;
        }
        public static void PublicCanvasInit() {
            if(PublicCanvas) {
                return;
            }
            GameObject pCanvasObj = PCanvasObj = new GameObject("Overlayer Canvas");
            PublicCanvas = pCanvasObj.AddComponent<Canvas>();
            PublicCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            PublicCanvas.sortingOrder = 32760;
            CanvasScaler scaler = pCanvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            var currentRes = Screen.currentResolution;
            scaler.referenceResolution = new Vector2(currentRes.width, currentRes.height);
            pCanvasObj.AddComponent<GraphicRaycaster>();
            DontDestroyOnLoad(PublicCanvas);
        }
        public static void DragInit() {
            if (DragObj != null) {
                return;
            }
            if(PublicCanvas == null) {
                PublicCanvasInit();
            }
            DragObj = new GameObject("Outline");
            DragObj.transform.SetParent(PublicCanvas.transform);
            DragObj.transform.localPosition = Vector3.zero;
            DragImage = DragObj.AddComponent<Image>();
            DragImage.color = new Color(1.0f, 1.0f, 1.0f, 0.16f);
            DragImage.sprite = null;
            DragImage.type = Image.Type.Simple;
            DragImage.rectTransform.sizeDelta = Vector2.zero;
            DragOutline = DragObj.AddComponent<Outline>();
            DragOutline.effectColor = Color.cyan;
            DragOutline.effectDistance = new Vector2(3, 3);
            DragOutline.useGraphicAlpha = true;
            DragOutline.enabled = true;
            DragObj.SetActive(false);
        }
        public void Update()
        {
            if (Main.IsPlaying) Text.text = PlayingReplacer.Replace();
            else Text.text = NotPlayingReplacer.Replace();
            if (isDragging) {
                DragObj.transform.position = Text.gameObject.transform.position;
                DragObj.transform.rotation = Text.gameObject.transform.rotation;
                DragImage.rectTransform.pivot = Text.rectTransform.pivot;
                DragImage.rectTransform.sizeDelta = new Vector2(Text.preferredWidth, Text.preferredHeight);
            }
        }
        public void ApplyConfig()
        {
            PlayingReplacer.Source = Config.PlayingText;
            NotPlayingReplacer.Source = Config.NotPlayingText;
            var lexConfig = MiscUtils.CreateLexConfigFromString(Config.LexOption);
            PlayingReplacer.SetLexConfig(lexConfig);
            NotPlayingReplacer.SetLexConfig(lexConfig);
            PlayingReplacer.UpdateTags(TagManager.All.Select(ot => ot.Tag));
            NotPlayingReplacer.UpdateTags(TagManager.NP.Select(ot => ot.Tag));
            PlayingReplacer.Compile();
            NotPlayingReplacer.Compile();
            TagManager.UpdatePatch();
            Text.lineSpacing = Config.LineSpacing;
            Text.lineSpacingAdjustment = Config.LineSpacingAdj;
            Text.colorGradient = Config.TextColor;
            Text.rectTransform.pivot = Config.Pivot;
            Text.rectTransform.localScale = Config.Scale;
            Text.rectTransform.anchoredPosition = (Config.Position - new Vector2(0.5f, 0.5f)) * new Vector2(Screen.width, Screen.height);
            Text.rectTransform.eulerAngles = Config.Rotation;
            Text.fontSize = Config.FontSize;
            Text.alignment = Config.Alignment;
            SetFont();
            Material[] sharedMaterials = Text.fontSharedMaterials;
            for (int i = 0; i < sharedMaterials.Length; i++)
            {
                var mat = new Material(sharedMaterials[i]);
                ApplyMaterial(mat);
                sharedMaterials[i] = mat;
            }
            Text.fontSharedMaterials = sharedMaterials;
            OnApplyConfig(this);
        }
        private static void InitMaterial(Material mat)
        {
            if (sr_msdf) mat.shader = sr_msdf;
            mat.EnableKeyword(ShaderUtilities.Keyword_Outline);
            mat.EnableKeyword(ShaderUtilities.Keyword_Underlay);
        }
        private void ApplyMaterial(Material mat)
        {
            mat.SetColor(ShaderUtilities.ID_OutlineColor, Config.OutlineColor);
            mat.SetFloat(ShaderUtilities.ID_OutlineWidth, Config.OutlineWidth);
            mat.SetColor(ShaderUtilities.ID_UnderlayColor, Config.ShadowColor);
            mat.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, Config.ShadowOffset.x);
            mat.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, Config.ShadowOffset.y);
            mat.SetFloat(ShaderUtilities.ID_UnderlayDilate, 1 - Config.ShadowDilate);
            mat.SetFloat(ShaderUtilities.ID_UnderlaySoftness, 1 - Config.ShadowSoftness);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if(isFrontDragging || !Config.Drag) {
                return;
            }
            isFrontDragging = true;
            isDragging = true;
            initialPointerPosition = eventData.position;
            initialObjectPosition = Text.rectTransform.anchoredPosition;

            DragObj.transform.SetParent(Text.transform);
            DragObj.SetActive(true);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if(isDragging) {
                isDragging = false;
                isFrontDragging = false;
                DragObj.SetActive(false);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if(isDragging)
            {
                Vector2 currentPointerPosition = eventData.position;
                Vector2 offset = currentPointerPosition - initialPointerPosition;
                Text.rectTransform.anchoredPosition = initialObjectPosition + offset;

                Vector2 screenSize = new Vector2(Screen.width, Screen.height);
                Config.Position = (Text.rectTransform.anchoredPosition / screenSize) + new Vector2(0.5f, 0.5f);
            }
        }

        private void SetFont()
        {
            if (FontManager.TryGetFont(Config.Font, out FontData font))
            {
                TMP_FontAsset targetFont = font.fontTMP;
                if (Config.EnableFallbackFonts)
                {
                    targetFont = TMP_FontAsset.CreateFontAsset(font.font);
                    var fallbacks = Config.FallbackFonts?.Select(f => FontManager.GetFont(f)).Where(d => d != null);
                    targetFont.fallbackFontAssetTable = fallbacks.Select(fd => fd.Value.fontTMP).ToList();
                }
                InitMaterial(targetFont.material);
                Text.font = targetFont;
            }
        }
    }
}
