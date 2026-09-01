/****************************************************************************
 * Copyright (c) 2024 Game Project UNDER MIT License
 *
 * 关卡 UI 控制器
 *
 * 负责关卡内的 UI 显示和交互
 ****************************************************************************/

using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game.Level.Runtime
{
    /// <summary>
    /// 关卡 UI 控制器
    /// 负责管理关卡内的所有 UI 元素
    /// </summary>
    public class LevelUIController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI levelNumberText;
        [SerializeField] private TextMeshProUGUI goalText;
        [SerializeField] private TextMeshProUGUI moveCountText;
        [SerializeField] private TextMeshProUGUI scoreText;

        [Header("Buttons")]
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button backButton;

        [Header("Panels")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject completePanel;
        [SerializeField] private GameObject failPanel;

        private LevelView levelView;

        private void Awake()
        {
            BindButtons();
            HideAllPanels();
        }

        public void Initialize(LevelView view)
        {
            levelView = view;

            // 订阅目标管理器事件
            var goalManager = view.GetGoalManager();
            if (goalManager != null)
            {
                goalManager.OnScoreChanged += UpdateScore;
                goalManager.OnMoveCountChanged += UpdateMoveCount;
            }

            Debug.Log("[LevelUIController] Initialized");
        }

        private void OnDestroy()
        {
            UnbindButtons();

            if (levelView != null)
            {
                var goalManager = levelView.GetGoalManager();
                if (goalManager != null)
                {
                    goalManager.OnScoreChanged -= UpdateScore;
                    goalManager.OnMoveCountChanged -= UpdateMoveCount;
                }
            }
        }

        #region 按钮绑定

        private void BindButtons()
        {
            if (pauseButton != null)
                pauseButton.onClick.AddListener(OnPauseClicked);

            if (restartButton != null)
                restartButton.onClick.AddListener(OnRestartClicked);

            if (backButton != null)
                backButton.onClick.AddListener(OnBackClicked);
        }

        private void UnbindButtons()
        {
            if (pauseButton != null)
                pauseButton.onClick.RemoveListener(OnPauseClicked);

            if (restartButton != null)
                restartButton.onClick.RemoveListener(OnRestartClicked);

            if (backButton != null)
                backButton.onClick.RemoveListener(OnBackClicked);
        }

        #endregion

        #region 按钮回调

        private void OnPauseClicked()
        {
            if (levelView != null)
            {
                if (levelView.IsPlaying)
                    levelView.PauseLevel();
                else
                    levelView.ResumeLevel();
            }
        }

        private void OnRestartClicked()
        {
            if (levelView != null)
            {
                HideAllPanels();
                levelView.ResetLevel();
            }
        }

        private void OnBackClicked()
        {
            // 返回大厅
            // 这里需要与状态机交互
            var stateMachine = Object.FindFirstObjectByType<State.LevelStateMachine>();
            if (stateMachine != null)
            {
                stateMachine.QuitToLobby();
            }
        }

        #endregion

        #region 生命周期回调

        public void OnLevelStart()
        {
            HideAllPanels();
            UpdateLevelInfo();
            UpdateGoalText();
            UpdateMoveCount(0);
            UpdateScore(0);

            Debug.Log("[LevelUIController] Level started UI");
        }

        public void OnLevelPause()
        {
            ShowPanel(pausePanel);
            Debug.Log("[LevelUIController] Level paused UI");
        }

        public void OnLevelResume()
        {
            HidePanel(pausePanel);
            Debug.Log("[LevelUIController] Level resumed UI");
        }

        public void OnLevelComplete()
        {
            ShowPanel(completePanel);
            Debug.Log("[LevelUIController] Level completed UI");
        }

        public void OnLevelFail()
        {
            ShowPanel(failPanel);
            Debug.Log("[LevelUIController] Level failed UI");
        }

        public void OnPieceMoved()
        {
            // 棋子移动时的 UI 反馈（如果需要）
        }

        #endregion

        #region UI 更新

        private void UpdateLevelInfo()
        {
            if (levelView == null)
                return;

            if (levelNumberText != null)
            {
                levelNumberText.text = $"Level {levelView.CurrentLevelNumber}";
            }
        }

        private void UpdateGoalText()
        {
            if (levelView == null || goalText == null)
                return;

            var goalManager = levelView.GetGoalManager();
            if (goalManager != null)
            {
                goalText.text = goalManager.GetGoalDescription();
            }
        }

        private void UpdateMoveCount(int count)
        {
            if (moveCountText != null)
            {
                moveCountText.text = $"Moves: {count}";
            }
        }

        private void UpdateScore(int score)
        {
            if (scoreText != null)
            {
                scoreText.text = $"Score: {score}";
            }
        }

        #endregion

        #region 面板管理

        private void HideAllPanels()
        {
            HidePanel(pausePanel);
            HidePanel(completePanel);
            HidePanel(failPanel);
        }

        private void ShowPanel(GameObject panel)
        {
            if (panel != null)
                panel.SetActive(true);
        }

        private void HidePanel(GameObject panel)
        {
            if (panel != null)
                panel.SetActive(false);
        }

        #endregion
    }
}
