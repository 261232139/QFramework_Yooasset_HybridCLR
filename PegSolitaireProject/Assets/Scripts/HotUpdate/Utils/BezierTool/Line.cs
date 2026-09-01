using UnityEngine;

/// <summary>
/// 线段，包含起点和终点
/// </summary>
public struct Line
{
    public Line(Vector3 start, Vector3 end)
    {
        StartPoint = start;
        EndPoint = end;
    }
    /// <summary>
    /// 线段的起点
    /// </summary>
    /// <value>The start point.</value>
    public Vector3 StartPoint
    {
        get;
        set;
    }
    /// <summary>
    /// 线段的终点
    /// </summary>
    /// <value>The end point.</value>
    public Vector3 EndPoint
    {
        get;
        set;
    }
    /// <summary>
    /// 判断一个点是否是自己的起点或者终点
    /// </summary>
    /// <returns><c>true</c>, if me was ised, <c>false</c> otherwise.</returns>
    /// <param name="point">Point.</param>
    public bool isMe(Vector3 point)
    {
        if (StartPoint == point || EndPoint == point)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    /// <summary>
    /// 根据传入的值获取这条线段上的任意一点，T>=0&&T<=1
    /// T=0时返回起点
    /// T=1时返回终点
    /// </summary>
    /// <returns>The point.</returns>
    /// <param name="T">T.</param>
    public Vector3 GetPoint(float T)
    {
        var point = new Vector3();
        if (T < 0)
        {
            T = 0;
        }
        else if (T > 1)
        {
            T = 1;
        }
        point = (EndPoint - StartPoint) * T + StartPoint;
        return point;
    }
}
