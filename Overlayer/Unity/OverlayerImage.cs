using Overlayer.Core;
using Overlayer.Core.TextReplacing;
using Overlayer.Models;
using Overlayer.Tags;
using Overlayer.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Overlayer.Unity;

public class OverlayerImage : OverlayerObject, IPointerDownHandler, IPointerUpHandler, IDragHandler, IPointerEnterHandler, IPointerExitHandler {
    public bool Initialized { get; private set; }
    private ImageConfig _config;
    public override ObjectConfig Config => _config;
    public ImageConfig ImageConfig => _config;

    public List<Sprite> Images = new();
    private Image _mainImage;

    public Replacer PlayingReplacer;
    public Replacer NotPlayingReplacer;

    private static bool isAlreadyDragging;
    private static int pointingCount = 0;
    private bool isDragging = false;
    private bool isPointing = false;
    private Vector2 initialObjectPosition;
    private Vector2 initialPointerLocal;
    private Vector2 scaleforDrag = Vector2.zero;

    public bool CanDrag => _config.Drag && !_config.Position.IsExpr;

    public void Init(OverlayerProfile profile, ImageConfig config) {
        if(Initialized) {
            return;
        }

        Parent = profile;
        _config = config;

        DontDestroyOnLoad(gameObject);
        GameObject mainObject = gameObject;
        mainObject.transform.SetParent(Parent.ProfileCanvas.transform);
        mainObject.MakeFlexible();

        _mainImage = mainObject.AddComponent<Image>();
        _mainImage.raycastTarget = _config.Drag;

        config.OnDragChanged += (state) => _mainImage.raycastTarget = state;

        PlayingReplacer = new Replacer(_config.PlayingCommand, TagManager.All.Select(t => t.Tag));
        NotPlayingReplacer = new Replacer(_config.NotPlayingCommand, TagManager.NP.Select(t => t.Tag));
        PlayingReplacer.Compile();
        NotPlayingReplacer.Compile();

        config.Init();
        ApplyConfig();
        _mainImage.raycastTarget = _config.Drag;
        _mainImage.rectTransform.anchorMin = Vector2.zero;
        _mainImage.rectTransform.anchorMax = Vector2.one;
        ApplyImages();
        Initialized = true;
    }

