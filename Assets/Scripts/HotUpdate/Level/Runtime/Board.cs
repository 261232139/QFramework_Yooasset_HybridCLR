using System.Collections.Generic;
using Game.Level.Data;
using UnityEngine;

namespace Game.Level.Runtime
{
    /// <summary>
    /// 棋盘布局管理器
    /// 只负责棋盘格子和棋子的视觉布局，不处理游戏逻辑
    /// </summary>
    public class Board : MonoBehaviour
    {
        [SerializeField] private MapGrid mapGridTemplate;
        [SerializeField] private Vector2 cellSize = new Vector2(150f, 150f);
        [SerializeField] private Vector2 cellSpacing = new Vector2(10f, 10f);

        [Header("Piece Prefabs")]
        [SerializeField] private GameObject pegPiecePrefab;
        [SerializeField] private GameObject gemPiecePrefab;
        [SerializeField] private GameObject stonePiecePrefab;

        private readonly List<MapGrid> generatedGrids = new List<MapGrid>();
        private readonly Dictionary<GridPosition, MapGrid> gridsByPosition = new Dictionary<GridPosition, MapGrid>();
        private readonly Dictionary<string, GameObject> pieceObjects = new Dictionary<string, GameObject>();
        
        private LevelConfig currentConfig;

        public LevelConfig CurrentConfig => currentConfig;
        public Vector2 CellSize => cellSize;
        public Vector2 CellSpacing => cellSpacing;

        private void Awake()
        {
            ResolveTemplate();
            if (mapGridTemplate != null)
                mapGridTemplate.gameObject.SetActive(false);
        }

        /// <summary>
        /// 构建棋盘布局（只负责视觉呈现）
        /// </summary>
        public void BuildLayout(LevelConfig config)
        {
            if (config == null)
            {
                Debug.LogError("[Board] Cannot build a null level config.");
                return;
            }

            if (!config.Validate(out var error))
            {
                Debug.LogError($"[Board] Cannot build invalid level '{config.levelId}': {error}");
                return;
            }

            ResolveTemplate();
            if (mapGridTemplate == null)
            {
                Debug.LogError("[Board] MapGrid template was not found under Board.");
                return;
            }

            ClearGeneratedGrids();
            
            currentConfig = config;

            var piecesByPosition = new Dictionary<GridPosition, PieceData>();
            foreach (var piece in config.pieces)
                piecesByPosition.Add(piece.position, piece);

            // 生成棋盘格子
            for (var y = 0; y < config.board.height; y++)
            {
                for (var x = 0; x < config.board.width; x++)
                {
                    if (!config.board.IsPlayable(x, y))
                        continue;

                    var position = new GridPosition(x, y);
                    piecesByPosition.TryGetValue(position, out var piece);

                    var grid = Instantiate(mapGridTemplate, transform);
                    grid.GetComponent<RectTransform>().anchoredPosition = GetAnchoredPosition(position, config.board);
                    grid.Initialize(position, piece);
                    grid.gameObject.SetActive(true);

                    generatedGrids.Add(grid);
                    gridsByPosition.Add(position, grid);
                }
            }

            // 生成棋子（从配置数据）
            GeneratePiecesFromConfig(piecesByPosition);
            
            Debug.Log($"[Board] Layout built: {config.board.width}x{config.board.height}, {config.pieces.Count} pieces");
        }

        /// <summary>从配置生成所有棋子的可视化对象</summary>
        private void GeneratePiecesFromConfig(Dictionary<GridPosition, PieceData> piecesByPosition)
        {
            if (currentConfig == null)
                return;

            foreach (var pieceData in piecesByPosition.Values)
            {
                CreatePieceVisual(pieceData);
            }
        }

