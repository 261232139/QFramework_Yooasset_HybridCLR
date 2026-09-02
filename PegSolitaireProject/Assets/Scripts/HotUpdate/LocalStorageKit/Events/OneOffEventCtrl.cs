using System;
using System.Collections.Generic;
using UnityEngine;

namespace HotUpdate.LocalStorageKit
{
    /// <summary>Persists one-off event flags and integer progress states.</summary>
    public sealed class OneOffEventCtrl : DataControllerBase<OneOffEventCtrl>
    {
        protected override string SAVE_KEY => "OneOffEvent";

        public override SaveMode SaveMode => SaveMode.Immediate;

        private OneOffEventSaveData mSaveData;

        public OneOffEventSaveData Data
        {
            get
            {
                if (mSaveData == null)
                {
                    Load();
                }

                return mSaveData;
            }
        }

        private OneOffEventCtrl()
        {
        }

        public override void OnSingletonInit()
        {
            base.OnSingletonInit();
            Load();
        }

        public bool CheckEventTriggered(OneOffEventType eventType)
        {
            return GetEventState(eventType) > 0;
        }

        public void TriggerEvent(OneOffEventType eventType, bool isTriggered = true)
        {
            SetEventState(eventType, isTriggered ? 1 : 0);
        }

        public int GetEventState(OneOffEventType eventType)
        {
            return GetEventState(eventType.ToString());
        }

        public void SetEventState(OneOffEventType eventType, int state)
        {
            SetEventState(eventType.ToString(), state);
        }

        public bool CheckEventTriggered(string eventKey)
        {
            return GetEventState(eventKey) > 0;
        }

        public int GetEventState(string eventKey)
        {
            ValidateKey(eventKey);
            var records = Data.eventRecords;
            for (var i = 0; i < records.Count; i++)
            {
                if (records[i].key == eventKey)
                {
                    return records[i].state;
                }
            }

            return 0;
        }

        public void SetEventState(string eventKey, int state)
        {
            ValidateKey(eventKey);
            var records = Data.eventRecords;
            for (var i = 0; i < records.Count; i++)
            {
                if (records[i].key != eventKey) continue;
                if (records[i].state == state) return;

                records[i].state = state;
                MarkDirty();
                return;
            }

            records.Add(new OneOffEventRecord { key = eventKey, state = state });
            MarkDirty();
        }

        public void TriggerEvent(string eventKey, bool isTriggered = true)
        {
            SetEventState(eventKey, isTriggered ? 1 : 0);
        }

        public void ResetData()
        {
            mSaveData = new OneOffEventSaveData();
            MarkDirty();
        }

        public override void Load()
        {
            try
            {
                if (ES3.KeyExists(SAVE_KEY))
                {
                    mSaveData = JsonUtility.FromJson<OneOffEventSaveData>(ES3.Load<string>(SAVE_KEY));
                    if (mSaveData == null)
                    {
                        throw new InvalidOperationException("One-off event JSON is empty or invalid.");
                    }

                    if (Normalize())
                    {
                        MarkDirty();
                    }

                    return;
                }
            }
            catch (Exception exception)
            {
                Debug.LogError($"[OneOffEventCtrl] 读取存档失败，将使用默认数据。{exception}");
            }

            mSaveData = new OneOffEventSaveData();
            MarkDirty();
        }

        public override void Save()
        {
            if (mSaveData == null) return;

            ES3.Save<string>(SAVE_KEY, JsonUtility.ToJson(mSaveData));
        }

        public override void Dispose()
        {
            mSaveData = null;
            base.Dispose();
        }

        private bool Normalize()
        {
            var changed = false;
            if (mSaveData.dataVersion != OneOffEventSaveData.CurrentDataVersion)
            {
                mSaveData.dataVersion = OneOffEventSaveData.CurrentDataVersion;
                changed = true;
            }

            if (mSaveData.eventRecords == null)
            {
                mSaveData.eventRecords = new List<OneOffEventRecord>();
                return true;
            }

            var lastStateByKey = new Dictionary<string, int>();
            foreach (var record in mSaveData.eventRecords)
            {
                if (record == null || string.IsNullOrWhiteSpace(record.key))
                {
                    changed = true;
                    continue;
                }

                if (lastStateByKey.ContainsKey(record.key)) changed = true;
                lastStateByKey[record.key] = record.state;
            }

            if (changed)
            {
                mSaveData.eventRecords.Clear();
                foreach (var pair in lastStateByKey)
                {
                    mSaveData.eventRecords.Add(new OneOffEventRecord { key = pair.Key, state = pair.Value });
                }
            }

            return changed;
        }

        private static void ValidateKey(string eventKey)
        {
            if (string.IsNullOrWhiteSpace(eventKey))
            {
                throw new ArgumentException("Event key cannot be null or empty.", nameof(eventKey));
            }
        }
    }
}
