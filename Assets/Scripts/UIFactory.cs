using UnityEngine;
using UnityEngine.UI;

public static class UIFactory
{
    public static void EnsureCanvas(GameObject go, int sortingOrder)
    {
        var canvas = go.GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = go.AddComponent<Canvas>();
        }
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;

        if (go.GetComponent<CanvasScaler>() == null)
        {
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
        }

        if (go.GetComponent<GraphicRaycaster>() == null)
        {
            go.AddComponent<GraphicRaycaster>();
        }
    }

    public static RectTransform CreatePanel(Transform parent, string name, Vector2 anchor, Vector2 anchoredPos, TextAnchor childAlignment)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        go.transform.SetParent(parent, false);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = anchor;
        rt.anchoredPosition = anchoredPos;

        var layout = go.GetComponent<VerticalLayoutGroup>();
        layout.childAlignment = childAlignment;
        layout.spacing = 6f;
        layout.padding = new RectOffset(14, 14, 12, 12);
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        var fitter = go.GetComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

        return rt;
    }

    public static Text CreateText(Transform parent, string content, int fontSize, TextAnchor alignment, Vector2? size = null)
    {
        var go = new GameObject("Label", typeof(RectTransform));
        go.transform.SetParent(parent, false);

        var text = go.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = Color.white;
        text.text = content;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;

        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = size ?? new Vector2(200f, fontSize + 8f);

        return text;
    }

    public static Slider CreateSlider(Transform parent, Vector2 size, Color fillColor)
    {
        var go = new GameObject("Bar", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        go.GetComponent<RectTransform>().sizeDelta = size;

        var slider = go.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 100f;
        slider.value = 100f;
        slider.interactable = false;
        slider.transition = Selectable.Transition.None;

        var bgGo = new GameObject("Background", typeof(RectTransform));
        bgGo.transform.SetParent(go.transform, false);
        bgGo.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.5f);
        StretchFull(bgGo.GetComponent<RectTransform>());

        var fillAreaGo = new GameObject("Fill Area", typeof(RectTransform));
        fillAreaGo.transform.SetParent(go.transform, false);
        StretchFull(fillAreaGo.GetComponent<RectTransform>());

        var fillGo = new GameObject("Fill", typeof(RectTransform));
        fillGo.transform.SetParent(fillAreaGo.transform, false);
        var fillImage = fillGo.AddComponent<Image>();
        fillImage.color = fillColor;
        StretchFull(fillGo.GetComponent<RectTransform>());

        slider.fillRect = fillGo.GetComponent<RectTransform>();
        slider.targetGraphic = fillImage;
        slider.direction = Slider.Direction.LeftToRight;

        return slider;
    }

    public static RectTransform CreateHBox(Transform parent, string name, float spacing)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter));
        go.transform.SetParent(parent, false);

        var layout = go.GetComponent<HorizontalLayoutGroup>();
        layout.spacing = spacing;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        var fitter = go.GetComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

        return go.GetComponent<RectTransform>();
    }

    public static Button CreateButton(Transform parent, string label, Vector2 size, System.Action onClick)
    {
        var go = new GameObject(label + " Button", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        go.GetComponent<RectTransform>().sizeDelta = size;

        var image = go.AddComponent<Image>();
        image.color = new Color(0.18f, 0.18f, 0.22f, 0.95f);

        var button = go.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(() => onClick());

        var text = CreateText(go.transform, label, 14, TextAnchor.MiddleCenter, size);
        StretchFull(text.GetComponent<RectTransform>());

        return button;
    }

    public static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}
