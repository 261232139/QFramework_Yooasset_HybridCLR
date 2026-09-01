using System;
using System.Collections.Generic;
using System.Linq;
using Game.Config;
using QFramework;
using UnityEngine;
using UnityEngine.Purchasing;

namespace HotUpdate.Game
{
    public readonly struct LocalizedPriceInfo
    {
        public readonly string PriceString;
        public readonly decimal PriceValue;
        public readonly string CurrencyCode;
        public readonly bool IsAvailable;

        public LocalizedPriceInfo(string priceString, decimal priceValue, string currencyCode, bool isAvailable)
        {
            PriceString = priceString;
            PriceValue = priceValue;
            CurrencyCode = currencyCode;
            IsAvailable = isAvailable;
        }
    }

    /// <summary>
    /// Unity IAP 5 manager. Product definitions are sourced from tbshopitems:
    /// ProductId is the store product ID and RepeatPurchase determines whether
    /// the product is consumable.
    /// </summary>
    public sealed class IAPManager : Singleton<IAPManager>
    {
        private readonly Dictionary<string, ShopItems> mShopItemsByProductId = new();
        private readonly Dictionary<string, PendingPurchaseCallback> mPendingCallbacks = new();

        private StoreController mStoreController;
        private bool mIsInitializing;
        private bool mEventsRegistered;
        private bool mProductsFetched;

        private IAPManager() { }

        public bool IsInitialized => mStoreController != null && mProductsFetched;

        public event Action<IReadOnlyList<Product>> ProductsLoaded;
        public event Action<ShopItems, string> PurchaseGranted;
        public event Action<ShopItems, string> EntitlementRestored;
        public event Action<string> PurchaseDeferred;
        public event Action<string> InitializationFailed;

        public async void InitializePurchasing()
        {
            if (IsInitialized || mIsInitializing) return;

            if (!ConfigManager.Instance.IsInitialized)
            {
                ReportInitializationFailure("ConfigManager must be initialized before IAPManager.");
                return;
            }

            BuildProductMap();
            if (mShopItemsByProductId.Count == 0)
            {
                ReportInitializationFailure("No valid ProductId entries were found in tbshopitems.");
                return;
            }

            mIsInitializing = true;
            try
            {
                mStoreController = UnityIAPServices.StoreController();
                RegisterStoreEvents();
                await mStoreController.Connect();
            }
            catch (Exception exception)
            {
                ReportInitializationFailure($"Unable to connect to the store: {exception.Message}");
            }
            finally
            {
                mIsInitializing = false;
            }
        }

        public void BuyProduct(string productId, Action<string, string> successCallback = null,
            Action<string, string, PurchaseFailureReason, string> failedCallback = null)
        {
            if (!IsInitialized)
            {
                FailPurchase(productId, failedCallback, PurchaseFailureReason.StoreNotConnected, "IAP is not initialized.");
                return;
            }

            if (string.IsNullOrWhiteSpace(productId) || !mShopItemsByProductId.ContainsKey(productId))
            {
                FailPurchase(productId, failedCallback, PurchaseFailureReason.ProductUnavailable,
                    "The requested product is not configured in tbshopitems.");
                return;
            }

            var product = mStoreController.GetProductById(productId);
            if (product == null || !product.availableToPurchase)
            {
                FailPurchase(productId, failedCallback, PurchaseFailureReason.ProductUnavailable,
                    "The product is unavailable in the current store.");
                return;
            }

            mPendingCallbacks[productId] = new PendingPurchaseCallback(successCallback, failedCallback);
            mStoreController.PurchaseProduct(product);
        }

        public void BuyShopItem(int shopItemId, Action<string, string> successCallback = null,
            Action<string, string, PurchaseFailureReason, string> failedCallback = null)
        {
            var shopItem = ConfigManager.Instance.GetShopItem(shopItemId);
            if (shopItem == null)
            {
                FailPurchase(string.Empty, failedCallback, PurchaseFailureReason.ProductUnavailable,
                    $"Shop item {shopItemId} does not exist.");
                return;
            }

            BuyProduct(shopItem.ProductId, successCallback, failedCallback);
        }

