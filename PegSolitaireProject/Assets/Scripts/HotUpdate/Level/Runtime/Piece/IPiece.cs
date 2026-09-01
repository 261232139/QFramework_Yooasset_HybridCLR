using System.Collections.Generic;
using Game.Level.Data;
using UnityEngine;

namespace Game.Level.Runtime
{
    public struct MoveResult
    {
        public bool IsValid;
        public string ErrorMessage;
        public GridPosition TargetPosition;
        public GridPosition JumpedPosition;
        public static MoveResult Success(GridPosition target, GridPosition jumped) => new MoveResult { IsValid = true, TargetPosition = target, JumpedPosition = jumped };
        public static MoveResult Fail(string error) => new MoveResult { IsValid = false, ErrorMessage = error };
    }

    public interface IPiece
    {
        string Id { get; }
        PieceType PieceType { get; }
        GridPosition Position { get; }
        bool IsMovable { get; }
        IReadOnlyList<IMoveSkill> MoveSkills { get; }
        PieceTrait Traits { get; }
        bool CanBeJumped { get; }
        bool IsRescueTarget { get; }
        bool IsSelected { get; set; }
        MoveResult ValidateMove(GridPosition from, GridPosition to, IBoardState board);
        IEnumerable<MoveOption> GetValidMoves(IBoardState board);
        void MoveTo(GridPosition newPosition);
        void Reset();
        GameObject GetVisualObject();
    }

    public interface IBoardState
    {
        bool IsInBounds(GridPosition position);
        bool HasCell(GridPosition position);
        IPiece GetPieceAt(GridPosition position);
        bool HasPieceAt(GridPosition position);
        int Width { get; }
        int Height { get; }
    }
}
