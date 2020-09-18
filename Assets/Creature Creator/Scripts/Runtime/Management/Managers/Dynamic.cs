using UnityEngine;

public class Dynamic : MonoBehaviour
{
    public static Transform Transform { get; private set; }

    private void Awake()
    {
        Transform = transform;
    }
}
