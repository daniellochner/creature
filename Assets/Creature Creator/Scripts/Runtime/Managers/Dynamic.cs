using UnityEngine;

public class Dynamic : MonoBehaviour
{
    #region Singleton
    private static Dynamic Instance { get; set; }
    private void Awake()
    {
        Instance = this;
    }
    #endregion

    #region Fields
    [SerializeField] private new Transform transform;
    [SerializeField] private Transform canvas;
    #endregion

    #region Properties
    public static Transform Transform { get { return Instance.transform; } }
    public static Transform Canvas { get { return Instance.canvas; } }
    #endregion
}
