using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using Tweens;

public static class UnityTweensTester
{
    static GameObject bindTarget;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CleanUp()
    {
        bindTarget.CancelTweens();
        GameObject.Destroy(bindTarget);
        bindTarget = null;
        GC.Collect();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Init()
    {
        bindTarget = new GameObject();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CreateFloatTweens(TestClass[] array, float duration)
    {
        for (int i = 0; i < array.Length; i++)
        {
            var index = i;
            var tween = new FloatTween
            {
                from = 0f,
                to = 10f,
                duration = duration,
                onUpdate = (_, value) => array[index].value = value,
            };
            bindTarget.AddTween(tween);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CreatePositionTweens(Transform[] transforms, float duration)
    {
        for (int i = 0; i < transforms.Length; i++)
        {
            var tween = new PositionTween
            {
                to = Vector3.one * i,
                duration = duration,
            };
            transforms[i].gameObject.AddTween(tween);
        }
    }
}