        /// <summary>创建单个棋子的可视化对象（从配置数据）</summary>
        private void CreatePieceVisual(PieceData pieceData)
        {
            var prefab = GetPiecePrefab(pieceData.pieceType);
            if (prefab == null)
            {
                Debug.LogWarning($"[Board] No prefab for piece type: {pieceData.pieceType}");
                return;
            }

            // 在对应的MapGrid上创建棋子
            if (gridsByPosition.TryGetValue(pieceData.position, out var grid))
            {
                var pieceObj = Instantiate(prefab, grid.transform);
                pieceObj.name = $"{pieceData.pieceType}_{pieceData.id}";
                
                var rectTransform = pieceObj.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.anchoredPosition = Vector2.zero;
                    rectTransform.localScale = Vector3.one;
                }

                pieceObjects[pieceData.id] = pieceObj;
                grid.SetPieceObject(pieceObj);
            }
        }

        /// <summary>根据棋子类型获取预制体</summary>
        private GameObject GetPiecePrefab(PieceType type)
        {
            switch (type)
            {
                case PieceType.Peg:
                    return pegPiecePrefab;
                case PieceType.Gem:
                    return gemPiecePrefab;
                case PieceType.Stone:
                    return stonePiecePrefab;
                default:
                    return pegPiecePrefab;
            }
        }

        /// <summary>
        /// 更新棋子视觉（被动接收来自 LevelView 的指令）
        /// </summary>
        public void UpdatePieceVisual(MoveExecutionResult result)
        {
            if (result == null || !result.Success)
                return;

            var piece = result.MovedPiece;
            var from = result.From;
            var to = result.To;

            // 更新视觉表现
            if (pieceObjects.TryGetValue(piece.Id, out var pieceObj) && 
                gridsByPosition.TryGetValue(from, out var fromGrid) &&
                gridsByPosition.TryGetValue(to, out var toGrid))
            {
                fromGrid.SetPieceObject(null);
                
                pieceObj.transform.SetParent(toGrid.transform);
                var rectTransform = pieceObj.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.anchoredPosition = Vector2.zero;
                }
                
                toGrid.SetPieceObject(pieceObj);
            }

            // 移除被跨越的棋子
            if (result.JumpedPiece != null)
            {
                RemovePieceVisual(result.JumpedPiece.Id);
            }
        }

        /// <summary>移除棋子视觉对象</summary>
        public void RemovePieceVisual(string pieceId)
        {
            if (pieceObjects.TryGetValue(pieceId, out var pieceObj))
            {
                // 找到并清除对应格子的引用
                foreach (var grid in generatedGrids)
                {
                    if (grid.PieceObject == pieceObj)
                    {
                        grid.SetPieceObject(null);
                        break;
                    }
                }

                Destroy(pieceObj);
                pieceObjects.Remove(pieceId);
            }
        }
        
        /// <summary>获取指定位置的 MapGrid</summary>
        public MapGrid GetGridAt(GridPosition position)
        {
            gridsByPosition.TryGetValue(position, out var grid);
            return grid;
        }

        public bool TryGetGrid(GridPosition position, out MapGrid grid) => gridsByPosition.TryGetValue(position, out grid);

        private Vector2 GetAnchoredPosition(GridPosition position, BoardData board)
        {
            var step = cellSize + cellSpacing;
            var originX = -(board.width - 1) * step.x * 0.5f;
            var originY = (board.height - 1) * step.y * 0.5f;
            return new Vector2(originX + position.x * step.x, originY - position.y * step.y);
        }

        private void ResolveTemplate()
        {
            if (mapGridTemplate == null)
                mapGridTemplate = GetComponentInChildren<MapGrid>(true);
        }

        private void ClearGeneratedGrids()
        {
            foreach (var grid in generatedGrids)
            {
                if (grid == null)
                    continue;

                if (Application.isPlaying)
                    Destroy(grid.gameObject);
                else
                    DestroyImmediate(grid.gameObject);
            }

            generatedGrids.Clear();
            gridsByPosition.Clear();
            
            // 清理棋子对象
            foreach (var pieceObj in pieceObjects.Values)
            {
                if (pieceObj != null)
                {
                    if (Application.isPlaying)
                        Destroy(pieceObj);
                    else
                        DestroyImmediate(pieceObj);
                }
            }
            pieceObjects.Clear();
            
            currentConfig = null;
        }
    }
}
