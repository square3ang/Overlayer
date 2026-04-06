
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

    public override ObjectConfig Config => TextConfig;
    public TextConfig TextConfig { get; private set; }
    public Replacer PlayingReplacer;
    public Replacer NotPlayingReplacer;
    public TextMeshProUGUI Text;
    private Material[] _instancedMaterials;
    private bool _fontChanged = false;

    private static bool isAlreadyDragging;
    private static int pointingCount = 0;
    private bool isDragging = false;
    private bool isPointing = false;
    private Vector2 initialObjectPosition;
    private Vector2 initialPointerLocal;

    public bool CanDrag => TextConfig.Drag && !TextConfig.Position.IsExpr;

    public static Shader sr_msdf;
    static OverlayerText() => sr_msdf = (Shader)typeof(ShaderUtilities).GetProperty("ShaderRef_MobileSDF", (BindingFlags)15420).GetValue(null);

    public void Init(OverlayerProfile profile, TextConfig config) {
        if(Initialized) {
            return;
        }

        Parent = profile;
        TextConfig = config;
        if(string.IsNullOrEmpty(config.Name)) {
            config.Name = $"Text {Parent.ObjectManager.Count + 1}";
        }
        PlayingReplacer = new Replacer(config.PlayingText, TagManager.All.Select(ot => ot.Tag));
        NotPlayingReplacer = new Replacer(config.NotPlayingText, TagManager.NP.Select(ot => ot.Tag));
        TagManager.OnLoadUnload += RefreshTags;
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
        config.Init();
        ApplyConfig();
        config.OnDragChanged += (state) => Text.raycastTarget = state;
        Text.raycastTarget = config.Drag;
        Text.gameObject.SetActive(config.Active);
        Initialized = true;
    }

    public void Update() {
        if(!Initialized || !Text) {
            return;
        }

        Text.text = Main.IsPlaying ? PlayingReplacer?.Replace() ?? "" : NotPlayingReplacer?.Replace() ?? "";

        if(TextConfig.LineSpacing.GetExprValue(float.Parse, out var ls)) {
            Text.lineSpacing = ls;
        }
        if(TextConfig.LineSpacingAdj.GetExprValue(float.Parse, out var lsa)) {
            Text.lineSpacingAdjustment = lsa;
        }
        if(TextConfig.TextColor.GetExprValue(MiscUtils.ParseGColor, out var color)) {
            Text.colorGradient = color;
        }
        if(TextConfig.Pivot.GetExprValue(MiscUtils.ParseVec2, out var pivot)) {
            Text.rectTransform.pivot = pivot;
        }
        if(TextConfig.Scale.GetExprValue(MiscUtils.ParseVec2, out var scale)) {
            Text.rectTransform.localScale = scale;
        }
        if(TextConfig.Position.GetExprValue(MiscUtils.ParseVec2, out var pos)) {
            Text.rectTransform.anchoredPosition = (pos - new Vector2(0.5f, 0.5f)) * new Vector2(1920, 1080);
        }
        if(TextConfig.Rotation.GetExprValue(MiscUtils.ParseVec3, out var rot)) {
            Text.rectTransform.eulerAngles = rot;
        }
        if(TextConfig.FontSize.GetExprValue(float.Parse, out var fs)) {
            Text.fontSize = fs;
        }

        if(_fontChanged || _instancedMaterials == null || _instancedMaterials.Length == 0) {
            Material[] shared = Text.fontSharedMaterials;
            if(shared != null && shared.Length > 0 && shared[0]) {
                RefreshMaterials(shared);
                _fontChanged = false;
            }
        }

        if(_instancedMaterials != null) {
            UpdateMaterialExpressions();
        }

        if(isDragging && OverlayerProfile.DragObj && OverlayerProfile.DragImage) {
            OverlayerProfile.DragObj.transform.position = Text.gameObject.transform.position;
            OverlayerProfile.DragObj.transform.rotation = Text.gameObject.transform.rotation;
            OverlayerProfile.DragImage.rectTransform.pivot = Text.rectTransform.pivot;
            OverlayerProfile.DragImage.rectTransform.sizeDelta = new Vector2(Text.preferredWidth, Text.preferredHeight);
        }
    }

    private void RefreshTags() {
        PlayingReplacer.UpdateTags(TagManager.All.Select(ot => ot.Tag));
        NotPlayingReplacer.UpdateTags(TagManager.NP.Select(ot => ot.Tag));
        PlayingReplacer.Compile();
        NotPlayingReplacer.Compile();
    }

    public override void ApplyConfig() {
        PlayingReplacer.Source = TextConfig.PlayingText;
        NotPlayingReplacer.Source = TextConfig.NotPlayingText;
        RefreshTags();
        TagManager.UpdatePatch();

        if(TextConfig.LineSpacing.GetNormalValue(out var ls)) {
            Text.lineSpacing = ls;
        }
        if(TextConfig.LineSpacingAdj.GetNormalValue(out var lsa)) {
            Text.lineSpacingAdjustment = lsa;
        }
        if(TextConfig.TextColor.GetNormalValue(out var color)) {
            Text.colorGradient = color;
        }
        if(TextConfig.Pivot.GetNormalValue(out var pivot)) {
            Text.rectTransform.pivot = pivot;
        }
        if(TextConfig.Scale.GetNormalValue(out var scale)) {
            Text.rectTransform.localScale = scale;
        }
        if(TextConfig.Position.GetNormalValue(out var pos)) {
            Text.rectTransform.anchoredPosition = (pos - new Vector2(0.5f, 0.5f)) * new Vector2(1920, 1080);
        }
        if(TextConfig.Rotation.GetNormalValue(out var rot)) {
            Text.rectTransform.eulerAngles = rot;
        }
        if(TextConfig.FontSize.GetNormalValue(out var fs)) {
            Text.fontSize = fs;
        }
        Text.alignment = TextConfig.Alignment;

        SetFont();
        UpdateMaterials();
        OnApplyConfig(this);
    }

    public string GetCurrentText() => Text.text;

    public void OnPointerDown(PointerEventData e) {
        if(!CanDrag || isAlreadyDragging) {
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
        if(!CanDrag || isDragging) {
            isDragging = false;
            isAlreadyDragging = false;
        }
    }

    public void OnDrag(PointerEventData e) {
        if(!CanDrag || !isDragging) {
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

        Vector2 canvasSize = new(1920, 1080);
        TextConfig.Position.Value = (Text.rectTransform.anchoredPosition / canvasSize) + new Vector2(0.5f, 0.5f);
    }

    public void OnPointerEnter(PointerEventData e) {
        if(!CanDrag) {
            return;
        }
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
        if(!CanDrag) {
            return;
        }
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
        if(!FontManager.TryGetFont(TextConfig.Font, out FontData font)) {
            return;
        }

        TMP_FontAsset targetFont = font.fontTMP;
        if(!targetFont) {
            return;
        }
        Text.font = targetFont;
        Text.fontSharedMaterial = targetFont.material;
        if(_instancedMaterials != null) {
            foreach(var m in _instancedMaterials) {
                if(m) {
                    Destroy(m);
                }
            }
            _instancedMaterials = null;
        }
        RefreshMaterials(Text.fontSharedMaterials);
        _fontChanged = true;
    }

    private void UpdateMaterials() {
        if(_instancedMaterials == null) {
            return;
        }

        foreach(var mat in _instancedMaterials) {
            if(!mat) {
                continue;
            }

            if(TextConfig.OutlineColor.GetNormalValue(out var oc)) {
                mat.SetColor(ShaderUtilities.ID_OutlineColor, oc);
            }
            if(TextConfig.OutlineWidth.GetNormalValue(out var ow)) {
                mat.SetFloat(ShaderUtilities.ID_OutlineWidth, ow);
            }
            if(TextConfig.ShadowColor.GetNormalValue(out var sc)) {
                mat.SetColor(ShaderUtilities.ID_UnderlayColor, sc);
            }
            if(TextConfig.ShadowOffset.GetNormalValue(out var so)) {
                mat.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, so.x);
                mat.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, so.y);
            }
            if(TextConfig.ShadowDilate.GetNormalValue(out var sd)) {
                mat.SetFloat(ShaderUtilities.ID_UnderlayDilate, 1 - sd);
            }
            if(TextConfig.ShadowSoftness.GetNormalValue(out var ss)) {
                mat.SetFloat(ShaderUtilities.ID_UnderlaySoftness, 1 - ss);
            }
        }
        Text.UpdateMeshPadding();
        Text.SetMaterialDirty();
    }

    private void RefreshMaterials(Material[] shared) {
        _instancedMaterials = new Material[shared.Length];
        for(int i = 0; i < shared.Length; i++) {
            if(!shared[i]) {
                continue;
            }
            _instancedMaterials[i] = new Material(shared[i]);
            if(sr_msdf) {
                _instancedMaterials[i].shader = sr_msdf;
            }
            _instancedMaterials[i].EnableKeyword(ShaderUtilities.Keyword_Outline);
            _instancedMaterials[i].EnableKeyword(ShaderUtilities.Keyword_Underlay);
        }
        Text.fontSharedMaterials = _instancedMaterials;
    }

    private void UpdateMaterialExpressions() {
        bool changed = false;
        foreach(var mat in _instancedMaterials) {
            if(!mat) {
                continue;
            }

            if(TextConfig.OutlineColor.GetExprValue(MiscUtils.ParseColor, out var oc)) {
                mat.SetColor(ShaderUtilities.ID_OutlineColor, oc);
                changed = true;
            }
            if(TextConfig.OutlineWidth.GetExprValue(float.Parse, out var ow)) {
                mat.SetFloat(ShaderUtilities.ID_OutlineWidth, ow);
                changed = true;
            }
            if(TextConfig.ShadowColor.GetExprValue(MiscUtils.ParseColor, out var sc)) {
                mat.SetColor(ShaderUtilities.ID_UnderlayColor, sc);
                changed = true;
            }
            if(TextConfig.ShadowOffset.GetExprValue(MiscUtils.ParseVec2, out var so)) {
                mat.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, so.x);
                mat.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, so.y);
                changed = true;
            }
            if(TextConfig.ShadowDilate.GetExprValue(float.Parse, out var sd)) {
                mat.SetFloat(ShaderUtilities.ID_UnderlayDilate, 1 - sd);
                changed = true;
            }
            if(TextConfig.ShadowSoftness.GetExprValue(float.Parse, out var ss)) {
                mat.SetFloat(ShaderUtilities.ID_UnderlaySoftness, 1 - ss);
                changed = true;
            }
        }

        if(changed) {
            Text.UpdateMeshPadding();
            Text.SetMaterialDirty();
        }
    }
    private void OnDestroy() {
        TagManager.OnLoadUnload -= RefreshTags;
        TextConfig.Release();
        if(_instancedMaterials != null) {
            foreach(var m in _instancedMaterials) {
                if(m) {
                    Destroy(m);
                }
            }
        }
    }
}
