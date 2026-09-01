using System.Collections.Generic;
using QFramework;
using UnityEngine;

/// <summary>
/// 贝塞尔工具曲线,应该是贝塞尔曲线系统与外部交互的唯一接口
/// </summary>
public class BezierTools : MonoSingleton<BezierTools>
{
    /// <summary>
    /// 所有移动路径
    /// </summary>
    private List<MovePath> movePaths = new List<MovePath>();

    public void OnPause(bool isPause)
    {
    }
    public void Update()
    {
        for (int i = movePaths.Count - 1; i >= 0; i--)
        {
            movePaths[i].OnUpdate();
        }
    }

    private void OnDrawGizmos()
    {
        for (int i = movePaths.Count - 1; i >= 0; i--)
        {
            movePaths[i].OnDrawGizmos();
        }
    }

    /// <summary>
    /// 播放贝塞尔运动曲线
    /// </summary>
    /// <param name="target">运动目标</param>
    /// <param name="startPos">运动开始位置</param>
    /// <param name="endPos">运动结束位置</param>
    /// <param name="during">运动持续时间</param>
    /// <param name="delay">延迟运动</param>
    /// <param name="animationCurveType">运动速度曲线类型</param>
    /// <param name="moveType">移动路线类型</param>
    /// <param name="callback">移动结束回调</param>
    public void PlayBezier(GameObject target, Vector3 startPos, Vector3 endPos, float during, float delay = 0,
                                   AnimationCurveType animationCurveType = AnimationCurveType.Liner,
                                   MoveType moveType = MoveType.None, OnBezierMoveOverCallBack callback = null)
    {
        float ratio = 0.3f;
        float offset = 1.5f;
        float angle = 240;

        AnimationCurve SpeedCurve = GetCurveByType(animationCurveType);

        switch (moveType)
        {
            case MoveType.Normal:
                ratio = 0.3f;
                offset = 1.5f;
                angle = 240;
                break;
            case MoveType.LeftOutSide:
                ratio = 0.4f;
                offset = 3.0f;
                angle = 120;
                break;
            case MoveType.RightOutSide:
                ratio = 0.4f;
                offset = 3.0f;
                angle = 300;
                break;
            case MoveType.TopOutSide:
                ratio = 0.4f;
                offset = 2.5f;
                angle = 90; // 从上方向下
                break;
            case MoveType.BottomOutSide:
                ratio = 0.4f;
                offset = 2.5f;
                angle = 270; // 从下方向上
                break;
            case MoveType.Bounce:
                // 弹跳效果需要更复杂的曲线，这里简化处理
                ratio = 0.5f;
                offset = 2.0f;
                angle = 180;
                break;
            case MoveType.None:
            default:
                ratio = 0.5f;
                offset = 0.5f;
                angle = 180;
                break;
        }
        PlayBezier(target, startPos, endPos, during, delay, SpeedCurve, callback, ratio, offset, angle);
    }

    /// <summary>
    /// 播放贝塞尔运动曲线(三点)
    /// </summary>
    /// <param name="target">运动目标</param>
    /// <param name="startPos">运动开始位置</param>
    /// <param name="endPos">运动结束位置</param>
    /// <param name="during">运动持续时间</param>
    /// <param name="delay">延迟运动</param>
    /// <param name="SpeedCurve">速度曲线</param>
    /// <param name="callback">运动结束回调</param>
    public void PlayBezier(GameObject target, Vector3 startPos, Vector3 midPos, Vector3 endPos,
        float during, float delay, AnimationCurve SpeedCurve, OnBezierMoveOverCallBack callback = null)
    {
        if (SpeedCurve == null)
        {
            SpeedCurve = GetLinerCurve();
        }

        List<Vector3> points = new List<Vector3> { startPos, midPos, endPos };

        //实例化一条移动路径
        MovePath movePath = new MovePath(target, points, during, delay, SpeedCurve, callback, OnMoveCompleteCallBack);
        AddMovePath(target, movePath);
    }

    public void PlayBezier(GameObject target, Vector3[] paths,
        float during, float delay, AnimationCurveType animationCurveType = AnimationCurveType.Liner, OnBezierMoveOverCallBack callback = null)
    {
        AnimationCurve SpeedCurve = GetCurveByType(animationCurveType);

        List<Vector3> points = new List<Vector3>(paths);

        //实例化一条移动路径
        MovePath movePath = new MovePath(target, points, during, delay, SpeedCurve, callback, OnMoveCompleteCallBack);
        AddMovePath(target, movePath);
    }

    private void OnMoveCompleteCallBack(MovePath movePath)
    {
        movePaths.Remove(movePath);
    }

