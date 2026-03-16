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

namespace Overlayer.Unity;

public class OverlayerText : OverlayerObject, IPointerDownHandler, IPointerUpHandler, IDragHandler, IPointerEnterHandler, IPointerExitHandler {
    public static event Action<OverlayerText> OnApplyConfig = delegate { };
    public bool Initialized { get; private set; }
    private TextConfig _config;
    public override ObjectConfig Config => _config;
    public TextConfig TextConfig => _config;
    public Replacer PlayingReplacer;
    public Replacer NotPlayingReplacer;
    public TextMeshProUGUI Text;

    private static bool isAlreadyDragging;
    private static int pointingCount = 0;
    private bool isDragging = false;
    private bool isPointing = false;
    private Vector2 initialObjectPosition;
    private Vector2 initialPointerLocal;

    #region Statics
    public static Shader sr_msdf;
    static OverlayerText() => sr_msdf = (Shader)typeof(ShaderUtilities).GetProperty("ShaderRef_MobileSDF", (BindingFlags)15420).GetValue(null);
    #endregion
    public void Init(OverlayerProfile profile, TextConfig config) {
        if(Initialized) {
            return;
        }

        Parent = profile;
        _config = config;
        if(string.IsNullOrEmpty(config.Name)) {
            config.Name = $"Text {Parent.ObjectManager.Count + 1}";
        }

        PlayingReplacer = new Replacer(config.PlayingText, TagManager.All.Select(ot => ot.Tag));
        NotPlayingReplacer = new Replacer(config.NotPlayingText, TagManager.NP.Select(ot => ot.Tag));
        DontDestroyOnLoad(gameObject);
        GameObject mainObject = gameObject;
        mainObject.transform.SetParent(Parent.ProfileCanvas.transform);
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

    public void Update() {
        if(!Initialized || Text == null) {
            return;
        }

        Text.text = Main.IsPlaying ? PlayingReplacer?.Replace() ?? "" : NotPlayingReplacer?.Replace() ?? "";

        if(isDragging && OverlayerProfile.DragObj != null && OverlayerProfile.DragImage != null) {
            OverlayerProfile.DragObj.transform.position = Text.gameObject.transform.position;
            OverlayerProfile.DragObj.transform.rotation = Text.gameObject.transform.rotation;
            OverlayerProfile.DragImage.rectTransform.pivot = Text.rectTransform.pivot;
            OverlayerProfile.DragImage.rectTransform.sizeDelta = new Vector2(Text.preferredWidth, Text.preferredHeight);
        }
    }
    public override void ApplyConfig() {
        PlayingReplacer.Source = _config.PlayingText;
        NotPlayingReplacer.Source = _config.NotPlayingText;
        PlayingReplacer.UpdateTags(TagManager.All.Select(ot => ot.Tag));
        NotPlayingReplacer.UpdateTags(TagManager.NP.Select(ot => ot.Tag));
        PlayingReplacer.Compile();
        NotPlayingReplacer.Compile();
        TagManager.UpdatePatch();
        Text.lineSpacing = _config.LineSpacing;
        Text.lineSpacingAdjustment = _config.LineSpacingAdj;
        Text.colorGradient = _config.TextColor;
        Text.rectTransform.pivot = _config.Pivot;
        Text.rectTransform.localScale = _config.Scale;
        Text.rectTransform.anchoredPosition = (_config.Position - new Vector2(0.5f, 0.5f)) * new Vector2(1920, 1080);
        Text.rectTransform.eulerAngles = _config.Rotation;
        Text.fontSize = _config.FontSize;
        Text.alignment = _config.Alignment;
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
        mat.SetColor(ShaderUtilities.ID_OutlineColor, _config.OutlineColor);
        mat.SetFloat(ShaderUtilities.ID_OutlineWidth, _config.OutlineWidth);
        mat.SetColor(ShaderUtilities.ID_UnderlayColor, _config.ShadowColor);
        mat.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, _config.ShadowOffset.x);
        mat.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, _config.ShadowOffset.y);
        mat.SetFloat(ShaderUtilities.ID_UnderlayDilate, 1 - _config.ShadowDilate);
        mat.SetFloat(ShaderUtilities.ID_UnderlaySoftness, 1 - _config.ShadowSoftness);
    }

    public string GetCurrentText() => Text.text;

    public void OnPointerDown(PointerEventData e) {
        if(isAlreadyDragging) {
            return;
        }

        isDragging = true;
        isAlreadyDragging = true;

        RectTransform parentRect = Text.rectTransform.parent as RectTransform;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            e.position,
            e.pressEventCamera,
            out initialPointerLocal
        );

        initialObjectPosition = Text.rectTransform.anchoredPosition;
    }

    public void OnPointerUp(PointerEventData e) {
        if(isDragging) {
            isDragging = false;
            isAlreadyDragging = false;
        }
    }

    public void OnDrag(PointerEventData e) {
        if(!isDragging) {
            return;
        }

        RectTransform parentRect = Text.rectTransform.parent as RectTransform;
        Vector2 currentPointerLocal;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            e.position,
            e.pressEventCamera,
            out currentPointerLocal
        );

        Vector2 offset = currentPointerLocal - initialPointerLocal;
        Text.rectTransform.anchoredPosition = initialObjectPosition + offset;

        Vector2 canvasSize = new Vector2(1920, 1080);
        _config.Position = (Text.rectTransform.anchoredPosition / canvasSize) + new Vector2(0.5f, 0.5f);
    }

    public void OnPointerEnter(PointerEventData e) {
        isPointing = true;
        pointingCount++;
        if(!isAlreadyDragging) {
            OverlayerProfile.DragObj.transform.position = Text.gameObject.transform.position;
            OverlayerProfile.DragObj.transform.rotation = Text.gameObject.transform.rotation;
            OverlayerProfile.DragImage.rectTransform.pivot = Text.rectTransform.pivot;
            OverlayerProfile.DragImage.rectTransform.sizeDelta = new Vector2(Text.preferredWidth, Text.preferredHeight);
        }
        OverlayerProfile.DragObj.SetActive(true);
    }

    public void OnPointerExit(PointerEventData e) {
        pointingCount--;
        if(pointingCount <= 0) {
            pointingCount = 0;
            if(!isAlreadyDragging) {
                OverlayerProfile.DragObj.SetActive(false);
            }
        }
        isPointing = false;
    }

    private void SetFont() {
        if(FontManager.TryGetFont(_config.Font, out FontData font)) {
            TMP_FontAsset targetFont = font.fontTMP;
            if(_config.EnableFallbackFonts) {
                targetFont = TMP_FontAsset.CreateFontAsset(font.font);
                var fallbacks = _config.FallbackFonts?.Select(FontManager.GetFont).Where(d => d != null);
                targetFont.fallbackFontAssetTable = fallbacks.Select(fd => fd.Value.fontTMP).ToList();
            }
            InitMaterial(targetFont.material);
            Text.font = targetFont;
        }
    }
}
