/****************************************************************************
 * Copyright (c) 2017 snowcold
 * Copyright (c) 2017 ~ 2023 liangxie
 * 
 * https://qframework.cn
 * https://github.com/liangxiegame/QFramework
 * https://gitee.com/liangxiegame/QFramework
 *
 * Refactored 2024 — 底层替换为 YooAsset v3
 ****************************************************************************/

using System.Collections;
using System.Linq;

namespace QFramework
{
    using System.Collections.Generic;
    using UnityEngine;

    [MonoSingletonPath("QFramework/ResKit/ResManager")]
    public class ResMgr : MonoBehaviour,ISingleton
    {
        public static ResMgr Instance => MonoSingletonProperty<ResMgr>.Instance;

        #region 初始化

        private static bool mResMgrInited = false;

        public static bool ResMgrInited => mResMgrInited;

        /// <summary>
        /// 初始化 ResKit + YooAsset
        /// </summary>
        public static void Init()
        {
            if (mResMgrInited) return;
            mResMgrInited = true;

            SafeObjectPool<YooAssetRes>.Instance.Init(40, 20);
            SafeObjectPool<ResSearchKeys>.Instance.Init(40, 20);
            SafeObjectPool<ResLoader>.Instance.Init(40, 20);

            Instance.InitResMgr();
        }

        public static IEnumerator InitAsync()
        {
            if (mResMgrInited) yield break;
            mResMgrInited = true;

            SafeObjectPool<YooAssetRes>.Instance.Init(40, 20);
            SafeObjectPool<ResSearchKeys>.Instance.Init(40, 20);
            SafeObjectPool<ResLoader>.Instance.Init(40, 20);

            yield return Instance.InitResMgrAsync();
        }

        #endregion

        public int Count
        {
            get { return mTable.Count(); }
        }

        public static bool IsApplicationQuit { get;private set; }

        private void OnApplicationQuit()
        {
            IsApplicationQuit = true;
        }

        #region 字段

        private ResTable mTable = new ResTable();

        private bool mIsResMapDirty;

        #endregion

        public void InitResMgr()
        {
            // 初始化 YooAsset 桥接器（同步）
            YooAssetBridge.Initialize();
        }

        public IEnumerator InitResMgrAsync()
        {
            // 初始化 YooAsset 桥接器（异步）
            yield return YooAssetBridge.InitializeAsync();
        }

        #region 属性

        public void ClearOnUpdate()
        {
            mIsResMapDirty = true;
        }

        public void PushIEnumeratorTask(IEnumeratorTask task)
        {
            Instance.StartCoroutine(task.DoLoadAsync(() => { }));
        }

        public IRes GetRes(ResSearchKeys resSearchKeys, bool createNew = false)
        {
            var res = mTable.GetResBySearchKeys(resSearchKeys);

            if (res != null)
            {
                return res;
            }

            if (!createNew)
            {
                return null;
            }

            res = ResFactory.Create(resSearchKeys);

            if (res != null)
            {
                mTable.Add(res);
            }

            return res;
        }

        public T GetRes<T>(ResSearchKeys resSearchKeys) where T : class, IRes
        {
            return GetRes(resSearchKeys) as T;
        }

        #endregion

        #region Private Func

        private void Update()
        {
            if (mIsResMapDirty)
            {
                RemoveUnusedRes();
            }
        }

        private void RemoveUnusedRes()
        {
            if (!mIsResMapDirty)
            {
                return;
            }

            mIsResMapDirty = false;

            foreach (var res in mTable.ToArray())
            {
                if (res.RefCount <= 0 && res.State != ResState.Loading)
                {
                    if (res.ReleaseRes())
                    {
                        mTable.Remove(res);
                        res.Recycle2Cache();
                    }
                }
            }
        }

        private void OnGUI()
        {
            if (PlatformCheck.IsEditor && Input.GetKey(KeyCode.F1))
            {
                GUILayout.BeginVertical("box");

                GUILayout.Label("ResKit (YooAsset v3)", new GUIStyle {fontSize = 30});
                GUILayout.Space(10);
                GUILayout.Label("ResInfo", new GUIStyle {fontSize = 20});
                mTable.ToList().ForEach(res => { GUILayout.Label((res as Res).ToString()); });
                GUILayout.Space(10);

                GUILayout.Label("Pools", new GUIStyle() {fontSize = 20});
                GUILayout.Label(string.Format("ResSearchRule:{0}",
                    SafeObjectPool<ResSearchKeys>.Instance.CurCount));
                GUILayout.Label(string.Format("ResLoader:{0}",
                    SafeObjectPool<ResLoader>.Instance.CurCount));
                GUILayout.EndVertical();
            }
        }

        #endregion

        public void OnSingletonInit()
        {
        }
    }
}