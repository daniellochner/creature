using UnityEngine;

namespace DanielLochner.Assets.CreatureCreator
{
    public abstract class BodyPart : Item
    {
        [Header("Body Part")]
        [SerializeField] private GameObject prefab;
        [SerializeField] private int price;
        [SerializeField] private int complexity;
        [SerializeField] private Attribute[] attributes;
        [EnumFlags] [SerializeField] private Transformation transformations;
    }
}