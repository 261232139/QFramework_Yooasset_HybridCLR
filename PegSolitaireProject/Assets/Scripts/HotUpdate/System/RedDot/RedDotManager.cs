using System;
using System.Collections.Generic;
using QFramework;

namespace HotUpdate.RedDot
{
    /// <summary>
    /// Owns the red-dot tree and exposes node registration, updates, persistence, and change subscriptions.
    /// </summary>
    public sealed class RedDotManager : Singleton<RedDotManager>
    {
        private readonly Dictionary<string, RedDotNode> mNodes = new Dictionary<string, RedDotNode>();

        private RedDotManager()
        {
        }

        public override void OnSingletonInit()
        {
            RegisterBuiltInNodes();
        }

        public RedDotNode AddNode(string nodeName, string parentNodeName = null, bool saveLocal = false)
        {
            ValidateNodeName(nodeName);
            if (mNodes.ContainsKey(nodeName))
            {
                throw new InvalidOperationException($"A red-dot node named '{nodeName}' is already registered.");
            }

            RedDotNode parent = null;
            if (!string.IsNullOrEmpty(parentNodeName) && !mNodes.TryGetValue(parentNodeName, out parent))
            {
                throw new KeyNotFoundException($"Parent red-dot node '{parentNodeName}' is not registered.");
            }

            var node = new RedDotNode(nodeName, parent, saveLocal);
            mNodes.Add(nodeName, node);
            parent?.AddChild(node);
            return node;
        }

        public bool RemoveNode(string nodeName)
        {
            if (!mNodes.TryGetValue(nodeName, out var node))
            {
                return false;
            }

            RemoveNodeAndDescendants(node);
            return true;
        }

        public bool TryGetNode(string nodeName, out RedDotNode node)
        {
            return mNodes.TryGetValue(nodeName, out node);
        }

        public RedDotNode GetNode(string nodeName)
        {
            mNodes.TryGetValue(nodeName, out var node);
            return node;
        }

        public int GetRedDotCount(string nodeName)
        {
            return mNodes.TryGetValue(nodeName, out var node) ? node.Count : 0;
        }

        public void SetCount(string nodeName, int count)
        {
            GetRequiredNode(nodeName).SetCount(count);
        }

        public void LoadLocalRedDotData(string nodeName)
        {
            GetRequiredNode(nodeName).LoadLocalData();
        }

        public void Subscribe(string nodeName, OnRedDotChangedCallback callback, bool invokeImmediately = true)
        {
            if (callback == null) throw new ArgumentNullException(nameof(callback));

            var node = GetRequiredNode(nodeName);
            node.Changed += callback;
            if (invokeImmediately)
            {
                callback(node);
            }
        }

        public void Unsubscribe(string nodeName, OnRedDotChangedCallback callback)
        {
            if (callback == null || !mNodes.TryGetValue(nodeName, out var node))
            {
                return;
            }

            node.Changed -= callback;
        }

        // Compatibility aliases for the referenced RedDot API.
        public void SetInvoke(string nodeName, int redDotCount) => SetCount(nodeName, redDotCount);

        public void RefeshRedDot(string nodeName)
        {
            if (mNodes.TryGetValue(nodeName, out var node))
            {
                node.NotifyChanged();
            }
        }

        public void SetRedDotNodeCallBack(string nodeName, OnRedDotChangedCallback callback)
        {
            var node = GetRequiredNode(nodeName);
            node.SetCallback(callback);
            callback?.Invoke(node);
        }

        public override void Dispose()
        {
            foreach (var node in mNodes.Values)
            {
                node.Detach();
            }

            mNodes.Clear();
            base.Dispose();
        }

        private void RegisterBuiltInNodes()
        {
            if (!mNodes.ContainsKey(RedDotConst.LobbyCoinBar))
            {
                AddNode(RedDotConst.LobbyCoinBar);
            }
        }

        private RedDotNode GetRequiredNode(string nodeName)
        {
            if (!mNodes.TryGetValue(nodeName, out var node))
            {
                throw new KeyNotFoundException($"Red-dot node '{nodeName}' is not registered.");
            }

            return node;
        }

        private void RemoveNodeAndDescendants(RedDotNode node)
        {
            var childNames = new List<string>(node.Children.Keys);
            foreach (var childName in childNames)
            {
                RemoveNodeAndDescendants(mNodes[childName]);
            }

            node.Parent?.RemoveChild(node.Name);
            mNodes.Remove(node.Name);
            node.Detach();
        }

        private static void ValidateNodeName(string nodeName)
        {
            if (string.IsNullOrWhiteSpace(nodeName))
            {
                throw new ArgumentException("A red-dot node name is required.", nameof(nodeName));
            }
        }
    }
}
