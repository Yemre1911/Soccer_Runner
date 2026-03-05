using System;
using UnityEngine;

public static class WeightedPicker
{
    public static T Pick<T>(T[] items, Func<T, float> weightSelector)
    {
        if (items == null || items.Length == 0) return default;

        float total = 0f;
        for (int i = 0; i < items.Length; i++)
        {
            float w = Mathf.Max(0f, weightSelector(items[i]));
            total += w;
        }
        if (total <= 0f) return default;

        float r = UnityEngine.Random.value * total;
        float acc = 0f;

        for (int i = 0; i < items.Length; i++)
        {
            float w = Mathf.Max(0f, weightSelector(items[i]));
            acc += w;
            if (r <= acc) return items[i];
        }

        return items[items.Length - 1];
    }
}