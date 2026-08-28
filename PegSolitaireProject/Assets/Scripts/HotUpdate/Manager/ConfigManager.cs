using System;
using Luban;
using QFramework;
using UnityEngine;
using YooAsset;

namespace Game.Config
{
    /// <summary>
    /// Loads and provides access to the Luban configuration tables.
    /// </summary>
    public sealed class ConfigManager : Singleton<ConfigManager>
    {
        private static readonly string[] ConfigList =
        {
            "tbitem",
            "tblanguage",
            "tbshopitems",
            "tbcensorship"
        };

        private bool mIsInitialized;
        private Tables mTables;

        private ConfigManager()
        {
        }

        public bool IsInitialized => mIsInitialized;

        public Tables Tables
        {
            get
            {
                EnsureInitialized();
                return mTables;
            }
        }

        public void Initialize()
        {
            if (mIsInitialized)
            {
                Debug.LogWarning("[ConfigManager] Configuration tables are already initialized.");
                return;
            }

            var package = YooAssetBridge.DefaultPackage;
            if (package == null)
            {
                throw new InvalidOperationException("[ConfigManager] YooAsset default package is not initialized.");
            }

            mTables = new Tables(configName => LoadConfigBuffer(package, configName));
            mIsInitialized = true;
            Debug.Log($"[ConfigManager] Loaded {ConfigList.Length} configuration tables.");
        }

        public Item GetItem(int id) => Tables.TbItem.GetOrDefault(id);

        public Language GetLanguage(int id) => Tables.TbLanguage.GetOrDefault(id);

        public ShopItems GetShopItem(int id) => Tables.TbShopItems.GetOrDefault(id);

        public Censorship GetCensorship(int id) => Tables.TbCensorship.GetOrDefault(id);

        public override void Dispose()
        {
            mTables = null;
            mIsInitialized = false;
            base.Dispose();
        }

        private static ByteBuf LoadConfigBuffer(ResourcePackage package, string configName)
        {
            using var handle = package.LoadAssetSync<TextAsset>(configName);
            if (handle.Status != EOperationStatus.Succeeded)
            {
                throw new InvalidOperationException(
                    $"[ConfigManager] Failed to load configuration '{configName}': {handle.Error}");
            }

            var configAsset = handle.AssetObject as TextAsset;
            if (configAsset == null)
            {
                throw new InvalidOperationException(
                    $"[ConfigManager] Configuration '{configName}' is not a TextAsset.");
            }

            return new ByteBuf(configAsset.bytes);
        }

        private void EnsureInitialized()
        {
            if (!mIsInitialized)
            {
                throw new InvalidOperationException("[ConfigManager] Call Initialize before accessing configuration tables.");
            }
        }
    }
}