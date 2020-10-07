using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SerializableTransform
{
    public Vector3 position;
    public Vector3 scale;
    public Quaternion rotation;
    public List<SerializableTransform> children = new List<SerializableTransform>();

    public SerializableTransform(Transform transform)
    {
        position = transform.position;
        scale = transform.localScale;
        rotation = transform.rotation;

        foreach (Transform child in transform)
        {
            children.Add(new SerializableTransform(child));
        }
    }

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