using System;
using QFramework;

namespace HotUpdate.ForcePopup
{
    /// <summary>
    /// Hot-update facade for the automatic popup scheduler.
    /// Configure the gate after the lobby/UI is ready, then call Start.
    /// </summary>
    public sealed class ForcePopupSystem : Singleton<ForcePopupSystem>
    {
        private ForcePopupManager mManager;

        private ForcePopupSystem()
        {
        }

        public ForcePopupManager Manager => mManager;

        public bool Executing => mManager != null && mManager.IsExecuting;

        public bool HasPoppedItem => mManager != null && mManager.HasPoppedItem;

        public IForcePopupItem CurrentItem => mManager?.CurrentItem;

        public override void OnSingletonInit()
        {
            mManager = new ForcePopupManager();
        }

        public void Initialize(Func<bool> canExecute)
        {
            mManager.Configure(canExecute);
        }

        public void Register(IForcePopupItem item)
        {
            mManager.Register(item);
        }

        public bool Unregister(string itemId)
        {
            return mManager.Unregister(itemId);
        }

        public void Start()
        {
            mManager.Start();
        }

        public void Stop()
        {
            mManager.Stop();
        }

        public override void Dispose()
        {
            mManager?.Dispose();
            mManager = null;
            base.Dispose();
        }
    }
}
