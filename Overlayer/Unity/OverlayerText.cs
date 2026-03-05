using Overlayer.Core;
using Overlayer.Core.TextReplacing;
using Overlayer.Models;
using Overlayer.Tags;
using Overlayer.Utils;
using System;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Overlayer.Unity;

public class OverlayerText : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IPointerEnterHandler, IPointerExitHandler {
    public static event Action<OverlayerText> OnApplyConfig = delegate { };
    public bool Initialized { get; private set; }
    public TextConfig Config;
    public Replacer PlayingReplacer;
    public Replacer NotPlayingReplacer;
    public TextMeshProUGUI Text;
    public static GameObject DragObj;
    public static Image DragImage;

    private static bool isAlreadyDragging;
    private static int pointingCount = 0;
    private bool isDragging = false;
    private bool isPointing = false;
    private Vector2 initialPointerPosition;
    private Vector2 initialObjectPosition;

    #region Statics
    public static GameObject PCanvasObj;
    public static Canvas PublicCanvas;
    public static Shader sr_msdf;
    static OverlayerText() => sr_msdf = (Shader)typeof(ShaderUtilities).GetProperty("ShaderRef_MobileSDF", (BindingFlags)15420).GetValue(null);
    #endregion
    public void Init(TextConfig config) {
        if(Initialized) {
            return;
        }

        Config = config;
        if(string.IsNullOrEmpty(config.Name)) {
            config.Name = $"Text {TextManager.Count + 1}";
        }

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
        Text.enableAutoSizing = false;
        var rt = Text.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        ApplyConfig();
        config.OnDragChanged += (state) => Text.raycastTarget = state;
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
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        pCanvasObj.AddComponent<GraphicRaycaster>();
        DontDestroyOnLoad(PublicCanvas);
    }
    public static void DragInit() {
        if(DragObj != null) {
            return;
        }
        if(PublicCanvas == null) {
            PublicCanvasInit();
        }
        DragObj = new GameObject("Outline");
        DragObj.transform.SetParent(PublicCanvas.transform);
        DragObj.transform.localPosition = Vector3.zero;
        DragImage = DragObj.AddComponent<Image>();

        Texture2D outlinetex = new(3, 3, TextureFormat.RGBA32, false);
        Color[] outlinetexpixels = new Color[] {
            Color.white, Color.white, Color.white,
            Color.white, Color.clear, Color.white,
            Color.white, Color.white, Color.white,
        };
        outlinetex.SetPixels(outlinetexpixels);
        outlinetex.Apply();
        outlinetex.filterMode = FilterMode.Point;
        Sprite outline = Sprite.Create(
            outlinetex,
            new Rect(0, 0, 3, 3),
            new Vector2(0.5f, 0.5f),
            32f,
            0,
            SpriteMeshType.FullRect,
            new Vector4(1, 1, 1, 1)
        );

        DragImage.color = new Color(0.0f, 1.0f, 1.0f, 0.8f);
        DragImage.sprite = outline;
        DragImage.type = Image.Type.Sliced;
        DragImage.rectTransform.sizeDelta = Vector2.zero;
        DragObj.SetActive(false);
    }
    public void Update() {
        Text.text = Main.IsPlaying ? PlayingReplacer.Replace() : NotPlayingReplacer.Replace();

        if(isDragging) {
            DragObj.transform.position = Text.gameObject.transform.position;
            DragObj.transform.rotation = Text.gameObject.transform.rotation;
            DragImage.rectTransform.pivot = Text.rectTransform.pivot;
            DragImage.rectTransform.sizeDelta = new Vector2(Text.preferredWidth, Text.preferredHeight);
        }
    }
    public void ApplyConfig() {
        PlayingReplacer.Source = Config.PlayingText;
        NotPlayingReplacer.Source = Config.NotPlayingText;
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
        Text.rectTransform.anchoredPosition = (Config.Position - new Vector2(0.5f, 0.5f)) * new Vector2(1920, 1080);
        Text.rectTransform.eulerAngles = Config.Rotation;
        Text.fontSize = Config.FontSize;
        Text.alignment = Config.Alignment;
        SetFont();
        Material[] sharedMaterials = Text.fontSharedMaterials;
        for(int i = 0; i < sharedMaterials.Length; i++) {
            var mat = new Material(sharedMaterials[i]);
            ApplyMaterial(mat);
            sharedMaterials[i] = mat;
        }
        Text.fontSharedMaterials = sharedMaterials;
        OnApplyConfig(this);
    }
    private static void InitMaterial(Material mat) {
        if(sr_msdf) {
            mat.shader = sr_msdf;
        }

        mat.EnableKeyword(ShaderUtilities.Keyword_Outline);
        mat.EnableKeyword(ShaderUtilities.Keyword_Underlay);
    }
    private void ApplyMaterial(Material mat) {
        mat.SetColor(ShaderUtilities.ID_OutlineColor, Config.OutlineColor);
        mat.SetFloat(ShaderUtilities.ID_OutlineWidth, Config.OutlineWidth);
        mat.SetColor(ShaderUtilities.ID_UnderlayColor, Config.ShadowColor);
        mat.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, Config.ShadowOffset.x);
        mat.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, Config.ShadowOffset.y);
        mat.SetFloat(ShaderUtilities.ID_UnderlayDilate, 1 - Config.ShadowDilate);
        mat.SetFloat(ShaderUtilities.ID_UnderlaySoftness, 1 - Config.ShadowSoftness);
    }

    public string GetCurrentText() => Text.text;

    public void OnPointerDown(PointerEventData e) {
        if(isAlreadyDragging) {
            return;
        }
        isDragging = true;
        isAlreadyDragging = true;
        initialPointerPosition = e.position;
        initialObjectPosition = Text.rectTransform.anchoredPosition;
    }

    public void OnPointerUp(PointerEventData e) {
        if(isDragging) {
            isDragging = false;
            isAlreadyDragging = false;
        }
    }

    public void OnDrag(PointerEventData e) {
        if(isDragging) {
            Vector2 currentPointerPosition = e.position;
            Vector2 offset = currentPointerPosition - initialPointerPosition;
            Text.rectTransform.anchoredPosition = initialObjectPosition + offset;

            Vector2 screenSize = new(1920, 1080);
            Config.Position = (Text.rectTransform.anchoredPosition / screenSize) + new Vector2(0.5f, 0.5f);
        }
    }

    public void OnPointerEnter(PointerEventData e) {
        isPointing = true;
        pointingCount++;
        if(!isAlreadyDragging) {
            DragObj.transform.SetParent(Text.transform);
            DragObj.transform.position = Text.gameObject.transform.position;
            DragObj.transform.rotation = Text.gameObject.transform.rotation;
            DragImage.rectTransform.pivot = Text.rectTransform.pivot;
            DragImage.rectTransform.sizeDelta = new Vector2(Text.preferredWidth, Text.preferredHeight);
        }
        DragObj.SetActive(true);
    }

    public void OnPointerExit(PointerEventData e) {
        pointingCount--;
        if(pointingCount == 0) {
            if(!isAlreadyDragging) {
                DragObj.SetActive(false);
            }
        } else if(pointingCount < 0) {
            pointingCount = 0;
        }
        isPointing = false;
    }

    private void SetFont() {
        if(FontManager.TryGetFont(Config.Font, out FontData font)) {
            TMP_FontAsset targetFont = font.fontTMP;
            if(Config.EnableFallbackFonts) {
                targetFont = TMP_FontAsset.CreateFontAsset(font.font);
                var fallbacks = Config.FallbackFonts?.Select(FontManager.GetFont).Where(d => d != null);
                targetFont.fallbackFontAssetTable = fallbacks.Select(fd => fd.Value.fontTMP).ToList();
            }
            InitMaterial(targetFont.material);
            Text.font = targetFont;
        }
    }
}
