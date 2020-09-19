using UnityEngine;

public class Dynamic : MonoBehaviour
{
    public static Transform Transform { get; private set; }
    public static Transform Canvas { get; private set; }

    private void Awake()
    {
        Transform = transform;
        Canvas = transform.GetChild(0);
    }
}