    public void Update() {
        if(!Initialized || _mainImage == null || Images.Count == 0) {
            return;
        }

        string rawCommand = Main.IsPlaying ? PlayingReplacer?.Replace() ?? "" : NotPlayingReplacer?.Replace() ?? "";

        int idx;
        if(int.TryParse(rawCommand, out idx) && idx >= 0 && idx < Images.Count) {
            _mainImage.sprite = Images[idx];
        } else {
            _mainImage.sprite = ImageManager.DefaultSprite;
        }

        if(_config.Color.GetExprValue(MiscUtils.ParseColor, out var color)) {
            _mainImage.color = color;
        }
        if(_config.Pivot.GetExprValue(MiscUtils.ParseVec2, out var pivot)) {
            _mainImage.rectTransform.pivot = pivot;
        }
        if(_config.Position.GetExprValue(MiscUtils.ParseVec2, out var pos)) {
            _mainImage.rectTransform.anchoredPosition = (pos - new Vector2(0.5f, 0.5f)) * new Vector2(1920, 1080);
        }
        if(_config.Scale.GetExprValue(MiscUtils.ParseVec2, out var scale)) {
            _mainImage.rectTransform.localScale = scale;
            scaleforDrag = scale;
        }
        if(_config.Rotation.GetExprValue(MiscUtils.ParseVec3, out var rot)) {
            _mainImage.rectTransform.rotation = Quaternion.Euler(rot);
        }

        if(isDragging && OverlayerProfile.DragObj != null && OverlayerProfile.DragImage != null) {
            OverlayerProfile.DragObj.transform.position = _mainImage.transform.position;
            OverlayerProfile.DragObj.transform.rotation = _mainImage.transform.rotation;
            OverlayerProfile.DragImage.rectTransform.pivot = _mainImage.rectTransform.pivot;
            OverlayerProfile.DragImage.rectTransform.sizeDelta = new Vector2(_mainImage.preferredWidth, _mainImage.preferredHeight) * scaleforDrag;
        }
    }
    private void RefreshTags() {
        PlayingReplacer.UpdateTags(TagManager.All.Select(ot => ot.Tag));
        NotPlayingReplacer.UpdateTags(TagManager.NP.Select(ot => ot.Tag));
        PlayingReplacer.Compile();
        NotPlayingReplacer.Compile();
    }
    public override void ApplyConfig() {
        PlayingReplacer.Source = _config.PlayingCommand;
        NotPlayingReplacer.Source = _config.NotPlayingCommand;
        PlayingReplacer.UpdateTags(TagManager.All.Select(ot => ot.Tag));
        NotPlayingReplacer.UpdateTags(TagManager.NP.Select(ot => ot.Tag));
        PlayingReplacer.Compile();
        NotPlayingReplacer.Compile();
        TagManager.UpdatePatch();
        if(_config.Color.GetNormalValue(out var color)) {
            _mainImage.color = color;
        }
        if(_config.Pivot.GetNormalValue(out var pivot)) {
            _mainImage.rectTransform.pivot = pivot;
        }
        if(_config.Position.GetNormalValue(out var pos)) {
            _mainImage.rectTransform.anchoredPosition = (pos - new Vector2(0.5f, 0.5f)) * new Vector2(1920, 1080);
        }
        if(_config.Scale.GetNormalValue(out var scale)) {
            _mainImage.rectTransform.localScale = scale;
            scaleforDrag = scale;
        }
        if(_config.Rotation.GetNormalValue(out var rot)) {
            _mainImage.rectTransform.rotation = Quaternion.Euler(rot);
        }
        _mainImage.gameObject.SetActive(_config.Active);
    }

    public void ApplyImages() {
        Images.Clear();
        _mainImage.sprite = ImageManager.DefaultSprite;
        for(int i = 0; i < _config.Images.Count; i++) {
            string imagePath = _config.Images[i];
            Images.Add(ImageManager.GetSpriteSafe(imagePath));
        }
    }

    public void OnPointerDown(PointerEventData e) {
        if(!CanDrag || isAlreadyDragging) {
            return;
        }
        isDragging = true;
        isAlreadyDragging = true;
        RectTransform parentRect = _mainImage.rectTransform.parent as RectTransform;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, e.position, e.pressEventCamera, out initialPointerLocal);
        initialObjectPosition = _mainImage.rectTransform.anchoredPosition;
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
        RectTransform parentRect = _mainImage.rectTransform.parent as RectTransform;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, e.position, e.pressEventCamera, out Vector2 currentPointerLocal);
        Vector2 offset = currentPointerLocal - initialPointerLocal;
        _mainImage.rectTransform.anchoredPosition = initialObjectPosition + offset;
        Vector2 canvasSize = new(1920, 1080);
        _config.Position.Value = (_mainImage.rectTransform.anchoredPosition / canvasSize) + new Vector2(0.5f, 0.5f);
    }

    public void OnPointerEnter(PointerEventData e) {
        if(!CanDrag) {
            return;
        }
        isPointing = true;
        pointingCount++;
        if(!isAlreadyDragging) {
            OverlayerProfile.DragObj.transform.position = _mainImage.transform.position;
            OverlayerProfile.DragObj.transform.rotation = _mainImage.transform.rotation;
            OverlayerProfile.DragImage.rectTransform.pivot = _mainImage.rectTransform.pivot;
            OverlayerProfile.DragImage.rectTransform.sizeDelta = new Vector2(_mainImage.preferredWidth, _mainImage.preferredHeight) * scaleforDrag;
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
            if(!isAlreadyDragging && OverlayerProfile.DragObj != null) {
                OverlayerProfile.DragObj.SetActive(false);
            }
        }
        isPointing = false;
    }

    private void OnDestroy() {
        TagManager.OnLoadUnload -= RefreshTags;
    }
}