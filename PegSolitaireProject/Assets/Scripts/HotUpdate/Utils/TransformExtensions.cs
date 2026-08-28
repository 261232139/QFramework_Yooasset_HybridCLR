using UnityEngine;

namespace HotUpdate.Utils
{
    public static class TransformExtensions
    {
        /// <summary>
        /// Finds a child transform recursively. Avoid using this in per-frame code.
        /// </summary>
        public static Transform DeepFind(this Transform parent, string name)
        {
            if (parent == null) return null;
            if (string.IsNullOrEmpty(name)) return parent;

            var result = parent.Find(name);
            if (result != null) return result;

            foreach (Transform child in parent)
            {
                result = child.DeepFind(name);
                if (result != null) return result;
            }

            return null;
        }
    }
}
