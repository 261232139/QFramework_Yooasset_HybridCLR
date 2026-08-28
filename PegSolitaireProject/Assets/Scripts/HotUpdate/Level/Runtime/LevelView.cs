/****************************************************************************
 * Copyright (c) 2024 Game Project UNDER MIT License
 *
 * 关卡视图（总指挥官）
 *
 * 作为关卡的总协调器，负责组件间的通信
 ****************************************************************************/

using UnityEngine;
using Game.Level.Data;

namespace Game.Level.Runtime
{
    /// <summary>
    /// 关卡视图 - 总指挥官
    /// 负责关卡的整体协调、组件通信、生命周期管理
    /// </summary>
    public class LevelView : MonoBehaviour
    {
        [Header("View Components")]
        [SerializeField] private Board board;
        [SerializeField] private LevelInputHandler inputHandler;
        [SerializeField] private LevelUIController uiController;

        [Header("Goal Settings")]
        [SerializeField] private LevelGoal levelGoal = new LevelGoal();

        // 逻辑控制器（运行时创建）
        private LevelController controller;
        
        private LevelConfig currentConfig;
        private int currentLevelNumber;
        private bool isPlaying;

        // 事件
        public event System.Action<LevelConfig> OnLevelStarted;
        public event System.Action OnLevelCompleted;
        public event System.Action OnLevelFailed;

        // 公共属性
        public LevelConfig CurrentConfig => currentConfig;
        public BoardStateManager BoardState => controller?.BoardState;
        public bool IsPlaying => isPlaying;
        public int CurrentLevelNumber => currentLevelNumber;
        public LevelController Controller => controller;

        private void Awake()
        {
            InitializeComponents();
        }

        private void OnDestroy()
        {
            CleanupLevel();
        }

        #region 关卡生命周期

        /// <summary>
        /// 加载关卡
        /// </summary>
        public void LoadLevel(LevelConfig config, int levelNumber)
        {
            if (config == null)
            {
                Debug.LogError("[LevelView] Cannot load null config");
                return;
            }

            currentConfig = config;
            currentLevelNumber = levelNumber;

            // 1. 创建逻辑控制器
            var goalManager = new LevelGoalManager(levelGoal);
            controller = new LevelController(config, goalManager);
            goalManager.Initialize(controller.BoardState);

            // 2. Board 只负责布局
            board.BuildLayout(config, controller.BoardState);

            // 3. 初始化各个组件
            InitializeManagers();

            Debug.Log($"[LevelView] Level {levelNumber} loaded: {config.levelId}");
        }

        /// <summary>
        /// 开始关卡
        /// </summary>
        public void StartLevel()
        {
            if (currentConfig == null || controller == null)
            {
                Debug.LogError("[LevelView] Cannot start level: not loaded");
                return;
            }

            isPlaying = true;

            // 启用输入
            if (inputHandler != null)
                inputHandler.EnableInput(true);

            // 开始跟踪目标
            controller.GoalManager.StartTracking();

            CheckGameOver();

            // 更新 UI
            if (uiController != null)
                uiController.OnLevelStart();

            OnLevelStarted?.Invoke(currentConfig);
            Debug.Log($"[LevelView] Level started: {currentConfig.levelId}");
        }

        /// <summary>
        /// 暂停关卡
        /// </summary>
        public void PauseLevel()
        {
            if (!isPlaying) return;

            isPlaying = false;

            if (inputHandler != null)
                inputHandler.EnableInput(false);

            if (uiController != null)
                uiController.OnLevelPause();

            Debug.Log("[LevelView] Level paused");
        }

        /// <summary>
        /// 恢复关卡
        /// </summary>
        public void ResumeLevel()
        {
            if (isPlaying) return;

            isPlaying = true;

            if (inputHandler != null)
                inputHandler.EnableInput(true);

            if (uiController != null)
                uiController.OnLevelResume();

            Debug.Log("[LevelView] Level resumed");
        }

        /// <summary>
        /// 完成关卡
        /// </summary>
        public void CompleteLevel()
        {
            if (!isPlaying) return;

            isPlaying = false;

            if (inputHandler != null)
                inputHandler.EnableInput(false);

            controller?.GoalManager.StopTracking();

            if (uiController != null)
                uiController.OnLevelComplete();

            OnLevelCompleted?.Invoke();
            Debug.Log("[LevelView] Level completed!");
        }