    /// <summary>
    /// 播放贝塞尔运动曲线(目前不对外开放  如果有需求权限访问修饰符可以变成public)
    /// </summary>
    /// <param name="target">运动目标</param>
    /// <param name="startPos">运动开始位置</param>
    /// <param name="endPos">运动结束位置</param>
    /// <param name="during">运动持续时间</param>
    /// <param name="delay">延迟运动</param>
    /// <param name="SpeedCurve">速度曲线</param>
    /// <param name="callback">运动结束回调</param>
    /// <param name="ratio">选择点的位置 占 总线段的比例</param>
    /// <param name="offset">选点距离中间点的偏移</param>
    /// <param name="angle">偏移角度</param>
    private void PlayBezier(GameObject target, Vector3 startPos, Vector3 endPos, float during, float delay,
        AnimationCurve SpeedCurve, OnBezierMoveOverCallBack callback, float ratio, float offset, float angle)
    {
        //根据参数获取线上的点
        List<Vector3> points = GetPoints(startPos, endPos, ratio, offset, angle);

        //实例化一条移动路径
        MovePath movePath = new MovePath(target, points, during, delay, SpeedCurve, callback, OnMoveCompleteCallBack);
        AddMovePath(target, movePath);
    }

    private void AddMovePath(GameObject target, MovePath movePath)
    {
        MovePath existPath = movePaths.Find((path) => path.Target == target);
        if (existPath != null)
        {
            existPath.Stop();
        }

        movePaths.Add(movePath);
        movePath.Play();
    }

    private AnimationCurve GetCurveByType(AnimationCurveType animationCurveType)
    {
        AnimationCurve SpeedCurve;
        switch (animationCurveType)
        {
            case AnimationCurveType.Liner:
                SpeedCurve = GetLinerCurve();
                break;
            case AnimationCurveType.EaseIn:
                SpeedCurve = GetEaseInCurve();
                break;
            case AnimationCurveType.EaseOut:
                SpeedCurve = GetEaseOutCurve();
                break;
            case AnimationCurveType.EaseInOut:
                SpeedCurve = GetEaseInOutCurve();
                break;
            case AnimationCurveType.EaseInCubic:
                SpeedCurve = GetEaseInCubicCurve();
                break;
            case AnimationCurveType.EaseInQuart:
                SpeedCurve = GetEaseInQuartCurve();
                break;
            case AnimationCurveType.EaseInQuint:
                SpeedCurve = GetEaseInQuintCurve();
                break;
            case AnimationCurveType.HeavyEaseIn:
                SpeedCurve = GetHeavyEaseInCurve();
                break;
            default:
                SpeedCurve = GetLinerCurve();
                break;
        }
        return SpeedCurve;
    }

    // 获取Liner效果
    public AnimationCurve GetLinerCurve()
    {
        return new AnimationCurve(
            new Keyframe(0f, 0f),
            new Keyframe(1, 1)
        );
    }

    // 获取EaseInOut效果
    public AnimationCurve GetEaseInOutCurve()
    {
        return new AnimationCurve(
            new Keyframe(0f, 0f, 0f, 2f),    // 起始点：平滑加速
            new Keyframe(0.5f, 1f, 0f, 0f),  // 中间点：达到顶峰后开始减速
            new Keyframe(1f, 0f, -2f, 0f)    // 结束点：平滑减速
        );
    }

    // 获取EaseIn效果
    public AnimationCurve GetEaseInCurve()
    {
        return new AnimationCurve(
            new Keyframe(0f, 0f, 0f, 2f), // 起始点：初始速度低
            new Keyframe(1f, 1f, 0f, 0f)  // 结束点：加速达到目标值
        );
    }

    // 获取EaseOut效果
    public AnimationCurve GetEaseOutCurve()
    {
        return new AnimationCurve(
            new Keyframe(0f, 0f, 2f, 0f), // 起始点：初始速度高
            new Keyframe(1f, 1f, 0f, 0f)  // 结束点：平滑减速
        );
    }

    // 获取 EaseInCubic 曲线 (y = x^3)
    public AnimationCurve GetEaseInCubicCurve()
    {
        return new AnimationCurve(
            new Keyframe(0f, 0f, 0f, 0f),
            new Keyframe(0.25f, 0.0156f, 0.1875f, 0.1875f),
            new Keyframe(0.5f, 0.125f, 0.75f, 0.75f),
            new Keyframe(0.75f, 0.4219f, 1.6875f, 1.6875f),
            new Keyframe(1f, 1f, 3f, 0f)
        );
    }

    // 获取 EaseInQuart 曲线 (y = x^4)
    public AnimationCurve GetEaseInQuartCurve()
    {
        return new AnimationCurve(
            new Keyframe(0f, 0f, 0f, 0f),
            new Keyframe(0.25f, 0.0039f, 0.0625f, 0.0625f),
            new Keyframe(0.5f, 0.0625f, 0.5f, 0.5f),
            new Keyframe(0.75f, 0.3164f, 1.6875f, 1.6875f),
            new Keyframe(1f, 1f, 4f, 0f)
        );
    }

    // 获取 EaseInQuint 曲线 (y = x^5)
    public AnimationCurve GetEaseInQuintCurve()
    {
        return new AnimationCurve(
            new Keyframe(0f, 0f, 0f, 0f),
            new Keyframe(0.25f, 0.001f, 0.0195f, 0.0195f),
            new Keyframe(0.5f, 0.03125f, 0.3125f, 0.3125f),
            new Keyframe(0.75f, 0.2373f, 1.582f, 1.582f),
            new Keyframe(1f, 1f, 5f, 0f)
        );
    }