        /// <summary>Restores non-consumable purchases. Required by Apple storefronts.</summary>
        public void RestorePurchases(Action<bool, string> completed = null)
        {
            if (!IsInitialized)
            {
                completed?.Invoke(false, "IAP is not initialized.");
                return;
            }

            mStoreController.RestoreTransactions((success, error) => completed?.Invoke(success, error));
        }

        public ShopItems GetShopItemByProductId(string productId)
        {
            return !string.IsNullOrEmpty(productId) && mShopItemsByProductId.TryGetValue(productId, out var shopItem)
                ? shopItem : null;
        }

        public LocalizedPriceInfo GetLocalizedProductPriceInfo(string productId)
        {
            if (!IsInitialized) return new LocalizedPriceInfo(string.Empty, 0m, string.Empty, false);

            var product = mStoreController.GetProductById(productId);
            return product != null && product.availableToPurchase
                ? new LocalizedPriceInfo(product.metadata.localizedPriceString, product.metadata.localizedPrice,
                    product.metadata.isoCurrencyCode, true)
                : new LocalizedPriceInfo(string.Empty, 0m, string.Empty, false);
        }

        private void BuildProductMap()
        {
            mShopItemsByProductId.Clear();
            foreach (var shopItem in ConfigManager.Instance.Tables.TbShopItems.DataList)
            {
                if (string.IsNullOrWhiteSpace(shopItem.ProductId)) continue;
                if (!mShopItemsByProductId.TryAdd(shopItem.ProductId, shopItem))
                    Debug.LogError($"[IAPManager] Duplicate ProductId '{shopItem.ProductId}' in tbshopitems.");
            }
        }

        private void RegisterStoreEvents()
        {
            if (mEventsRegistered) return;

            mStoreController.OnStoreConnected += OnStoreConnected;
            mStoreController.OnStoreDisconnected += OnStoreDisconnected;
            mStoreController.OnProductsFetched += OnProductsFetched;
            mStoreController.OnProductsFetchFailed += OnProductsFetchFailed;
            mStoreController.OnPurchasePending += OnPurchasePending;
            mStoreController.OnPurchaseConfirmed += OnPurchaseConfirmed;
            mStoreController.OnPurchaseFailed += OnPurchaseFailed;
            mStoreController.OnPurchaseDeferred += OnPurchaseDeferred;
            mStoreController.OnPurchasesFetched += OnPurchasesFetched;
            mStoreController.OnPurchasesFetchFailed += OnPurchasesFetchFailed;
            mEventsRegistered = true;
        }

        private void UnregisterStoreEvents()
        {
            if (!mEventsRegistered || mStoreController == null) return;

            mStoreController.OnStoreConnected -= OnStoreConnected;
            mStoreController.OnStoreDisconnected -= OnStoreDisconnected;
            mStoreController.OnProductsFetched -= OnProductsFetched;
            mStoreController.OnProductsFetchFailed -= OnProductsFetchFailed;
            mStoreController.OnPurchasePending -= OnPurchasePending;
            mStoreController.OnPurchaseConfirmed -= OnPurchaseConfirmed;
            mStoreController.OnPurchaseFailed -= OnPurchaseFailed;
            mStoreController.OnPurchaseDeferred -= OnPurchaseDeferred;
            mStoreController.OnPurchasesFetched -= OnPurchasesFetched;
            mStoreController.OnPurchasesFetchFailed -= OnPurchasesFetchFailed;
            mEventsRegistered = false;
        }

        private void OnStoreConnected()
        {
            var definitions = mShopItemsByProductId.Values.Select(item => new ProductDefinition(item.ProductId,
                item.RepeatPurchase ? ProductType.Consumable : ProductType.NonConsumable)).ToList();
            mStoreController.FetchProducts(definitions);
        }

        private void OnStoreDisconnected(StoreConnectionFailureDescription failure)
        {
            mProductsFetched = false;
            ReportInitializationFailure($"Store disconnected: {failure.Message}");
        }

        private void OnProductsFetched(List<Product> products)
        {
            mProductsFetched = true;
            ProductsLoaded?.Invoke(products);
            mStoreController.FetchPurchases();
        }

        private void OnProductsFetchFailed(ProductFetchFailed failure)
        {
            ReportInitializationFailure($"Product fetch failed: {failure.FailureReason}");
        }

