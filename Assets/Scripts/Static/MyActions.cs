using UnityEngine;
using System;

public static class MyActions
{
    public static Action<int, Vector3, float> onLoadMagic;
    public static Action<int, Vector3, Vector3, float> onShootMagic;
    public static Func<int, Magic> onCreateMagic;
}
