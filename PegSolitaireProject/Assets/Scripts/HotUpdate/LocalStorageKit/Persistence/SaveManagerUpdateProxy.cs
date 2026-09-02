/****************************************************************************
 * SaveManagerUpdateProxy — SaveManager 的 Update 代理
 * 
 * 职责：作为 MonoBehaviour，为非 MonoBehaviour 的 SaveManager 提供 Update 驱动
 * 这是一个轻量级的代理模式实现，遵循 QFramework 的设计理念
 ****************************************************************************/

using UnityEngine;

namespace HotUpdate.LocalStorageKit
{
    internal class SaveManagerUpdateProxy : MonoBehaviour
    {
        private SaveManager mSaveManager;

        public void Initialize(SaveManager saveManager)
        {
            mSaveManager = saveManager;
        }

        private void Update()
        {
            mSaveManager?.OnUpdate(Time.deltaTime);
        }

        private void OnApplicationQuit()
        {
            mSaveManager?.OnApplicationQuit();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            mSaveManager?.OnApplicationPause(pauseStatus);
        }
    }
}
