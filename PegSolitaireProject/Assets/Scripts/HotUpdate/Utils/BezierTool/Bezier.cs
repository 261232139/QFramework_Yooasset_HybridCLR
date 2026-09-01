using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 贝塞尔曲线
/// </summary>
public class Bezier
{
    private List<Vector3> m_Points;
    private List<Line> createdLine;
    /// <summary>
    /// Initializes a new instance of the <see cref="MyTools.Bezier"/> class.
    /// </summary>
    /// <param name="points">Points.</param>
    public Bezier(List<Vector3> points)
    {
        if (points.Count < 2)
        {
            throw (new ArgumentException("实例化贝塞尔曲线至少需要2个点"));
        }
        else
        {
            m_Points = new List<Vector3>();
            createdLine = new List<Line>();
            CreateLine(points);
        }
    }
    #region 修改参数的方法
    public void AddPoint(Vector3 point)
    {
        m_Points.Add(point);
        CreateLine(m_Points);
    }

    public void AddPointAt(int index, Vector3 point)
    {
        if (index >= 0 && index < m_Points.Count)
        {
            m_Points.Insert(index, point);
            CreateLine(m_Points);
        }
        else
        {
            throw (new ArgumentOutOfRangeException("索引超出范围"));
        }
    }

    public void RemovePoint(Vector3 point)
    {
        if (m_Points.Count > 2)
        {
            for (int i = 0; i < m_Points.Count; i++)
            {
                if (m_Points[i] == point)
                {
                    m_Points.RemoveAt(i);
                    CreateLine(m_Points);
                }
                else
                {
                    continue;
                }
            }
        }
        else
        {
            Exception ex = new Exception("当前曲线锚点数量已经最低，不能移除锚点");
            throw (ex);
        }
    }

    public void RemovePointAt(int index)
    {
        if (m_Points.Count > 2)
        {
            m_Points.RemoveAt(index);
            CreateLine(m_Points);
        }
        else
        {
            Exception ex = new Exception("当前曲线锚点数量已经最低，不能移除锚点");
            throw (ex);
        }
    }

    public void UpdatePoint(int ListIndex, Vector3 point)
    {
        if (ListIndex < 0)
        {
            throw (new ArgumentException("坐标索引参数错误（取值必须大于0）"));
        }
        else if (ListIndex >= m_Points.Count)
        {
            throw (new ArgumentException("坐标索引参数错误（取值必须x小于曲线顶点的个数）"));
        }
        else
        {
            m_Points[ListIndex] = point;
            CreateLine(m_Points);
        }
    }

    #endregion

    /// <summary>
    /// 根据传入的参数获取曲线上某一点的值
    /// </summary>
    /// <returns>The point.</returns>
    /// <param name="T">取值参数（0-1）.</param>
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
        var bufListLine = createdLine;
        if (bufListLine == null)
        {
            throw (new NullReferenceException("曲线锚点为空"));
        }
        while (bufListLine.Count > 1)
        {

            bufListLine = CaculateResoultLine(bufListLine, T);
        }
        if (bufListLine.Count == 1)
        {
            point = bufListLine[0].GetPoint(T);
        }
        else
        {
            throw (new Exception("Program Error : Current Line Count is:   " + bufListLine.Count));
        }

        return point;

    }
    /// <summary>
    /// 根据当前的线段以及取值参数T，创建新的线段链表（新的链表长度始终等于原始链表长度-1）。
    /// 使用迭代计算的方式降低程序的复杂性
    /// </summary>
    /// <param name="Lines">Lines.</param>
    /// <param name="T">T.</param>
    private List<Line> CaculateResoultLine(List<Line> Lines, float T)
    {
        var ListLine = new List<Line>();
        for (int i = 0; i < Lines.Count - 1; i++)
        {
            var j = i + 1;
            Line bufLine = new Line(Lines[i].GetPoint(T), Lines[j].GetPoint(T));
            ListLine.Add(bufLine);
        }
        return ListLine;
    }
    /// <summary>
    /// 根据已知的锚点依次创建一条连续的折线
    /// </summary>
    /// <param name="points">Points.</param>
    private void CreateLine(List<Vector3> points)
    {
        createdLine = new List<Line>();
        m_Points = points;
        for (int i = 0; i < points.Count; i++)
        {
            var j = i + 1;
            if (j >= points.Count)
            {
                break;
            }
            else
            {
                Line curLine = new Line(points[i], points[j]);
                createdLine.Add(curLine);
            }
        }
    }
}
