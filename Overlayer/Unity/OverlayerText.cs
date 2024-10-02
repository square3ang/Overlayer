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

        private bool isDragging = false; // 드래그 중인지 여부를 저장할 변수
        private Vector2 initialPointerPosition; // 마우스 클릭 시점의 포인터 위치
        private Vector2 initialObjectPosition;  // 마우스 클릭 시점의 객체 위치

        #region Statics
        public static GameObject PCanvasObj;
        public static Canvas PublicCanvas;
        public static Shader sr_msdf;
        static OverlayerText()
        {
            PatchGuard.Ignore(() =>
            {
                sr_msdf = (Shader)typeof(ShaderUtilities).GetProperty("ShaderRef_MobileSDF", (BindingFlags)15420).GetValue(null);
            });
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
            if (!PublicCanvas)
            {
                GameObject pCanvasObj = PCanvasObj = new GameObject("Overlayer Canvas");
                PublicCanvas = pCanvasObj.AddComponent<Canvas>();
                PublicCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                PublicCanvas.sortingOrder = int.MaxValue;
                CanvasScaler scaler = pCanvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                var currentRes = Screen.currentResolution;
                scaler.referenceResolution = new Vector2(currentRes.width, currentRes.height);
                pCanvasObj.AddComponent<GraphicRaycaster>();
                DontDestroyOnLoad(PublicCanvas);

            }
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
            Text.gameObject.SetActive(config.Active);
            Initialized = true;
        }
        public void Update()
        {
            if (Main.IsPlaying) Text.text = PlayingReplacer.Replace();
            else Text.text = NotPlayingReplacer.Replace();
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
            // 마우스 클릭 시 초기 위치 저장
            isDragging = true;
            initialPointerPosition = eventData.position;
            initialObjectPosition = Text.rectTransform.anchoredPosition;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            // 마우스 버튼에서 손을 뗄 때 드래그 해제
            isDragging = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            // 드래그 중인 경우 객체 위치를 갱신
            /*if(isDragging)
            {
                Vector2 currentPointerPosition = eventData.position;
                Vector2 offset = currentPointerPosition - initialPointerPosition;
                Text.rectTransform.anchoredPosition = initialObjectPosition + offset;
            }*/
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
