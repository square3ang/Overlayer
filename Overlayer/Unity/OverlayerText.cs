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
using UnityEngine.UIElements;

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
    public Material[] RuntimeMaterials;

    private static bool isAlreadyDragging;
    private static int pointingCount = 0;
    private bool isDragging = false;
    private bool isPointing = false;
    private Vector2 initialObjectPosition;
    private Vector2 initialPointerLocal;
    
    public bool CanDrag => _config.Drag && !_config.Position.IsExpr;

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
        var shared = Text.fontSharedMaterials;
        RuntimeMaterials = new Material[shared.Length];
        for(int i = 0; i < shared.Length; i++) {
            var mat = new Material(shared[i]);
            InitMaterial(mat);
            ApplyMaterial(mat);
            RuntimeMaterials[i] = mat;
        }

        Text.fontSharedMaterials = RuntimeMaterials;
        config.Init();
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

        if(_config.LineSpacing.GetExprValue(float.Parse, out var ls)) {
            Text.lineSpacing = ls;
        }
        if(_config.LineSpacingAdj.GetExprValue(float.Parse, out var lsa)) {
            Text.lineSpacingAdjustment = lsa;
        }
        if(_config.TextColor.GetExprValue(ParseGColor, out var color)) {
            Text.colorGradient = color;
        }
        if(_config.Pivot.GetExprValue(ParseVec2, out var pivot)) {
            Text.rectTransform.pivot = pivot;
        }
        if(_config.Scale.GetExprValue(ParseVec2, out var scale)) {
            Text.rectTransform.localScale = scale;
        }
        if(_config.Position.GetExprValue(ParseVec2, out var pos)) {
            Text.rectTransform.anchoredPosition = (pos - new Vector2(0.5f, 0.5f)) * new Vector2(1920, 1080);
        }
        if(_config.Rotation.GetExprValue(ParseVec3, out var rot)) {
            Text.rectTransform.eulerAngles = rot;
        }
        if(_config.FontSize.GetExprValue(float.Parse, out var fs)) {
            Text.fontSize = fs;
        }
        for(int i = 0; i < RuntimeMaterials.Length; i++) {
            var mat = RuntimeMaterials[i];
            if(_config.OutlineColor.GetExprValue(ParseColor, out var oc)) {
                mat.SetColor(ShaderUtilities.ID_OutlineColor, oc);
            }
            if(_config.OutlineWidth.GetExprValue(float.Parse, out var ow)) {
                mat.SetFloat(ShaderUtilities.ID_OutlineWidth, ow);
            }
            if(_config.ShadowColor.GetExprValue(ParseColor, out var sc)) {
                mat.SetColor(ShaderUtilities.ID_UnderlayColor, sc);
            }
            if(_config.ShadowOffset.GetExprValue(ParseVec2,out var so)) {
                mat.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, so.x);
                mat.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, so.y);
            }
            if(_config.ShadowDilate.GetExprValue(float.Parse, out var sd)) {
                mat.SetFloat(ShaderUtilities.ID_UnderlayDilate, 1 - sd);
            }
            if(_config.ShadowSoftness.GetExprValue(float.Parse, out var ss)) {
                mat.SetFloat(ShaderUtilities.ID_UnderlaySoftness, 1 - ss);
            }
        }

        if(isDragging && OverlayerProfile.DragObj != null && OverlayerProfile.DragImage != null) {
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
        PlayingReplacer.Source = _config.PlayingText;
        NotPlayingReplacer.Source = _config.NotPlayingText;
        RefreshTags();
        TagManager.UpdatePatch();
        if(_config.LineSpacing.GetNormalValue(out var ls)) {
            Text.lineSpacing = ls;
        }
        if(_config.LineSpacingAdj.GetNormalValue(out var lsa)) {
            Text.lineSpacingAdjustment = lsa;
        }
        if(_config.TextColor.GetNormalValue(out var color)) {
            Text.colorGradient = color;
        }
        if(_config.Pivot.GetNormalValue(out var pivot)) {
            Text.rectTransform.pivot = pivot;
        }
        if(_config.Scale.GetNormalValue(out var scale)) {
            Text.rectTransform.localScale = scale;
        }
        if(_config.Position.GetNormalValue(out var pos)) {
            Text.rectTransform.anchoredPosition = (pos - new Vector2(0.5f, 0.5f)) * new Vector2(1920, 1080);
        }
        if(_config.Rotation.GetNormalValue(out var rot)) {
            Text.rectTransform.eulerAngles = rot;
        }
        if(_config.FontSize.GetNormalValue(out var fs)) {
            Text.fontSize = fs;
        }
        Text.alignment = _config.Alignment;

        SetFont();
        var shared = Text.fontSharedMaterials;
        for(int i = 0; i < RuntimeMaterials.Length; i++) {
            ApplyMaterial(RuntimeMaterials[i]);
        }
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
        if(_config.OutlineColor.GetNormalValue(out var oc)) {
            mat.SetColor(ShaderUtilities.ID_OutlineColor, oc);
        }
        if(_config.OutlineWidth.GetNormalValue(out var ow)) {
            mat.SetFloat(ShaderUtilities.ID_OutlineWidth, ow);
        }
        if(_config.ShadowColor.GetNormalValue(out var sc)) {
            mat.SetColor(ShaderUtilities.ID_UnderlayColor, sc);
        }
        if(_config.ShadowOffset.GetNormalValue(out var so)) {
            mat.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, so.x);
            mat.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, so.y);
        }
        if(_config.ShadowDilate.GetNormalValue(out var sd)) {
            mat.SetFloat(ShaderUtilities.ID_UnderlayDilate, 1 - sd);
        }
        if(_config.ShadowSoftness.GetNormalValue(out var ss)) {
            mat.SetFloat(ShaderUtilities.ID_UnderlaySoftness, 1 - ss);
        }
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

        Vector2 canvasSize = new Vector2(1920, 1080);
        _config.Position.Value = (Text.rectTransform.anchoredPosition / canvasSize) + new Vector2(0.5f, 0.5f);
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

    private void OnDestroy() {
        TagManager.OnLoadUnload -= RefreshTags;
        TextConfig.Release();
         if(RuntimeMaterials != null) {
            foreach(var mat in RuntimeMaterials) {
                Destroy(mat);
            }
        }
    }

    private static float NextFloat(string s, ref int idx) {
        int start = idx;

        while(idx < s.Length && s[idx] != ',') {
            idx++;
        }

        float v = float.Parse(s.Substring(start, idx - start), System.Globalization.CultureInfo.InvariantCulture);
        idx++;

        return v;
    }
    private static GColor ParseGColor(string s) {
        int idx = 0;

        return new GColor {
            topLeft = new Color(
                NextFloat(s, ref idx), NextFloat(s, ref idx),
                NextFloat(s, ref idx), NextFloat(s, ref idx)
            ),
            topRight = new Color(
                NextFloat(s, ref idx), NextFloat(s, ref idx),
                NextFloat(s, ref idx), NextFloat(s, ref idx)
            ),
            bottomLeft = new Color(
                NextFloat(s, ref idx), NextFloat(s, ref idx),
                NextFloat(s, ref idx), NextFloat(s, ref idx)
            ),
            bottomRight = new Color(
                NextFloat(s, ref idx), NextFloat(s, ref idx),
                NextFloat(s, ref idx), NextFloat(s, ref idx)
            ),
            gradientEnabled = true
        };
    }
    private static Vector2 ParseVec2(string s) {
        int idx = 0;

        return new Vector2(
            NextFloat(s, ref idx),
            float.Parse(s.Substring(idx), System.Globalization.CultureInfo.InvariantCulture)
        );
    }
    private static Vector3 ParseVec3(string s) {
        int idx = 0;

        float x = NextFloat(s, ref idx);
        float y = NextFloat(s, ref idx);
        float z = float.Parse(s.Substring(idx), System.Globalization.CultureInfo.InvariantCulture);

        return new Vector3(x, y, z);
    }
    private static Color ParseColor(string s) {
        int idx = 0;

        float r = NextFloat(s, ref idx);
        float g = NextFloat(s, ref idx);
        float b = NextFloat(s, ref idx);
        float a = float.Parse(s.Substring(idx), System.Globalization.CultureInfo.InvariantCulture);

        return new Color(r, g, b, a);
    }
    private static TextAlignmentOptions ParseAlign(string s) => (TextAlignmentOptions)int.Parse(s);
}
