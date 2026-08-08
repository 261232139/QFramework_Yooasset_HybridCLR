using Game.Level.Data;
using UnityEngine;

namespace Game.Level.Runtime
{
    /// <summary>Visual representation of one playable board cell.</summary>
    public class MapGrid : MonoBehaviour
    {
        [SerializeField] private GameObject node;
        [SerializeField] private GameObject arrow;
        [SerializeField] private Transform pieceContainer;

        public GridPosition Position { get; private set; }
        public PieceData Piece { get; private set; }
        public GameObject PieceObject => currentPieceObject;
        
        private GameObject currentPieceObject;

        private void Awake() => BindReferences();

        public void Initialize(GridPosition position, PieceData piece)
        {
            BindReferences();

            Position = position;
            Piece = piece;
            gameObject.name = $"MapGrid ({position.x}, {position.y})";

            var hasPiece = piece != null;
            if (node != null)
                node.SetActive(hasPiece);

            if (arrow != null)
                arrow.SetActive(hasPiece && piece.isMovable);
        }

        /// <summary>设置当前格子上的棋子对象</summary>
        public void SetPieceObject(GameObject pieceObj)
        {
            currentPieceObject = pieceObj;
            
            // 更新 node 和 arrow 的可见性
            var hasPiece = pieceObj != null;
            if (node != null)
                node.SetActive(hasPiece);
                
            if (arrow != null && Piece != null)
                arrow.SetActive(hasPiece && Piece.isMovable);
        }

        /// <summary>获取当前格子上的棋子对象</summary>
        public GameObject GetPieceObject() => currentPieceObject;

        /// <summary>检查格子是否有棋子</summary>
        public bool HasPiece() => currentPieceObject != null;

        private void BindReferences()
        {
            if (node == null)
            {
                var nodeTransform = transform.Find("Node");
                if (nodeTransform != null)
                    node = nodeTransform.gameObject;
            }

            if (arrow == null && node != null)
            {
                var arrowTransform = node.transform.Find("Arrow");
                if (arrowTransform != null)
                    arrow = arrowTransform.gameObject;
            }

            if (pieceContainer == null)
            {
                var containerTransform = transform.Find("PieceContainer");
                if (containerTransform != null)
                    pieceContainer = containerTransform;
                else
                    pieceContainer = transform;
            }
        }
    }
}
