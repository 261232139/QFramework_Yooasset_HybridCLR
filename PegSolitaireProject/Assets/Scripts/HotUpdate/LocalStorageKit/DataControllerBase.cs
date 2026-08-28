/****************************************************************************
 * DataControllerBase — 数据控制器基类
 * 
 * 职责：为所有数据控制器提供通用的存储逻辑
 * 包含存储键、数据变更标识等基础功能
 * 继承 QFramework.Singleton<T>，子类也会是单例
 ****************************************************************************/

using UnityEngine;
using QFramework;

namespace HotUpdate.LocalStorageKit
{
    public enum SaveMode
    {
        Immediate,
        Delayed
    }

    public abstract class DataControllerBase<T> : Singleton<T>, IDataController where T : DataControllerBase<T>
    {
        protected abstract string SAVE_KEY { get; }
        
        public abstract SaveMode SaveMode { get; }
        
        public bool IsDirty { get; protected set; }

        public override void OnSingletonInit()
        {
            IsDirty = false;
            SaveManager.Instance.RegisterController(this);
            Debug.Log($"[{typeof(T).Name}] 已初始化并注册到存储管理器");
        }

        public override void Dispose()
        {
            SaveManager.Instance.UnregisterController(this);
            IsDirty = false;
            base.Dispose();
        }

        protected void MarkDirty()
        {
            if (!IsDirty)
            {
                IsDirty = true;
                SaveManager.Instance.RegisterDirtyController(this);
            }
        }

        public void ClearDirty()
        {
            IsDirty = false;
        }

        public abstract void Save();
        
        public abstract void Load();
    }
}
