using UnityEngine;
using Game.Level.Data;

namespace Game.Level.Runtime
{
    public class LevelView : MonoBehaviour
    {
        [SerializeField] private Board board;
        [SerializeField] private PieceMoveManager pieceMoveManager;
        [SerializeField] private LevelUIController uiController;
        [SerializeField] private LevelGoal levelGoal = new LevelGoal();

        private LevelController controller;
        private LevelConfig currentConfig;
        private int currentLevelNumber;
        private bool isPlaying;

        public event System.Action<LevelConfig> OnLevelStarted;
        public event System.Action OnLevelCompleted;
        public event System.Action OnLevelFailed;

        public LevelConfig CurrentConfig => currentConfig;
        public BoardStateManager BoardState => controller?.BoardState;
        public bool IsPlaying => isPlaying;
        public int CurrentLevelNumber => currentLevelNumber;
        public LevelController Controller => controller;

        private void OnDestroy()
        {
            CleanupLevel();
        }

        public void LoadLevel(LevelConfig config, int levelNumber)
        {
            if (config == null)
            {
                Debug.LogError("[LevelView] Cannot load null config");
                return;
            }

            currentConfig = config;
            currentLevelNumber = levelNumber;

            var goalManager = new LevelGoalManager(levelGoal);
            controller = new LevelController(config, goalManager);
            goalManager.Initialize(controller.BoardState);
            board.BuildLayout(config, controller.BoardState);
            InitializeManagers();

            Debug.Log($"[LevelView] Level {levelNumber} loaded: {config.levelId}");
        }

        public void StartLevel()
        {
            if (currentConfig == null || controller == null)
            {
                Debug.LogError("[LevelView] Cannot start level: not loaded");
                return;
            }

            isPlaying = true;
            SetInputEnabled(true);
            controller.GoalManager.StartTracking();
            CheckGameOver();
            uiController?.OnLevelStart();
            OnLevelStarted?.Invoke(currentConfig);
        }

        public void PauseLevel()
        {
            if (!isPlaying)
                return;

            isPlaying = false;
            SetInputEnabled(false);
            uiController?.OnLevelPause();
        }

        public void ResumeLevel()
        {
            if (isPlaying)
                return;

            isPlaying = true;
            SetInputEnabled(true);
            uiController?.OnLevelResume();
        }

        public void CompleteLevel()
        {
            if (!isPlaying)
                return;

            isPlaying = false;
            SetInputEnabled(false);
            controller?.GoalManager.StopTracking();
            uiController?.OnLevelComplete();
            OnLevelCompleted?.Invoke();
        }

        public void FailLevel()
        {
            if (!isPlaying)
                return;

            isPlaying = false;
            SetInputEnabled(false);
            controller?.GoalManager.StopTracking();
            uiController?.OnLevelFail();
            OnLevelFailed?.Invoke();
        }

        public void ResetLevel()
        {
            if (currentConfig == null)
                return;

            LoadLevel(currentConfig, currentLevelNumber);
            StartLevel();
        }

        public void CleanupLevel()
        {
            isPlaying = false;
            SetInputEnabled(false);
            controller?.GoalManager.StopTracking();
            currentConfig = null;
            controller = null;
        }

        private void InitializeManagers()
        {
            if (pieceMoveManager != null)
            {
                pieceMoveManager.Initialize(controller, board);
                pieceMoveManager.OnPieceSelected -= HandlePieceSelected;
                pieceMoveManager.OnPieceDeselected -= HandlePieceDeselected;
                pieceMoveManager.OnMoveExecuted -= HandleMoveExecuted;
                pieceMoveManager.OnPieceSelected += HandlePieceSelected;
                pieceMoveManager.OnPieceDeselected += HandlePieceDeselected;
                pieceMoveManager.OnMoveExecuted += HandleMoveExecuted;
            }

            controller.GoalManager.OnGoalCompleted += HandleGoalCompleted;
            controller.GoalManager.OnGoalFailed += HandleGoalFailed;
            uiController?.Initialize(this);
        }

        private void SetInputEnabled(bool enabled)
        {
            pieceMoveManager?.EnableInput(enabled);
        }

        private void HandleMoveExecuted(MoveExecutionResult result)
        {
            if (!isPlaying || result == null || !result.Success)
                return;

            board?.UpdatePieceVisual(result);
            uiController?.OnPieceMoved();
            CheckGameOver();
        }

        private void CheckGameOver()
        {
            if (!isPlaying || controller == null)
                return;

            if (controller.IsVictory())
                CompleteLevel();
            else if (controller.IsDefeat())
                FailLevel();
        }

        private void HandleGoalCompleted() => CheckGameOver();
        private void HandleGoalFailed() => CheckGameOver();

        private void HandlePieceSelected(IPiece piece)
        {
            board?.SetPieceSelected(piece, true);
            board?.ShowMoveableEffects(piece, BoardState);
        }

        private void HandlePieceDeselected(IPiece piece)
        {
            board?.RefreshPieceStates();
            board?.HideMoveableEffects();
        }

        public Board GetBoard() => board;
        public LevelGoalManager GetGoalManager() => controller?.GoalManager;
        public LevelUIController GetUIController() => uiController;
    }
}

