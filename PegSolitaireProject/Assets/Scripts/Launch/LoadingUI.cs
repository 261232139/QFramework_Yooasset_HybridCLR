/****************************************************************************
 * Copyright (c) 2024 liangxiegame UNDER MIT License
 *
 * LoadingUI — Loading界面控制器，由GameLauncher自动挂载到LaunchUI上
 *
 * 职责：
 *   1. 自动查找子物体绑定引用（ProgressImg / PercentTxt）
 *   2. 订阅 LaunchContext.OnProgressChanged 驱动进度条
 *   3. ProgressImg初始X = -width，完成时X = 0
 *   4. 提供Hide方法供进入大厅时调用
 ****************************************************************************/

using UnityEngine;
using TMPro;

namespace Launch
{
    public class LoadingUI : MonoBehaviour
    {
        private RectTransform mProgressImg;
        private TextMeshProUGUI mPercentTxt;

        private float mProgressWidth;
        private float mProgressStartX;

        private static LoadingUI sInstance;
        public static LoadingUI Instance => sInstance;

        private void Awake()
        {
            sInstance = this;
            AutoBindReferences();
            ResetProgress();
        }

        private void AutoBindReferences()
        {
            // 查找 ProgressObj/Progress/Mask/ProgressImg
            var progressObj = transform.Find("ProgressObj");
            if (progressObj == null)
            {
                Debug.LogError("[LoadingUI] 找不到 ProgressObj");
                return;
            }

            var progressImgTrans = progressObj.Find("Progress/Mask/ProgressImg");
            if (progressImgTrans != null)
            {
                mProgressImg = progressImgTrans as RectTransform;
            }
            else
            {
                Debug.LogError("[LoadingUI] 找不到 ProgressObj/Progress/Mask/ProgressImg");
            }

            // 查找 ProgressObj/PercentTxt
            var percentTxtTrans = progressObj.Find("PercentTxt");
            if (percentTxtTrans != null)
            {
                mPercentTxt = percentTxtTrans.GetComponent<TextMeshProUGUI>();
            }
            else
            {
                Debug.LogError("[LoadingUI] 找不到 ProgressObj/PercentTxt");
            }

            // 计算进度条宽度
            if (mProgressImg != null)
            {
                mProgressWidth = mProgressImg.rect.width;
                if (mProgressWidth <= 0f)
                {
                    // rect可能还没layout，用sizeDelta兜底
                    mProgressWidth = mProgressImg.sizeDelta.x;
                }
                mProgressStartX = -mProgressWidth;
            }
        }

        private void ResetProgress()
        {
            if (mProgressImg != null)
            {
                var pos = mProgressImg.anchoredPosition;
                pos.x = mProgressStartX;
                mProgressImg.anchoredPosition = pos;
            }

            if (mPercentTxt != null)
            {
                mPercentTxt.text = "0%";
            }
        }

        private void OnEnable()
        {
            LaunchContext.Instance.OnProgressChanged += OnProgressChanged;
        }

        private void OnDisable()
        {
            if (LaunchContext.Instance != null)
                LaunchContext.Instance.OnProgressChanged -= OnProgressChanged;
        }

        private void OnProgressChanged(float progress)
        {
            progress = Mathf.Clamp01(progress);

            if (mProgressImg != null)
            {
                var pos = mProgressImg.anchoredPosition;
                pos.x = Mathf.Lerp(mProgressStartX, 0f, progress);
                mProgressImg.anchoredPosition = pos;
            }

            if (mPercentTxt != null)
            {
                mPercentTxt.text = $"{Mathf.RoundToInt(progress * 100)}%";
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            Debug.Log("[LoadingUI] 已隐藏");
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        private void OnDestroy()
        {
            if (sInstance == this)
                sInstance = null;
        }
    }
}
