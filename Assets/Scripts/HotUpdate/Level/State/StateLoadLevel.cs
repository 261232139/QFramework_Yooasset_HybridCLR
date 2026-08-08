/****************************************************************************
 * Copyright (c) 2024 Game Project UNDER MIT License
 *
 * 加载关卡状态
 *
 * 加载关卡配置、资源等
 ****************************************************************************/

using UnityEngine;
using QFramework;
using Game.Level.Runtime;
using System.Collections;

namespace Game.Level.State
{
    /// <summary>
    /// 加载关卡状态
    /// 加载关卡配置、资源等
    /// </summary>
    internal class StateLoadLevel : LevelStateBase
    {
        public StateLoadLevel(FSM<LevelState> fsm, LevelStateMachine target) : base(fsm, target) { }

        protected override void OnEnter()
        {
            Debug.Log($"[LevelState] LoadLevel - Loading level {Context.LevelNumber}");
            LevelEventManager.TriggerEvent(LevelEventType.LevelLoadStart, Context.LevelNumber);

            if (Context.Config == null)
            {
                Debug.LogError("[LevelState] Cannot load level without config!");
                mFSM.ChangeState(LevelState.LevelFail);
                return;
            }

            if (Context.CoroutineHost != null)
                Context.CoroutineHost.StartCoroutine(LoadLevelAsync());
            else
                LoadLevelSync();
        }

        private IEnumerator LoadLevelAsync()
        {
            // 动态加载 LevelScene 预制体
            var package = YooAssetBridge.DefaultPackage;
            if (package == null)
            {
                Debug.LogError("[StateLoadLevel] YooAsset package not initialized!");
                mFSM.ChangeState(LevelState.LevelFail);
                yield break;
            }

            // 加载 LevelScene 预制体
            var levelScenePrefabName = "LevelScene";
            Debug.Log($"[StateLoadLevel] Loading LevelScene prefab: {levelScenePrefabName}");
            
            var handle = package.LoadAssetAsync<GameObject>(levelScenePrefabName);
            yield return handle;

            if (handle.Status != YooAsset.EOperationStatus.Succeeded || handle.AssetObject == null)
            {
                Debug.LogError($"[StateLoadLevel] Failed to load LevelScene prefab");
                handle.Dispose();
                mFSM.ChangeState(LevelState.LevelFail);
                yield break;
            }

            // 实例化 LevelScene 到 UIRoot 的 Level 节点下
            var levelScenePrefab = handle.AssetObject as GameObject;
            
            // 获取 UIRoot 的 Level 节点
            var uiRoot = UIRoot.Instance;
            if (uiRoot == null || uiRoot.Level == null)
            {
                Debug.LogError("[StateLoadLevel] UIRoot or UIRoot.Level not found!");
                handle.Dispose();
                mFSM.ChangeState(LevelState.LevelFail);
                yield break;
            }
            
            var levelSceneObj = Object.Instantiate(levelScenePrefab, uiRoot.Level);
            levelSceneObj.name = "LevelScene";
            
            // 设置 RectTransform 为全屏
            var rectTransform = levelSceneObj.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;
                rectTransform.localScale = Vector3.one;
                rectTransform.localPosition = Vector3.zero;
            }
            
            handle.Dispose();

            // 优先查找 LevelView（新架构）
            var levelView = levelSceneObj.GetComponent<LevelView>();
            if (levelView != null)
            {
                Debug.Log("[StateLoadLevel] Using LevelView (new architecture)");
                
                // 使用 LevelView 加载关卡
                levelView.LoadLevel(Context.Config, Context.LevelNumber);

                // 监听关卡事件
                levelView.OnLevelCompleted += HandleLevelCompleted;
                levelView.OnLevelFailed += HandleLevelFailed;
            }
            else
            {
                // LevelView not found - cannot proceed
                Debug.LogError("[StateLoadLevel] LevelView component not found on loaded prefab!");
                Object.Destroy(levelSceneObj);
                mFSM.ChangeState(LevelState.LevelFail);
                yield break;
            }

            // 等待一帧确保所有对象初始化完成
            yield return null;

            LevelEventManager.TriggerEvent(LevelEventType.LevelLoadComplete, Context.LevelNumber);
            mFSM.ChangeState(LevelState.LevelReady);
        }

        private void LoadLevelSync()
        {
            Debug.LogError("[StateLoadLevel] Sync loading not supported for LevelScene. Use async loading.");
            mFSM.ChangeState(LevelState.LevelFail);
        }

        private void HandleLevelCompleted()
        {
            Debug.Log("[StateLoadLevel] Level completed event received");
            mFSM.ChangeState(LevelState.LevelSuccess);
        }

        private void HandleLevelFailed()
        {
            Debug.Log("[StateLoadLevel] Level failed event received");
            mFSM.ChangeState(LevelState.LevelFail);
        }
    }
}
