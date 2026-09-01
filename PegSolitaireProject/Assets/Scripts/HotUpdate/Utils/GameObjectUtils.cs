using System;
using System.Collections.Generic;
using UnityEngine;

public static class GameObjectUtils
{
    /// <summary>
    /// 获取或添加Component
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static T GetOrAddComponent<T>(this GameObject obj) where T : Component
    {
        T component = obj.GetComponent<T>();
        if (component == null)
            component = obj.AddComponent<T>();

        return component;
    }

    /// <summary>
    /// 安全设置gameObject.setActive
    /// </summary>
    public static void SetActive(this Component component, bool isActive)
    {
        if (component != null && component.gameObject.activeSelf != isActive)
        {
            component.gameObject.SetActive(isActive);
        }
    }

    public static void SetColliderEnabled(this Component component, bool isEnabled)
    {
        if (component != null)
        {
            Collider[] colliderList = component.GetComponentsInChildren<Collider>();
            foreach (var collider in colliderList)
            {
                collider.enabled = isEnabled;
            }
        }
    }
    /// <summary>
    /// 安全设置 for gameObject.setActive
    /// </summary>
    public static void SetActive<T>(this IEnumerable<T> components, bool isActive) where T : Component
    {
        if (components != null)
        {
            foreach (var component in components)
            {
                SetActive(component, isActive);
            }
        }
    }

    /// <summary>
    /// 移除组件，如果是Transform组件则移除对象
    /// </summary>
    public static void DestroyObject(this Component obj)
    {
        if (obj == null) return;
        if (obj is Transform)
        {
            GameObject.Destroy(obj.gameObject);
        }
        else
        {
            GameObject.Destroy(obj);
        }
    }
    /// <summary>
    /// 移除对象
    /// </summary>
    public static void DestroyObject(this GameObject obj)
    {
        if (obj == null) return;
        GameObject.Destroy(obj);
    }
    /// <summary>
    /// 重置对象位置和父级
    /// </summary>
    /// <param name="gameObject"></param>
    /// <param name="parent"></param>
    public static void ResetGameObjectPos(this GameObject gameObject, Transform parent)
    {
        if (parent != null)
        {
            gameObject.transform.SetParent(parent, false);
        }
        gameObject.transform.localPosition = Vector3.zero;
        gameObject.transform.localRotation = Quaternion.identity;
        gameObject.transform.localScale = Vector3.one;
    }

    /// <summary>
    /// 设置对象Layer
    /// </summary>
    /// <param name="gameObject"></param>
    /// <param name="layerName"></param>
    /// <param name="withChild">是否改变子节点</param>
    public static void SetLayer(this GameObject gameObject, string layerName, bool withChild = true)
    {
        SetLayer(gameObject, LayerMask.NameToLayer(layerName), withChild);
    }
    public static void SetLayer(this GameObject gameObject, int layer, bool withChild = true)
    {
        gameObject.layer = layer;

        if (withChild)
        {
            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                GameObject child = gameObject.transform.GetChild(i).gameObject;
                SetLayer(child, layer, withChild);
            }
        }
    }

    /// <summary>
    /// 获取子节点上的Component
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="transform"></param>
    /// <param name="childPath"></param>
    /// <returns></returns>
    public static T GetComponentOnChild<T>(this Transform transform, string childPath) where T : Component
    {
        Transform childTransform = transform.Find(childPath);

        return childTransform?.gameObject.GetComponent<T>();
    }

    public static void SetX(this Transform transform, float x)
    {
        Vector3 newPosition = new Vector3(x, transform.position.y, transform.position.z);
        transform.position = newPosition;
    }

    public static void SetY(this Transform transform, float y)
    {
        Vector3 newPosition = new Vector3(transform.position.x, y, transform.position.z);
        transform.position = newPosition;
    }

    public static void SetZ(this Transform transform, float z)
    {
        Vector3 newPosition = new Vector3(transform.position.x, transform.position.y, z);
        transform.position = newPosition;
    }

    public static void SetPosition2D(this Transform transform, Vector3 target)
    {
        Vector3 newPostion = new Vector3(target.x, target.y, transform.position.z);
        transform.position = newPostion;
    }

    public static void SetLocalX(this Transform transform, float x)
    {
        Vector3 newPosition = new Vector3(x, transform.localPosition.y, transform.localPosition.z);
        transform.localPosition = newPosition;
    }

    public static void SetLocalY(this Transform transform, float y)
    {
        Vector3 newPosition = new Vector3(transform.localPosition.x, y, transform.localPosition.z);
        transform.localPosition = newPosition;
    }

    public static void SetLocalZ(this Transform transform, float z)
    {
        Vector3 newPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, z);
        transform.localPosition = newPosition;
    }

    public static void MoveLocalXYZ(this Transform transform, float deltaX, float deltaY, float deltaZ)
    {
        Vector3 newPosition = new Vector3(transform.localPosition.x + deltaX, transform.localPosition.y + deltaY, transform.localPosition.z + deltaZ);
        transform.localPosition = newPosition;
    }

    public static void SetLocalScale(this Transform transform, float scale)
    {
        transform.localScale = Vector3.one * scale;
    }

    public static void SetLocalScaleX(this Transform transform, float x)
    {
        Vector3 newScale = new Vector3(x, transform.localScale.y, transform.localScale.z);
        transform.localScale = newScale;
    }

    public static void SetLocalScaleY(this Transform transform, float y)
    {
        Vector3 newScale = new Vector3(transform.localScale.x, y, transform.localScale.z);
        transform.localScale = newScale;
    }

    public static void SetLocalScaleZ(this Transform transform, float z)
    {
        Vector3 newScale = new Vector3(transform.localScale.x, transform.localScale.y, z);
        transform.localScale = newScale;
    }

    public static void LookAt2D(this Transform transform, Vector3 target, float angle = 0)
    {
        Vector3 dir = target - transform.position;
        angle += Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle - 90);
    }

    public static void LookAt2D(this Transform transform, Transform target, float angle = 0)
    {
        LookAt2D(transform, target.position, angle);
    }

    // public static void SetItemSpriteByID(this UnityEngine.UI.Image itemIcon, int itemId, Action callBack = null)
    // {
    //     ItemHelper.GetItemSprite(itemId, (sprite) =>
    //     {
    //         itemIcon.sprite = sprite;
    //     });
    // }
}
