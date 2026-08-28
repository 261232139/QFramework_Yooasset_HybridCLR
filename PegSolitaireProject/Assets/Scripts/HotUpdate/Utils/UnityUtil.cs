using UnityEngine;

namespace HotUpdate.Utils
{
    /// <summary>
    /// Unity APIs that must be invoked from the main thread.
    /// </summary>
    public static class UnityUtil
    {
        public static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
