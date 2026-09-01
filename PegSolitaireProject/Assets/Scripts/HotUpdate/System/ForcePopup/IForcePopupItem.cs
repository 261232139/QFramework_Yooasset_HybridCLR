using System.Threading;
using System.Threading.Tasks;

namespace HotUpdate.ForcePopup
{
    /// <summary>
    /// Describes one automatically scheduled popup. Lower priorities are shown first.
    /// </summary>
    public interface IForcePopupItem
    {
        string Id { get; }

        int Priority { get; }

        bool NeedPopup();

        bool CanPopup();

        Task BeforePopupAsync(CancellationToken cancellationToken);

        Task PopupAsync(CancellationToken cancellationToken);

        bool IsPopupComplete();

        Task AfterPopupAsync(CancellationToken cancellationToken);
    }
}
