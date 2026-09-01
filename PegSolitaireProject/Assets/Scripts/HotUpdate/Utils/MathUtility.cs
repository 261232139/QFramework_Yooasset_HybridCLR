using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using Random = System.Random;
#else
using Random = UnityEngine.Random;
#endif

public static class MathUtility
{
    private const float MIN_VALUE = 1.0E-5f;
    public static bool UseSystemRandom = false;

#if UNITY_EDITOR
    private static Random _random;
#endif

    public static bool floatEquals(float p_fA, float p_fB)
    {
        if (Mathf.Abs(p_fA - p_fB) < MIN_VALUE)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 得到一个浮动数值
    /// </summary>
    /// <param name="baseValue">基准值</param>
    /// <param name="floatingRate">浮动率</param>
    /// <returns></returns>
    public static float GetFloatingValue(float baseValue, float floatingRate)
    {
        float rate = UnityEngine.Random.Range(1.0f - floatingRate, 1.0f + floatingRate);
        return baseValue * rate;
    }

    /// <summary>
    /// 范围内随机选择
    /// min: include
    /// max: exclude
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static int GetRandom(int min, int max)
    {
#if UNITY_EDITOR
        if (UseSystemRandom)
        {
            if (_random == null)
            {
                _random = new Random();
            }
            return _random.Next(min, max);
        }
        else
        {
            return UnityEngine.Random.Range(min, max);
        }
#else
            return UnityEngine.Random.Range(min, max);
#endif
    }

    /// <summary>
    /// 范围内随机选择
    /// min: include
    /// max: include
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static float GetRandom(float min, float max)
    {
        return UnityEngine.Random.Range(min, max);
    }

    /// <summary>
    /// 根据一些权重值，随机选择某一个权重值对应的选项，返回其索引。要求所有权重值的和为100且每个权重值在区间【0，100】，否则返回-1；
    /// </summary>
    /// <returns>The random index by weights.</returns>
    /// <param name="weights">Weights.</param>
    public static int GetRandomIndexByWeights(int[] weights)
    {
        for (int i = 0; i < weights.Length; i++)
        {
            if (weights[i] < 0 || weights[i] > 100)
            {
                return -1;
            }
        }

        if (weights.Sum() != 100)
        {
            return -1;
        }

        int sum = 0;
        int[] init = new int[weights.Length];
        for (int i = 0; i < weights.Length; i++)
        {
            sum += weights[i];
            init[i] = sum;
        }

        int random = UnityEngine.Random.Range(0, 100);
        for (int i = 0; i < init.Length; i++)
        {
            if (random < init[i])
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// 按权重随机得到一个索引
    /// </summary>
    /// <param name="weights"></param>
    /// <returns></returns>
    public static int GetRandomIndexByWeightsSimple(params int[] weights)
    {
        return GetRandomIndexByWeights(weights);
    }

    /// <summary>
    /// 按权重随机得到一个索引
    /// </summary>
    /// <param name="weights"></param>
    public static int GetRandomIndexByWeights(List<int> weightsList)
    {
        return GetRandomIndexByWeights(weightsList.ToArray());
    }

}
