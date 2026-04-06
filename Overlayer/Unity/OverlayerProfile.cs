using Overlayer.Core;
using Overlayer.Models;
using UnityEngine;
using UnityEngine.UI;

namespace Overlayer.Unity;

public class OverlayerProfile : MonoBehaviour {
    public Canvas ProfileCanvas;
    public CanvasGroup Group;
    public ProfileConfig Config;
    public ObjectManager ObjectManager;

    public static GameObject PCanvasObj;
    public static Canvas PublicCanvas;
    public static GameObject DragObj;
    public static Image DragImage;

    public void Init(string name) {
        PublicCanvasInit();

        gameObject.transform.SetParent(PublicCanvas.transform, false);

        ProfileCanvas = gameObject.GetComponent<Canvas>() ?? gameObject.AddComponent<Canvas>();
        ProfileCanvas.renderMode = PublicCanvas.renderMode;
        ProfileCanvas.worldCamera = PublicCanvas.worldCamera;

        Group = gameObject.GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
        Group.alpha = Config.Opacity;
        Group.interactable = true;
        Group.blocksRaycasts = true;

        RectTransform rt = gameObject.GetComponent<RectTransform>() ?? gameObject.AddComponent<RectTransform>();
        RectTransform publicRt = PublicCanvas.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = Vector2.zero;
        rt.localScale = Vector3.one;

        gameObject.AddComponent<GraphicRaycaster>();

        DragInit();

        ObjectManager = new ObjectManager(this);
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
        DontDestroyOnLoad(PublicCanvas);
    }

    public static void DragInit() {
        if(DragObj) {
            return;
        }
        DragObj = new GameObject("Drag Outline");
        DragObj.transform.SetParent(PublicCanvas.transform);
        DragObj.transform.localPosition = Vector3.zero;
        DragImage = DragObj.AddComponent<Image>();

        Texture2D outlinetex = new(3, 3, TextureFormat.RGBA32, false);
        Color[] outlinetexpixels =
        [
            Color.white, Color.white, Color.white,
            Color.white, Color.clear, Color.white,
            Color.white, Color.white, Color.white
        ];
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

    public void ApplyConfig() => Group.alpha = Config.Opacity;
}
