/****************************************************************************
 * SaveManager — 本地存储管理器
 * 
 * 职责：管理所有数据控制器的存储操作
 * 支持即时存储和延时存储两种模式
 * 使用 Singleton<T> 实现单例，通过 UpdateProxy 驱动 Update
 ****************************************************************************/

using System.Collections.Generic;
using UnityEngine;
using QFramework;

namespace HotUpdate.LocalStorageKit
{
    public class SaveManager : Singleton<SaveManager>
    {
        private const float DELAYED_SAVE_INTERVAL = 0.5f;

        private readonly HashSet<IDataController> mImmediateControllers = new HashSet<IDataController>();
        private readonly HashSet<IDataController> mDelayedControllers = new HashSet<IDataController>();
        private readonly List<IDataController> mAllControllers = new List<IDataController>();

        private float mDelayedSaveTimer = 0f;
        private SaveManagerUpdateProxy mUpdateProxy;

        private SaveManager() { }

        public override void OnSingletonInit()
        {
            EnsureUpdateProxy();
            Debug.Log("[SaveManager] 存储管理器已初始化");
        }

        private void EnsureUpdateProxy()
        {
            if (mUpdateProxy == null)
            {
                var proxyGO = new GameObject("[SaveManager UpdateProxy]");
                Object.DontDestroyOnLoad(proxyGO);
                mUpdateProxy = proxyGO.AddComponent<SaveManagerUpdateProxy>();
                mUpdateProxy.Initialize(this);
            }
        }

        public void RegisterController(IDataController controller)
        {
            if (controller == null)
            {
                Debug.LogError("[SaveManager] 无法注册空的控制器");
                return;
            }

            if (!mAllControllers.Contains(controller))
            {
                mAllControllers.Add(controller);
                Debug.Log($"[SaveManager] 注册控制器: {controller.GetType().Name}, 存储模式: {controller.SaveMode}");
            }
        }

        public void UnregisterController(IDataController controller)
        {
            if (controller == null) return;

            mAllControllers.Remove(controller);
            mImmediateControllers.Remove(controller);
            mDelayedControllers.Remove(controller);
            Debug.Log($"[SaveManager] 注销控制器: {controller.GetType().Name}");
        }

        public void RegisterDirtyController(IDataController controller)
        {
            if (controller == null || !controller.IsDirty) return;

            if (controller.SaveMode == SaveMode.Immediate)
            {
                mImmediateControllers.Add(controller);
            }
            else
            {
                mDelayedControllers.Add(controller);
            }
        }

        internal void OnUpdate(float deltaTime)
        {
            ProcessImmediateSave();
            ProcessDelayedSave(deltaTime);
        }

        private void ProcessImmediateSave()
        {
            if (mImmediateControllers.Count == 0) return;

            foreach (var controller in mImmediateControllers)
            {
                if (controller != null && controller.IsDirty)
                {
                    controller.Save();
                    controller.ClearDirty();
                }
            }

            mImmediateControllers.Clear();
        }

        private void ProcessDelayedSave(float deltaTime)
        {
            if (mDelayedControllers.Count == 0) return;

            mDelayedSaveTimer += deltaTime;

            if (mDelayedSaveTimer >= DELAYED_SAVE_INTERVAL)
            {
                foreach (var controller in mDelayedControllers)
                {
                    if (controller != null && controller.IsDirty)
                    {
                        controller.Save();
                        controller.ClearDirty();
                    }
                }

                mDelayedControllers.Clear();
                mDelayedSaveTimer = 0f;
            }
        }

        public void SaveAll()
        {
            Debug.Log("[SaveManager] 保存所有控制器数据");
            foreach (var controller in mAllControllers)
            {
                if (controller != null)
                {
                    controller.Save();
                    controller.ClearDirty();
                }
            }

            mImmediateControllers.Clear();
            mDelayedControllers.Clear();
        }

        public void SaveAllImmediate()
        {
            Debug.Log("[SaveManager] 立即保存所有即时存储控制器");
            foreach (var controller in mAllControllers)
            {
                if (controller != null && controller.SaveMode == SaveMode.Immediate)
                {
                    controller.Save();
                    controller.ClearDirty();
                }
            }

            mImmediateControllers.Clear();
        }

        public void SaveAllDelayed()
        {
            Debug.Log("[SaveManager] 立即保存所有延时存储控制器");
            foreach (var controller in mAllControllers)
            {
                if (controller != null && controller.SaveMode == SaveMode.Delayed)
                {
                    controller.Save();
                    controller.ClearDirty();
                }
            }

            mDelayedControllers.Clear();
        }

        internal void OnApplicationQuit()
        {
            Debug.Log("[SaveManager] 应用退出，保存所有数据");
            SaveAll();
        }

        internal void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                Debug.Log("[SaveManager] 应用暂停，保存所有数据");
                SaveAll();
            }
        }

        public override void Dispose()
        {
            if (mUpdateProxy != null)
            {
                Object.Destroy(mUpdateProxy.gameObject);
                mUpdateProxy = null;
            }
            
            mAllControllers.Clear();
            mImmediateControllers.Clear();
            mDelayedControllers.Clear();
            base.Dispose();
        }
    }
}
