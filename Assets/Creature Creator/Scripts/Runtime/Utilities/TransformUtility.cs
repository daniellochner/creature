using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformUtility : MonoBehaviour
{
    public static void RecurseUpdate(Transform transform, SerializableTransform serializableTransform)
    {
        transform.position = serializableTransform.position;
        transform.localScale = serializableTransform.scale;
        transform.rotation = serializableTransform.rotation;

        for (int i = 0; i < transform.childCount; i++)
        {
            RecurseUpdate(transform.GetChild(i), serializableTransform.children[i]);
        }
    }
}
