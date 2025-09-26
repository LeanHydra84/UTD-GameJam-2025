using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{
    public static Stack<T> ToStack<T>(this IEnumerable<T> list)
    {
        return new Stack<T>(list);
    }

    public static float ClampAngle(float angle, float min, float max)
    {
        float start = (min + max) * 0.5f - 180;
        float floor = Mathf.FloorToInt((angle - start) / 360) * 360;
        return Mathf.Clamp(angle, min + floor, max + floor);
    }

    public static void SetAll<T>(this T[] array, T value)
    {
        for(int i = 0; i < array.Length; i++)
        {
            array[i] = value;
        }
    }
    
    public static bool IsAll<T>(this T[] array, T value)
    {
        for (int i = 0; i < array.Length; i++)
        {
            if(!array[i].Equals(value)) return false;
        }
        return true;
    }

    public static bool IsAllTrue(this bool[] array)
    {
        foreach(bool b in array)
        {
            if(!b) return false;
        }
        return true;
    }


}