    // BezierTools.cs
    /// <summary>
    /// 获取 极其缓慢 -> 瞬间提速 曲线 (使用更极端的 Keyframe 设置)
    /// </summary>
    public AnimationCurve GetHeavyEaseInCurve()
    {
        // 目标: 
        // 1. 起点 (0, 0) 的出切线斜率必须是 0 (Flat/Broken)，确保初始速度为零。
        // 2. 曲线在 0.8f 之前必须非常平坦。

        // Keyframe(time, value, inTangent, outTangent)
        return new AnimationCurve(
            // Keyframe 0: 起点，速度为 0 (Out Tangent = 0)
            new Keyframe(0f, 0f, 0f, 0f),

            // Keyframe 1: 中间点。在 70% 的时间里，只完成了 5% 的路程 (更加缓慢)
            // 注意：将 inTangent 和 outTangent 设置为 0f (Flat) 来保持平坦。
            new Keyframe(0.7f, 0.05f, 0f, 0f),

            // Keyframe 2: 临界点。在 95% 的时间里，完成了 10% 的路程 (开始加速)
            // 这里的斜率开始变陡。
            new Keyframe(0.95f, 0.1f),

            // Keyframe 3: 终点 (1, 1)，入切线设置为一个大值 (例如 5.0) 来保证最后冲刺极快
            new Keyframe(1f, 1f, 5.0f, 0f)
        );
    }

    // 注意：如果您的 Keyframe 模拟效果不够，可以直接使用 Unity 内置的方法创建，
    // 但既然您有自己的工具，这个自定义 Keyframe 方案更加灵活。

    /// <summary>
    /// 设置目标点
    /// </summary>
    /// <param name="startPos">开始点</param>
    /// <param name="endPos">结束点</param>
    /// <param name="ratio">选择点的位置 占 总线段的比例</param>
    /// <param name="offset">选点距离中间点的偏移</param>
    /// <param name="angle">偏移角度</param>
    /// <returns></returns>
    private List<Vector3> GetPoints(Vector3 startPos, Vector3 endPos, float ratio, float offset, float angle)
    {
        List<Vector3> m_Point = new List<Vector3> { startPos, endPos };
        for (int j = 0; j < 2; j++)
        {
            List<Vector3> trs = new List<Vector3>();
            foreach (var item in m_Point)
            {
                trs.Add(item);
            }

            for (int i = trs.Count - 1; i >= 1; i--)
            {
                Vector3 mid = GetMiddlePointByDir(trs[i], trs[i - 1], ratio, offset, angle);
                m_Point.Insert(i, mid);
            }
        }
        return m_Point;
    }

    /// <summary>
    /// 获取中间点 偏移半径 选择方向
    /// </summary>
    /// <param name="start">开始点</param>
    /// <param name="end">结束点</param>
    /// <param name="ratio">选择点的位置 占 总线段的比例</param>
    /// <param name="offset">选点距离中间点的偏移</param>
    /// <param name="angle">偏移角度</param>
    /// <returns>选点的坐标</returns>
    private Vector3 GetMiddlePointByDir(Vector3 start, Vector3 end, float ratio, float offset, float angle)
    {
        Vector3 center = Vector3.Lerp(start, end, ratio);
        Vector3 normal = (end - start).normalized;
        Quaternion quaTurn = Quaternion.Euler(0, 0, -angle);  //Z轴旋转 :面朝Z轴正向的顺时针旋转
        normal = quaTurn * normal;
        Vector3 middle = center + normal * offset;
        return middle;
    }

}

/// <summary>
/// 贝塞尔曲线运动结束回调
/// </summary>
public delegate void OnBezierMoveOverCallBack();

/// <summary>
/// 运动速度曲线类型
/// 可以从dotween维护
/// </summary>
public enum AnimationCurveType
{
    None,
    Liner,
    EaseIn,
    EaseOut,
    EaseInOut,
    EaseInCubic,
    EaseInQuart,
    EaseInQuint,
    HeavyEaseIn, //极其缓慢,然后瞬间提速
}

/// <summary>
/// 运动曲线类型
/// </summary>
public enum MoveType
{
    None,
    /// <summary>
    /// 正常（临时  后期维护拓展）
    /// </summary>
    Normal,
    /// <summary>
    /// 从左边飞
    /// </summary>
    LeftOutSide,
    /// <summary>
    /// 从右边飞
    /// </summary>
    RightOutSide,
    /// <summary>
    /// 从上方飞入
    /// </summary>
    TopOutSide,
    /// <summary>
    /// 从下方飞入
    /// </summary>
    BottomOutSide,
    /// <summary>
    /// 弹跳效果
    /// </summary>
    Bounce,
    /// <summary>
    /// 波浪效果
    /// </summary>
    Wave,
    /// <summary>
    /// 螺旋效果
    /// </summary>
    Spiral
}