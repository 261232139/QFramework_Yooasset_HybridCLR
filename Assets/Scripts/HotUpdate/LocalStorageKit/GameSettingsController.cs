/****************************************************************************
 * GameSettingsController — 游戏设置控制器（延时存储示例）
 * 
 * 职责：管理游戏设置数据（音量、画质等）
 * 使用延时存储模式，降低频繁保存的性能开销
 ****************************************************************************/

using UnityEngine;
using System;

namespace HotUpdate.LocalStorageKit
{
    [Serializable]
    public class GameSettingsData
    {
        public float musicVolume = 1.0f;
        public float sfxVolume = 1.0f;
        public int qualityLevel = 2;
        public bool vibrateEnabled = true;

        public override string ToString()
        {
            return $"GameSettings [Music: {musicVolume}, SFX: {sfxVolume}, Quality: {qualityLevel}, Vibrate: {vibrateEnabled}]";
        }
    }

    public class GameSettingsController : DataControllerBase<GameSettingsController>
    {
        protected override string SAVE_KEY => "GameSettings";

        public override SaveMode SaveMode => SaveMode.Delayed;

        private GameSettingsData mSettingsData;

        public GameSettingsData Data
        {
            get
            {
                if (mSettingsData == null)
                {
                    Load();
                }
                return mSettingsData;
            }
        }

        public float MusicVolume
        {
            get => Data.musicVolume;
            set
            {
                if (!Mathf.Approximately(Data.musicVolume, value))
                {
                    Data.musicVolume = Mathf.Clamp01(value);
                    MarkDirty();
                }
            }
        }

        public float SfxVolume
        {
            get => Data.sfxVolume;
            set
            {
                if (!Mathf.Approximately(Data.sfxVolume, value))
                {
                    Data.sfxVolume = Mathf.Clamp01(value);
                    MarkDirty();
                }
            }
        }

        public int QualityLevel
        {
            get => Data.qualityLevel;
            set
            {
                if (Data.qualityLevel != value)
                {
                    Data.qualityLevel = Mathf.Clamp(value, 0, 5);
                    MarkDirty();
                }
            }
        }

        public bool VibrateEnabled
        {
            get => Data.vibrateEnabled;
            set
            {
                if (Data.vibrateEnabled != value)
                {
                    Data.vibrateEnabled = value;
                    MarkDirty();
                }
            }
        }

        private GameSettingsController() { }

        public override void OnSingletonInit()
        {
            base.OnSingletonInit();
            Load();
        }

        public override void Load()
        {
            if (ES3.KeyExists(SAVE_KEY))
            {
                mSettingsData = ES3.Load<GameSettingsData>(SAVE_KEY);
                Debug.Log($"[GameSettingsController] 设置加载成功: {mSettingsData}");
            }
            else
            {
                mSettingsData = new GameSettingsData();
                Debug.Log("[GameSettingsController] 未找到设置，使用默认值");
                Save();
            }
        }

        public override void Save()
        {
            if (mSettingsData == null) return;

            ES3.Save<GameSettingsData>(SAVE_KEY, mSettingsData);
            Debug.Log($"[GameSettingsController] 设置已保存: {mSettingsData}");
        }

        public void ResetToDefault()
        {
            mSettingsData = new GameSettingsData();
            MarkDirty();
            Debug.Log("[GameSettingsController] 设置已重置为默认值");
        }

        public override void Dispose()
        {
            mSettingsData = null;
            base.Dispose();
        }
    }
}
