using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace HotUpdate.UI
{
    public sealed class LobbyPanelData : UIPanelData
    {
        public int LevelNumber { get; }
        public Action<int> EnterLevel { get; }

        public LobbyPanelData(int levelNumber, Action<int> enterLevel)
        {
            LevelNumber = levelNumber;
            EnterLevel = enterLevel;
        }
    }

    public class LobbyPanel : UIPanel
    {
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private Button enterLevelButton;

        private LobbyPanelData Data => mUIData as LobbyPanelData;

        protected override void OnInit(IUIData uiData = null)
        {
            var nestedCanvas = GetComponent<Canvas>();
            if (nestedCanvas != null)
                nestedCanvas.enabled = false;

            var nestedRaycaster = GetComponent<GraphicRaycaster>();
            if (nestedRaycaster != null)
                nestedRaycaster.enabled = false;

            if (levelText == null)
                levelText = GetComponentInChildren<TextMeshProUGUI>(true);

            if (enterLevelButton == null)
                enterLevelButton = GetComponentInChildren<Button>(true);

            if (enterLevelButton == null)
            {
                Debug.LogError("[LobbyPanel] Enter level button is missing.");
                return;
            }

            enterLevelButton.onClick.RemoveListener(OnEnterLevelClicked);
            enterLevelButton.onClick.AddListener(OnEnterLevelClicked);
        }

        protected override void OnOpen(IUIData uiData = null)
        {
            mUIData = uiData;
            RefreshLevelText();
            Debug.Log($"[LobbyPanel] Opened at Level {Data?.LevelNumber ?? 1}");
        }

        protected override void OnClose()
        {
        }

        protected override void ClearUIComponents()
        {
            if (enterLevelButton != null)
                enterLevelButton.onClick.RemoveListener(OnEnterLevelClicked);

            levelText = null;
            enterLevelButton = null;
        }

        private void RefreshLevelText()
        {
            if (levelText != null)
                levelText.text = $"Level {Data?.LevelNumber ?? 1:D2}";
        }

        private void OnEnterLevelClicked()
        {
            var levelNumber = Data?.LevelNumber ?? 1;
            Debug.Log($"[LobbyPanel] Enter Level {levelNumber}");
            Data?.EnterLevel?.Invoke(levelNumber);
        }
    }
}
