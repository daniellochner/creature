using UnityEngine;

public abstract class Item : ScriptableObject
{
    [Header("Item")]
    [SerializeField] private string description;
    [SerializeField] private Sprite icon;
}