        private void OnPurchasePending(PendingOrder order)
        {
            var productId = GetPrimaryProduct(order)?.definition.id;
            var transactionId = order.Info.TransactionID;
            if (string.IsNullOrEmpty(productId) || !mShopItemsByProductId.TryGetValue(productId, out var shopItem))
            {
                Debug.LogError("[IAPManager] A pending order does not map to a configured product. It will not be confirmed.");
                return;
            }

            if (PurchaseGranted == null)
            {
                Debug.LogError("[IAPManager] No PurchaseGranted handler is registered. The order will remain pending for safe retry.");
                FailPendingPurchase(productId, transactionId, PurchaseFailureReason.Unknown,
                    "No handler is registered to grant the purchased content.");
                return;
            }

            try
            {
                PurchaseGranted.Invoke(shopItem, transactionId);
                mPendingCallbacks.TryGetValue(productId, out var callbacks);
                callbacks.SuccessCallback?.Invoke(productId, transactionId);
                mPendingCallbacks.Remove(productId);
                mStoreController.ConfirmPurchase(order);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                FailPendingPurchase(productId, transactionId, PurchaseFailureReason.Unknown, exception.Message);
            }
        }

        private void OnPurchaseConfirmed(Order order)
        {
            Debug.Log($"[IAPManager] Purchase confirmed: {GetPrimaryProduct(order)?.definition.id}");
        }

        private void OnPurchaseFailed(FailedOrder order)
        {
            FailPendingPurchase(GetPrimaryProduct(order)?.definition.id ?? string.Empty, order.Info.TransactionID,
                order.FailureReason, order.Details);
        }

        private void OnPurchaseDeferred(DeferredOrder order)
        {
            var productId = GetPrimaryProduct(order)?.definition.id ?? string.Empty;
            Debug.Log($"[IAPManager] Purchase deferred: {productId}");
            PurchaseDeferred?.Invoke(productId);
        }

        private void OnPurchasesFetched(Orders orders)
        {
            foreach (var order in orders.ConfirmedOrders)
            {
                var productId = GetPrimaryProduct(order)?.definition.id;
                if (!string.IsNullOrEmpty(productId) && mShopItemsByProductId.TryGetValue(productId, out var shopItem)
                    && !shopItem.RepeatPurchase)
                    EntitlementRestored?.Invoke(shopItem, order.Info.TransactionID);
            }
        }

        private void OnPurchasesFetchFailed(PurchasesFetchFailureDescription failure)
        {
            Debug.LogWarning($"[IAPManager] Purchase synchronization failed: {failure.FailureReason} - {failure.Message}");
        }

        private static Product GetPrimaryProduct(Order order) => order.CartOrdered?.Items()?.FirstOrDefault()?.Product;

        private void FailPendingPurchase(string productId, string transactionId, PurchaseFailureReason reason, string message)
        {
            Debug.LogError($"[IAPManager] Purchase failed ({productId}): {reason} - {message}");
            if (mPendingCallbacks.TryGetValue(productId, out var callbacks))
            {
                callbacks.FailedCallback?.Invoke(productId, transactionId, reason, message);
                mPendingCallbacks.Remove(productId);
            }
        }

        private static void FailPurchase(string productId,
            Action<string, string, PurchaseFailureReason, string> failedCallback,
            PurchaseFailureReason reason, string message)
        {
            Debug.LogError($"[IAPManager] Purchase failed ({productId}): {reason} - {message}");
            failedCallback?.Invoke(productId, string.Empty, reason, message);
        }

        private void ReportInitializationFailure(string message)
        {
            Debug.LogError($"[IAPManager] {message}");
            InitializationFailed?.Invoke(message);
        }

        public override void Dispose()
        {
            UnregisterStoreEvents();
            mStoreController = null;
            mProductsFetched = false;
            mIsInitializing = false;
            mPendingCallbacks.Clear();
            mShopItemsByProductId.Clear();
            base.Dispose();
        }

        private readonly struct PendingPurchaseCallback
        {
            public readonly Action<string, string> SuccessCallback;
            public readonly Action<string, string, PurchaseFailureReason, string> FailedCallback;

            public PendingPurchaseCallback(Action<string, string> successCallback,
                Action<string, string, PurchaseFailureReason, string> failedCallback)
            {
                SuccessCallback = successCallback;
                FailedCallback = failedCallback;
            }
        }
    }
}
