/****************************************************************************
 * Copyright (c) 2024 liangxiegame UNDER MIT License
 * 
 * ResKit → YooAsset v3 适配层
 * 包装 YooAsset.AssetHandle / SceneHandle 实现 IRes 接口
 ****************************************************************************/

using System;
using System.Collections;
using UnityEngine;
using YooAsset;

namespace QFramework
{
    /// <summary>
    /// YooAsset 资源包装 — 将 YooAsset 的 AssetHandle 包装为 ResKit 的 IRes
    /// </summary>
    public class YooAssetRes : Res
    {
        private AssetHandle _handle;
        private SceneHandle _sceneHandle;
        private string _location;
        private bool _isScene;

        #region 对象池

        public static YooAssetRes Allocate(string location, Type assetType, bool isScene = false)
        {
            var res = SafeObjectPool<YooAssetRes>.Instance.Allocate();
            res.AssetName = location;
            res._location = location;
            res.AssetType = assetType;
            res._isScene = isScene;
            res._handle = null;
            res._sceneHandle = null;
            return res;
        }

        protected override void OnReleaseRes()
        {
            if (_handle != null)
            {
                _handle.Dispose();
                _handle = null;
            }

            if (_sceneHandle != null)
            {
                _sceneHandle = null; // SceneHandle 由 YooAsset 内部管理
            }

            mAsset = null;
        }

        public override void Recycle2Cache()
        {
            SafeObjectPool<YooAssetRes>.Instance.Recycle(this);
        }

        public override void OnRecycled()
        {
            base.OnRecycled();
            _handle = null;
            _sceneHandle = null;
            _location = null;
            _isScene = false;
        }

        #endregion

        #region 同步加载

        public override bool LoadSync()
        {
            if (!CheckLoadAble()) return false;

            State = ResState.Loading;

            var package = YooAssetBridge.DefaultPackage;
            if (package == null)
            {
                Debug.LogError("[YooAssetRes] DefaultPackage is null, call ResKit.Init() first!");
                OnResLoadFaild();
                return false;
            }

            try
            {
                _handle = package.LoadAssetSync(_location, AssetType);
                if (_handle == null || _handle.AssetObject == null)
                {
                    Debug.LogError($"[YooAssetRes] Failed to load asset: {_location}");
                    OnResLoadFaild();
                    return false;
                }

                mAsset = _handle.AssetObject;
                State = ResState.Ready;
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[YooAssetRes] LoadSync error: {_location}\n{e}");
                OnResLoadFaild();
                return false;
            }
        }

        #endregion

        #region 异步加载

        public override void LoadAsync()
        {
            if (!CheckLoadAble()) return;
            State = ResState.Loading;
            ResMgr.Instance.PushIEnumeratorTask(this);
        }

        public override IEnumerator DoLoadAsync(Action finishCallback)
        {
            if (RefCount <= 0)
            {
                OnResLoadFaild();
                finishCallback?.Invoke();
                yield break;
            }

            var package = YooAssetBridge.DefaultPackage;
            if (package == null)
            {
                Debug.LogError("[YooAssetRes] DefaultPackage is null, call ResKit.Init() first!");
                OnResLoadFaild();
                finishCallback?.Invoke();
                yield break;
            }

            var handle = package.LoadAssetAsync(_location, AssetType);
            _handle = handle;

            // 等待异步操作完成
            while (!handle.IsDone)
            {
                yield return null;
            }

            if (handle.Status != EOperationStatus.Succeeded || handle.AssetObject == null)
            {
                Debug.LogError($"[YooAssetRes] LoadAsync failed: {_location}, Error: {handle.Error}");
                OnResLoadFaild();
                finishCallback?.Invoke();
                yield break;
            }

            mAsset = handle.AssetObject;
            State = ResState.Ready;
            finishCallback?.Invoke();
        }

        #endregion

        #region 进度

        protected override float CalculateProgress()
        {
            if (_handle != null) return _handle.Progress;
            return 0;
        }

        #endregion

        #region 依赖（YooAsset 内部管理，返回空）

        public override string[] GetDependResList()
        {
            return null;
        }

        #endregion

        #region 卸载

        public override bool UnloadImage(bool flag)
        {
            return true;
        }

        #endregion

        public override string ToString()
        {
            return $"Type:YooAssetRes\t{base.ToString()}";
        }
    }
}