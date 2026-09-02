using System;
using UnityEngine;

namespace CustomAssets.App.Game.App.MonoBehaviour.UiAdapter
{
    public class SafeAreaUiAdjuster : UnityEngine.MonoBehaviour
    {
        #region Fields

        #region Public

        public Func<int> GetBottomBannerAdHeightPixelsFunc;

        #endregion

        #region Private

        [SerializeField] private bool dontDestroyAfterLateUpdate;

        [SerializeField] private int deltaBottomYForIOS;

        private int _bottomBannerAdHeightPixels;

        private int? _lastBottomHeightPixels;

        #endregion

        #endregion

        #region Properties

#if !UNITY_IOS
        private int BottomHeightPixels => _bottomBannerAdHeightPixels;
#else
        private int BottomHeightPixels => _bottomBannerAdHeightPixels + deltaBottomYForIOS;
#endif

        #endregion

        #region Methods

        private void LateUpdate()
        {
            _bottomBannerAdHeightPixels = GetBottomBannerAdHeightPixelsFunc?.Invoke() ?? 0;
            if (BottomHeightPixels == _lastBottomHeightPixels)
            {
                return;
            }

            AdjustInner();
            if (!dontDestroyAfterLateUpdate)
            {
                Destroy(this);
            }
        }

        private void AdjustInner()
        {
            _lastBottomHeightPixels = BottomHeightPixels;
            var canvas = GetComponentInParent<Canvas>();
            var pixelRect = canvas.worldCamera.pixelRect;
            var safeArea = Screen.safeArea;
            safeArea.height -= BottomHeightPixels;
            safeArea.height += safeArea.y;
            safeArea.y = BottomHeightPixels;
            var originalSafeAreaWidth = safeArea.width;
            var sizeDelta = canvas.GetComponent<RectTransform>().sizeDelta;
            var rate = sizeDelta.x / pixelRect.width * safeArea.width / originalSafeAreaWidth;
            var width = pixelRect.width * rate;
            var height = width * safeArea.height / safeArea.width;
            var rectTransform = GetComponent<RectTransform>();
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            rectTransform.localPosition = new Vector3(0,
                (safeArea.y - (pixelRect.height - safeArea.height) * 0.5f) * rate, 0);
        }

        #endregion
    }
}