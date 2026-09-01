/****************************************************************************
 * Copyright (c) 2024 Game Project UNDER MIT License
 *
 * 普通棋子类
 *
 * 实现基础的棋子类型
 ****************************************************************************/

using Game.Level.Data;
using UnityEngine;

namespace Game.Level.Runtime
{
    /// <summary>
    /// 普通棋子
    /// 遵循基础移动规则：仅能上下左右移动，必须跨越一个棋子
    /// </summary>
    public class NormalPiece : PieceBase
    {
        public NormalPiece(PieceData config, GameObject visualObject = null) 
            : base(config, visualObject)
        {
        }

        // 普通棋子使用基类的默认移动规则，不需要额外实现
    }

    /// <summary>
    /// Peg 棋子（钉子）
    /// </summary>
    public class PegPiece : PieceBase
    {
        public PegPiece(PieceData config, GameObject visualObject = null) 
            : base(config, visualObject)
        {
        }
    }

    /// <summary>
    /// Gem 棋子（宝石）
    /// </summary>
    public class GemPiece : PieceBase
    {
        public GemPiece(PieceData config, GameObject visualObject = null) 
            : base(config, visualObject)
        {
        }
    }

    /// <summary>
    /// Stone 棋子（石头）
    /// </summary>
    public class StonePiece : PieceBase
    {
        public StonePiece(PieceData config, GameObject visualObject = null) 
            : base(config, visualObject)
        {
        }
    }

    /// <summary>
    /// 棋子工厂
    /// 根据配置创建对应类型的棋子
    /// </summary>
    public static class PieceFactory
    {
        public static IPiece CreatePiece(PieceData config, GameObject visualObject = null)
        {
            switch (config.pieceType)
            {
                case PieceType.Peg:
                    return new PegPiece(config, visualObject);
                
                case PieceType.Gem:
                    return new GemPiece(config, visualObject);
                
                case PieceType.Stone:
                    return new StonePiece(config, visualObject);
                
                default:
                    Debug.LogWarning($"Unknown piece type: {config.pieceType}, creating NormalPiece");
                    return new NormalPiece(config, visualObject);
            }
        }
    }
}
