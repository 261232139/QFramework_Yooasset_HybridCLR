using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class FullScreenAdapter : MonoBehaviour
{
    //[Tooltip("如果为 true，会在检测到 SafeArea 时把内容做 Y 方向微调，默认 true")]
    //public bool useSafeAreaOffset = true;

    private Canvas rootCanvas;

    private void Awake()
    {
        rootCanvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        //canvasScaler = rootCanvas?.GetComponent<CanvasScaler>();
    }

    private void Start()
    {
        SetScaleCover();
    }

    /// <summary>
    /// 等比 Cover（覆盖屏幕），仅修改 localScale 与 anchoredPosition（位置微调），不改 sizeDelta
    /// 计算依据：当前对象的 rect.size（UI 单位） -> 换算像素 -> 对比 Screen -> 求 scale
    /// </summary>
    public void SetScaleCover()
    {
        RectTransform rectTransform = transform as RectTransform;

        Vector3 worldPosition = rootCanvas.transform.TransformPoint(Vector3.zero);
        Vector3 localPosition = rectTransform.parent.InverseTransformPoint(worldPosition);
        rectTransform.localPosition = new Vector3(0, localPosition.y, 0);

        Vector2 size = rectTransform.sizeDelta;
        float scaleX = Screen.width / (size.x * rectTransform.localScale.x * rootCanvas.scaleFactor);
        float scaleY = Screen.height / (size.y * rectTransform.localScale.y * rootCanvas.scaleFactor);
        float scale = Mathf.Max(scaleX, scaleY);
        rectTransform.sizeDelta *= scale;


        //if (rectTransform == null) return;

        //// 取对象在 UI 单位下的本地尺寸（rect 而不是 sizeDelta 更可靠）
        //Vector2 uiSize = rectTransform.rect.size;
        //if (uiSize.x <= 0f || uiSize.y <= 0f)
        //{
        //    // 退回到 sizeDelta 或 CanvasScaler 的参考分辨率
        //    uiSize = rectTransform.sizeDelta;
        //    if ((uiSize.x <= 0f || uiSize.y <= 0f) && canvasScaler != null)
        //        uiSize = canvasScaler.referenceResolution;
        //    if (uiSize.x <= 0f || uiSize.y <= 0f) // 最后兜底
        //        uiSize = new Vector2(Screen.width, Screen.height);
        //}

        //// 根 Canvas 的 scaleFactor（把 UI 单位转换为像素）
        //float rootScaleFactor = 1f;
        //if (rootCanvas != null)
        //    rootScaleFactor = rootCanvas.scaleFactor;
        //else if (canvasScaler != null)
        //    rootScaleFactor = Screen.height / Mathf.Max(canvasScaler.referenceResolution.y, 1f);

        //// 计算所需 finalScale，使得 (uiSize * rootScaleFactor * finalScale) 覆盖屏幕像素
        //float denomX = Mathf.Max(uiSize.x * rootScaleFactor, 0.0001f);
        //float denomY = Mathf.Max(uiSize.y * rootScaleFactor, 0.0001f);

        //float scaleX = (float)Screen.width / denomX;
        //float scaleY = (float)Screen.height / denomY;

        //float finalScale = Mathf.Max(scaleX, scaleY);

        //// 应用等比缩放（只改 localScale）
        //rectTransform.localScale = Vector3.one * finalScale;

        //// 保持中心对齐，避免缩放偏移
        //rectTransform.anchorMin = rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        //rectTransform.pivot = new Vector2(0.5f, 0.5f);

        //// SafeArea 微调（不改变 scale，只调整 anchoredPosition，使视觉居中）
        //if (useSafeAreaOffset && HasNotch())
        //{
        //    Rect safe = Screen.safeArea;
        //    float topOffset = Screen.height - (safe.y + safe.height);
        //    float bottomOffset = safe.y;

        //    float pixelOffsetY = (topOffset - bottomOffset) * 0.5f;

        //    // 将像素偏移转换为 UI 单位偏移：
        //    // uiOffset = pixelOffset / (rootScaleFactor * finalScale)
        //    float denom = Mathf.Max(rootScaleFactor * finalScale, 0.0001f);
        //    float uiOffsetY = pixelOffsetY / denom;

        //    rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, uiOffsetY);
        //}
        //else
        //{
        //    rectTransform.anchoredPosition = Vector2.zero;
        //}
    }

    private bool HasNotch()
    {
        Rect safe = Screen.safeArea;
        return safe.y > 0f || safe.height < Screen.height;
    }
}
