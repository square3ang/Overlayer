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

        Initialized = true;

        ApplyConfig();
        ApplyImages();
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
            _mainImage.sprite = null;
        }

        if(isDragging && OverlayerProfile.DragObj != null && OverlayerProfile.DragImage != null) {
            OverlayerProfile.DragObj.transform.position = _mainImage.gameObject.transform.position;
            OverlayerProfile.DragObj.transform.rotation = _mainImage.gameObject.transform.rotation;
            OverlayerProfile.DragImage.rectTransform.pivot = _mainImage.rectTransform.pivot;
            OverlayerProfile.DragImage.rectTransform.sizeDelta = new Vector2(
                _mainImage.rectTransform.rect.width, _mainImage.rectTransform.rect.height
            );
        }
    }

    public override void ApplyConfig() {
        PlayingReplacer.Source = _config.PlayingCommand;
        NotPlayingReplacer.Source = _config.NotPlayingCommand;
        PlayingReplacer.UpdateTags(TagManager.All.Select(ot => ot.Tag));
        NotPlayingReplacer.UpdateTags(TagManager.NP.Select(ot => ot.Tag));
        PlayingReplacer.Compile();
        NotPlayingReplacer.Compile();
        TagManager.UpdatePatch();
        _mainImage.color = _config.Color;
        _mainImage.raycastTarget = _config.Drag;
        var rt = _mainImage.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = _config.Pivot;
        rt.anchoredPosition = (_config.Position - new Vector2(0.5f, 0.5f)) * new Vector2(1920, 1080);
        rt.localScale = _config.Scale;
        rt.rotation = Quaternion.Euler(_config.Rotation);
        _mainImage.gameObject.SetActive(_config.Active);
    }

    public void ApplyImages() {
        Images.Clear();
        _mainImage.sprite = null;
        for(int i = 0; i < _config.Images.Count; i++) {
            string imagePath = _config.Images[i];
            if(imagePath != null) {
                Images.Add(ImageManager.GetSpriteSafe(imagePath));
            } else {
                Images.Add(null);
            }
        }
    }

    public void OnPointerDown(PointerEventData e) {
        if(isAlreadyDragging) {
            return;
        }
        isDragging = true;
        isAlreadyDragging = true;
        RectTransform parentRect = _mainImage.rectTransform.parent as RectTransform;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, e.position, e.pressEventCamera, out initialPointerLocal);
        initialObjectPosition = _mainImage.rectTransform.anchoredPosition;
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
        RectTransform parentRect = _mainImage.rectTransform.parent as RectTransform;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, e.position, e.pressEventCamera, out Vector2 currentPointerLocal);
        Vector2 offset = currentPointerLocal - initialPointerLocal;
        _mainImage.rectTransform.anchoredPosition = initialObjectPosition + offset;
        Vector2 canvasSize = new(1920, 1080);
        _config.Position = (_mainImage.rectTransform.anchoredPosition / canvasSize) + new Vector2(0.5f, 0.5f);
    }

    public void OnPointerEnter(PointerEventData e) {
        isPointing = true;
        pointingCount++;
        if(!isAlreadyDragging && OverlayerProfile.DragObj != null) {
            OverlayerProfile.DragObj.SetActive(true);
        }
    }
    public void OnPointerExit(PointerEventData e) {
        pointingCount--;
        if(pointingCount <= 0) {
            pointingCount = 0;
            if(!isAlreadyDragging && OverlayerProfile.DragObj != null) {
                OverlayerProfile.DragObj.SetActive(false);
            }
        }
        isPointing = false;
    }
}