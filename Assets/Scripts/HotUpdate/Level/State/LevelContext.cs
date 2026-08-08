using System.Collections.Generic;
using Game.Level.Data;
using Game.Level.Runtime;
using UnityEngine;

namespace Game.Level.State
{
    /// <summary>Shared runtime data for one loaded level.</summary>
    public class LevelContext
    {
        public LevelConfig Config { get; set; }
        public List<RuntimePiece> Pieces { get; } = new List<RuntimePiece>();
        public int LevelNumber { get; set; }
        public MonoBehaviour CoroutineHost { get; set; }

        public void ResetPieces()
        {
            foreach (var piece in Pieces)
                piece.Reset();
        }

        public void Clear()
        {
            Config = null;
            Pieces.Clear();
            LevelNumber = 0;
        }
    }
}
