using System;
using System.Collections.Generic;
using UnityEngine;

namespace HotUpdate.RedDot
{
    public delegate void OnRedDotChangedCallback(RedDotNode node);

    /// <summary>
    /// A node in a red-dot tree. Leaf values are set explicitly; parent values are the sum of their children.
    /// </summary>
    public sealed class RedDotNode
    {
        private const string StorageKeyPrefix = "HotUpdate.RedDot.";

        private readonly Dictionary<string, RedDotNode> mChildren = new Dictionary<string, RedDotNode>();
        private readonly bool mSaveLocal;
        private int mCount;

        internal RedDotNode(string name, RedDotNode parent, bool saveLocal)
        {
            Name = name;
            Parent = parent;
            mSaveLocal = saveLocal;
        }

        public string Name { get; }

        public RedDotNode Parent { get; private set; }

        public IReadOnlyDictionary<string, RedDotNode> Children => mChildren;

        public int Count => mCount;

        public bool SavesLocally => mSaveLocal;

        public event OnRedDotChangedCallback Changed;

        internal void AddChild(RedDotNode child)
        {
            mChildren.Add(child.Name, child);
            RefreshFromChildren();
        }

        internal void RemoveChild(string childName)
        {
            if (mChildren.Remove(childName))
            {
                RefreshFromChildren();
            }
        }

        internal void Detach()
        {
            Changed = null;
            Parent = null;
            mChildren.Clear();
        }

        internal void NotifyChanged()
        {
            Changed?.Invoke(this);
        }

        internal void SetCallback(OnRedDotChangedCallback callback)
        {
            Changed = callback;
        }

        /// <summary>
        /// Sets a leaf node's count. Negative values are clamped to zero.
        /// </summary>
        public void SetCount(int count)
        {
            if (mChildren.Count > 0)
            {
                throw new InvalidOperationException($"Cannot set '{Name}' directly because it has child nodes.");
            }

            SetCountInternal(Math.Max(0, count), true);
        }

        public void LoadLocalData()
        {
            if (!mSaveLocal || mChildren.Count > 0)
            {
                return;
            }

            SetCountInternal(PlayerPrefs.GetInt(GetStorageKey(), 0), false);
        }

        internal void RefreshFromChildren()
        {
            var total = 0;
            foreach (var child in mChildren.Values)
            {
                total += child.mCount;
            }

            SetCountInternal(total, false);
        }

        private void SetCountInternal(int count, bool saveLocal)
        {
            if (mCount == count)
            {
                return;
            }

            mCount = count;
            if (saveLocal && mSaveLocal)
            {
                PlayerPrefs.SetInt(GetStorageKey(), count);
                PlayerPrefs.Save();
            }

            NotifyChanged();
            Parent?.RefreshFromChildren();
        }

        private string GetStorageKey()
        {
            return StorageKeyPrefix + Name;
        }
    }
}
