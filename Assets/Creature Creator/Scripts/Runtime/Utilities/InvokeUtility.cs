using System.Collections;
using UnityEngine;

public class InvokeUtility
{
    public static IEnumerator Invoke(InvokeFunction invokeFunction, float delay = 0)
    {
        yield return new WaitForSeconds(delay);
        invokeFunction();
    }

    public delegate void InvokeFunction();
}
