using UnityEngine;

namespace Game.Level.Runtime
{
    /// <summary>棋子视图基类：只管理棋子自身的显示状态，不依赖格子对象。</summary>
    public abstract class PieceViewBase : MonoBehaviour
    {
        [SerializeField] private GameObject node;
        [SerializeField] private GameObject selected;
        [SerializeField] private GameObject arrow;

        public IPiece Piece { get; private set; }

        protected virtual void Awake() => BindReferences();

        public void Initialize(IPiece piece)
        {
            Piece = piece;
            BindReferences();
            RefreshState();
        }

        public void RefreshState()
        {
            if (node != null)
                node.SetActive(true);

            SetSelected(Piece != null && Piece.IsSelected);

            if (arrow != null)
                arrow.SetActive(Piece != null && Piece.IsMovable);
        }

        public void SetSelected(bool isSelected)
        {
            if (selected != null)
                selected.SetActive(isSelected);
        }

        private void BindReferences()
        {
            if (node == null)
            {
                var nodeTransform = transform.Find("Node");
                if (nodeTransform != null)
                    node = nodeTransform.gameObject;
            }

            if (selected == null && node != null)
            {
                var selectedTransform = node.transform.Find("Selected");
                if (selectedTransform != null)
                    selected = selectedTransform.gameObject;
            }

            if (arrow == null)
            {
                var arrowTransform = transform.Find("Arrow");
                if (arrowTransform != null)
                    arrow = arrowTransform.gameObject;
            }
        }
    }
}
