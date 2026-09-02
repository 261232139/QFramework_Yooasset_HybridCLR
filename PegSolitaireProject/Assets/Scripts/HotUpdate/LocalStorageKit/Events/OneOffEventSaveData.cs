using System;
using System.Collections.Generic;

namespace HotUpdate.LocalStorageKit
{
    [Serializable]
    public sealed class OneOffEventRecord
    {
        public string key;
        public int state;
    }

    /// <summary>
    /// Serializable storage for one-off event states.
    /// JsonUtility does not serialize Dictionary, so records are persisted as a list.
    /// </summary>
    [Serializable]
    public sealed class OneOffEventSaveData
    {
        public const int CurrentDataVersion = 1;

        public int dataVersion = CurrentDataVersion;
        public List<OneOffEventRecord> eventRecords = new List<OneOffEventRecord>();
    }
}
