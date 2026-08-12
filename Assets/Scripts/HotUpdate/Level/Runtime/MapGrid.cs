using Game.Level.Data;
using UnityEngine;

namespace Game.Level.Runtime
{
    /// <summary>单个有效棋盘格的视图，只管理格子本身及可落点特效。</summary>
    public class MapGrid : MonoBehaviour
    {
        [SerializeField] private GameObject node;
        [SerializeField] private GameObject moveableEffect;
        [SerializeField] private Transform pieceContainer;

        public GridPosition Position { get; private set; }
        public GameObject PieceObject => currentPieceObject;

        private GameObject currentPieceObject;

        private void Awake() => BindReferences();

        public void Initialize(GridPosition position)
        {
            BindReferences();
            Position = position;
            gameObject.name = $"MapGrid ({position.x}, {position.y})";

            if (node != null)
                node.SetActive(true);

            SetMoveableEffect(false);
        }

        public void SetPieceObject(GameObject pieceObj)
        {
            currentPieceObject = pieceObj;
            if (node != null)
                node.SetActive(true);
        }

        public void SetMoveableEffect(bool visible)
        {
            if (moveableEffect != null)
                moveableEffect.SetActive(visible);
        }

        public bool HasPiece() => currentPieceObject != null;

        private void BindReferences()
        {
            if (node == null)
            {
                var nodeTransform = transform.Find("Node");
                if (nodeTransform != null)
                    node = nodeTransform.gameObject;
            }

            if (moveableEffect == null && node != null)
            {
                var effectTransform = node.transform.Find("GridIcon/MoveableEffect");
                if (effectTransform != null)
                    moveableEffect = effectTransform.gameObject;
            }

            if (pieceContainer == null)
            {
                var containerTransform = transform.Find("Node/GridIcon/PieceContainer");
                pieceContainer = containerTransform != null ? containerTransform : transform;
            }
        }
    }
}
