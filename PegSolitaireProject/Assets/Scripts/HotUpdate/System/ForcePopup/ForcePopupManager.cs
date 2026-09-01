using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HotUpdate.ForcePopup
{
    public enum ForcePopupStep
    {
        BeforePopupStarted,
        BeforePopupFinished,
        PopupStarted,
        PopupFinished,
        AfterPopupStarted,
        AfterPopupFinished,
        CompletionWaitStarted,
        CompletionWaitFinished
    }

    /// <summary>
    /// Runs eligible popup items sequentially. It performs no UI work itself; item implementations own presentation.
    /// </summary>
    public sealed class ForcePopupManager : IDisposable
    {
        private readonly List<PopupEntry> mItems = new List<PopupEntry>();
        private readonly Dictionary<string, PopupEntry> mItemsById = new Dictionary<string, PopupEntry>();
        private Func<bool> mCanExecute = () => true;
        private CancellationTokenSource mCancellation;
        private long mRegistrationOrder;

        public IForcePopupItem CurrentItem { get; private set; }

        public bool HasPoppedItem { get; private set; }

        public bool IsExecuting { get; private set; }

        public event Action<IForcePopupItem, ForcePopupStep> ItemStepChanged;

        public event Action<IForcePopupItem, Exception> ItemFailed;

        public void Configure(Func<bool> canExecute)
        {
            mCanExecute = canExecute ?? (() => true);
        }

        public void Register(IForcePopupItem item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (string.IsNullOrWhiteSpace(item.Id)) throw new ArgumentException("A popup item ID is required.", nameof(item));
            if (mItemsById.ContainsKey(item.Id)) throw new InvalidOperationException($"A popup item named '{item.Id}' is already registered.");

            var entry = new PopupEntry(item, mRegistrationOrder++);
            mItems.Add(entry);
            mItemsById.Add(item.Id, entry);
            mItems.Sort(PopupEntry.Compare);
        }

        public bool Unregister(string itemId)
        {
            if (!mItemsById.TryGetValue(itemId, out var entry) || ReferenceEquals(CurrentItem, entry.Item))
            {
                return false;
            }

            mItemsById.Remove(itemId);
            return mItems.Remove(entry);
        }

        public bool TryGetItem(string itemId, out IForcePopupItem item)
        {
            if (mItemsById.TryGetValue(itemId, out var entry))
            {
                item = entry.Item;
                return true;
            }

            item = null;
            return false;
        }

        public void Start()
        {
            if (IsExecuting)
            {
                return;
            }

            mCancellation = new CancellationTokenSource();
            _ = RunAsync(mCancellation.Token);
        }

        public void Stop()
        {
            mCancellation?.Cancel();
        }

        public void Dispose()
        {
            Stop();
            mCancellation?.Dispose();
            mCancellation = null;
            mItems.Clear();
            mItemsById.Clear();
            CurrentItem = null;
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            IsExecuting = true;
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (!CanExecute())
                    {
                        await Task.Yield();
                        continue;
                    }

                    var item = GetNextEligibleItem();
                    if (item == null)
                    {
                        await Task.Yield();
                        continue;
                    }

                    await ExecuteItemAsync(item, cancellationToken);
                    await Task.Yield();
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
            }
            finally
            {
                HasPoppedItem = false;
                CurrentItem = null;
                IsExecuting = false;
            }
        }

        private async Task ExecuteItemAsync(IForcePopupItem item, CancellationToken cancellationToken)
        {
            CurrentItem = item;
            try
            {
                PublishStep(item, ForcePopupStep.BeforePopupStarted);
                await item.BeforePopupAsync(cancellationToken);
                PublishStep(item, ForcePopupStep.BeforePopupFinished);

                while (!item.CanPopup() || !CanExecute())
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await Task.Yield();
                }

                HasPoppedItem = true;
                PublishStep(item, ForcePopupStep.PopupStarted);
                await item.PopupAsync(cancellationToken);
                PublishStep(item, ForcePopupStep.PopupFinished);

                PublishStep(item, ForcePopupStep.AfterPopupStarted);
                await item.AfterPopupAsync(cancellationToken);
                PublishStep(item, ForcePopupStep.AfterPopupFinished);

                PublishStep(item, ForcePopupStep.CompletionWaitStarted);
                while (!item.IsPopupComplete())
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await Task.Yield();
                }
                PublishStep(item, ForcePopupStep.CompletionWaitFinished);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                ItemFailed?.Invoke(item, exception);
            }
            finally
            {
                HasPoppedItem = false;
                CurrentItem = null;
            }
        }

        private bool CanExecute()
        {
            try
            {
                return mCanExecute();
            }
            catch (Exception exception)
            {
                ItemFailed?.Invoke(null, exception);
                return false;
            }
        }

        private IForcePopupItem GetNextEligibleItem()
        {
            foreach (var entry in mItems)
            {
                try
                {
                    if (entry.Item.NeedPopup())
                    {
                        return entry.Item;
                    }
                }
                catch (Exception exception)
                {
                    ItemFailed?.Invoke(entry.Item, exception);
                }
            }

            return null;
        }

        private void PublishStep(IForcePopupItem item, ForcePopupStep step)
        {
            ItemStepChanged?.Invoke(item, step);
        }

        private sealed class PopupEntry
        {
            public PopupEntry(IForcePopupItem item, long registrationOrder)
            {
                Item = item;
                RegistrationOrder = registrationOrder;
            }

            public IForcePopupItem Item { get; }

            private long RegistrationOrder { get; }

            public static int Compare(PopupEntry left, PopupEntry right)
            {
                var priorityComparison = left.Item.Priority.CompareTo(right.Item.Priority);
                return priorityComparison != 0 ? priorityComparison : left.RegistrationOrder.CompareTo(right.RegistrationOrder);
            }
        }
    }
}
