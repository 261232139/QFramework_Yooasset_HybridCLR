using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// 基于贝塞尔曲线的一条移动路径
/// </summary>
public class MovePath
{
    /// <summary>
    /// 运动目标对象
    /// </summary>
    GameObject target;

    /// <summary>
    /// 路点信息
    /// </summary>
    List<Vector3> m_points;

    /// <summary>
    /// 整个运动持续多久
    /// </summary>
    float during;

    /// <summary>
    /// 当前是否播放中
    /// </summary>
    bool isPlaying;

    /// <summary>
    /// 当前运动时间
    /// </summary>
    float nowTime = 0;

    /// <summary>
    /// 延迟播放
    /// </summary>
    float delay;

    /// <summary>
    /// 当前运动轨迹的贝塞尔对象
    /// </summary>
    Bezier bezierCurve;

    /// <summary>
    /// 贝塞尔运动结束回调
    /// </summary>
    OnBezierMoveOverCallBack onFinishCallBack;

    Action<MovePath> stopCallBack;

    /// <summary>
    /// 当前运动曲线
    /// </summary>
    AnimationCurve SpeedCurve;

    /// <summary>
    /// 运动进度
    /// </summary>
    public float Process
    {
        get
        {
            float process = nowTime / during;
            return Mathf.Clamp01(process); ;
        }
    }

    /// <summary>
    /// 构造函数初始化
    /// </summary>
    /// <param name="target">运动目标</param>
    /// <param name="m_points">路点信息</param>
    /// <param name="during">运动总时间</param>
    /// <param name="delay">延迟时间</param>
    /// <param name="SpeedCurve">运动曲线</param>
    /// <param name="onFinishCallBack">运动结束回调</param>    
    public MovePath(GameObject target, List<Vector3> m_points, float during, float delay, AnimationCurve SpeedCurve, OnBezierMoveOverCallBack onFinishCallBack, Action<MovePath> stopCallBack)
    {
        this.target = target;
        this.m_points = m_points;
        this.during = during;
        this.delay = delay;
        this.SpeedCurve = SpeedCurve;
        this.onFinishCallBack = onFinishCallBack;
        this.stopCallBack = stopCallBack;
    }

    /// <summary>
    /// 心跳
    /// 调用于外部管理工具
    /// </summary>
    public void OnUpdate()
    {
        if (isPlaying && target && bezierCurve != null)
        {
            if (delay > 0)
            {
                delay -= Time.deltaTime;
                return;
            }
            if (Process < 0.99)
            {
                float TimeDelta = Time.deltaTime * SpeedCurve.Evaluate(Process);
                nowTime += Time.deltaTime;// TimeDelta;
                target.transform.position = bezierCurve.GetPoint(Process);
            }
            else
            {
                Stop();
            }
        }
    }

    public void Stop()
    {
        onFinishCallBack?.Invoke();
        stopCallBack?.Invoke(this);

        //初始化数据
        InitData();
    }

    /// <summary>
    /// 绘制
    /// 调用于外部管理工具
    /// </summary>
    public void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        //绘制锚点连线
        for (int i = 0; i < m_points.Count - 1; i++)
        {
            Gizmos.DrawLine(m_points[i], m_points[i + 1]);
        }

        Gizmos.color = Color.red;
        if (bezierCurve == null)
        {
            return;
        }
        //绘制曲线
        for (int i = 0; i < 50; i++)
        {
            var j = i + 1;
            Gizmos.DrawLine(bezierCurve.GetPoint(i / (float)50), bezierCurve.GetPoint(j / (float)50));
        }
    }

    /// <summary>
    /// 初始化数据
    /// </summary>
    private void InitData()
    {
        isPlaying = false;
        target = null;
        bezierCurve = null;
        nowTime = 0;
        m_points.Clear();
        during = 0;
        onFinishCallBack = null;
        delay = 0;
    }

    /// <summary>
    /// 播放
    /// </summary>
    public void Play()
    {
        if (target != null)
        {
            SetUp();
            isPlaying = true;
        }
    }

    /// <summary>
    /// 配置贝塞尔信息数据
    /// </summary>
    private void SetUp()
    {
        List<Vector3> vector3s = new List<Vector3>();
        for (int i = 0; i < m_points.Count; i++)
        {
            vector3s.Add(m_points[i]);
        }
        bezierCurve = new Bezier(vector3s);
    }

    public GameObject Target => target;
}