        /// <summary>
        /// 失败关卡
        /// </summary>
        public void FailLevel()
        {
            if (!isPlaying) return;

            isPlaying = false;

            if (inputHandler != null)
                inputHandler.EnableInput(false);

            controller?.GoalManager.StopTracking();

            if (uiController != null)
                uiController.OnLevelFail();

            OnLevelFailed?.Invoke();
            Debug.Log("[LevelView] Level failed!");
        }

        /// <summary>
        /// 重置关卡
        /// </summary>
        public void ResetLevel()
        {
            if (currentConfig == null) return;

            Debug.Log("[LevelView] Resetting level...");
            LoadLevel(currentConfig, currentLevelNumber);
            StartLevel();
        }

        /// <summary>
        /// 清理关卡
        /// </summary>
        public void CleanupLevel()
        {
            isPlaying = false;

            if (inputHandler != null)
                inputHandler.EnableInput(false);

            controller?.GoalManager.StopTracking();

            currentConfig = null;
            controller = null;

            Debug.Log("[LevelView] Level cleaned up");
        }

        #endregion

        #region 初始化

        private void InitializeComponents()
        {
            if (board == null)
                board = GetComponentInChildren<Board>();

            if (inputHandler == null)
                inputHandler = GetComponentInChildren<LevelInputHandler>();

            if (uiController == null)
                uiController = GetComponentInChildren<LevelUIController>();
        }

        private void InitializeManagers()
        {
            // 初始化输入处理器
            if (inputHandler != null)
            {
                inputHandler.Initialize(this, board);
                inputHandler.OnMoveRequested -= HandleMoveRequested;
                inputHandler.OnPieceSelected -= HandlePieceSelected;
                inputHandler.OnPieceDeselected -= HandlePieceDeselected;
                inputHandler.OnMoveRequested += HandleMoveRequested;
                inputHandler.OnPieceSelected += HandlePieceSelected;
                inputHandler.OnPieceDeselected += HandlePieceDeselected;
            }

            // 订阅目标事件
            if (controller != null)
            {
                controller.GoalManager.OnGoalCompleted += HandleGoalCompleted;
                controller.GoalManager.OnGoalFailed += HandleGoalFailed;
            }

            // 初始化 UI
            if (uiController != null)
            {
                uiController.Initialize(this);
            }
        }

        #endregion

        #region 游戏逻辑处理

        /// <summary>
        /// 处理移动请求（从输入处理器来）
        /// </summary>
        public void HandleMoveRequested(IPiece piece, GridPosition from, GridPosition to)
        {
            if (!isPlaying || controller == null)
                return;

            // 通过 Controller 执行移动
            var result = controller.ExecuteMove(piece, to);

            if (result.Success)
            {
                // 更新视图
                board.UpdatePieceVisual(result);
                
                // 更新 UI
                if (uiController != null)
                    uiController.OnPieceMoved();

                // 检查游戏结束
                CheckGameOver();
            }
            else
            {
                Debug.LogWarning($"[LevelView] Move failed: {result.ErrorMessage}");
            }
        }

        /// <summary>
        /// 检查游戏是否结束
        /// </summary>
        private void CheckGameOver()
        {
            if (!isPlaying || controller == null)
                return;

            if (controller.IsVictory())
            {
                CompleteLevel();
            }
            else if (controller.IsDefeat())
            {
                Debug.Log("[LevelView] No valid jump moves remain.");
                FailLevel();
            }
        }

        private void HandleGoalCompleted()
        {
            CheckGameOver();
        }

        private void HandleGoalFailed()
        {
            CheckGameOver();
        }

        private void HandlePieceSelected(IPiece piece)
        {
            board?.SetPieceSelected(piece, true);
            board?.ShowMoveableEffects(piece, BoardState);
        }

        private void HandlePieceDeselected()
        {
            board?.RefreshPieceStates();
            board?.HideMoveableEffects();
        }

        #endregion

        #region 公共接口

        public Board GetBoard() => board;
        public LevelInputHandler GetInputHandler() => inputHandler;
        public LevelGoalManager GetGoalManager() => controller?.GoalManager;
        public LevelUIController GetUIController() => uiController;

        #endregion
    }
}
