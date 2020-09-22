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
            children.Add(new SerializableTransform(child.transform));
        }
    }
}