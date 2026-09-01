using System.Threading;
using System.Threading.Tasks;

namespace HotUpdate.ForcePopup
{
    /// <summary>
    /// Convenience base class for popup items that do not need setup or teardown work.
    /// </summary>
    public abstract class BaseForcePopupItem : IForcePopupItem
    {
        public abstract string Id { get; }

        public abstract int Priority { get; }

        public abstract bool NeedPopup();

        public abstract bool CanPopup();

        public abstract Task PopupAsync(CancellationToken cancellationToken);

        public abstract bool IsPopupComplete();

        public virtual Task BeforePopupAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public virtual Task AfterPopupAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
