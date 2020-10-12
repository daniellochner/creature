using UnityEngine;

namespace DanielLochner.Assets.CreatureCreator
{
    public abstract class BodyPart : Item
    {
        #region Fields
        [Header("Body Part")]
        [SerializeField] private GameObject prefab;
        [SerializeField] private int price;
        [SerializeField] private int complexity;
        [SerializeField] private int health;
        [SerializeField] private Attribute[] attributes;
        [EnumFlags] [SerializeField] private Transformation transformations;
        [SerializeField] private float minScale = 0.25f;
        [SerializeField] private float maxScale = 2.5f;
        [SerializeField] private float scaleIncrement = 0.1f;
        #endregion

        #region Properties
        public GameObject Prefab { get { return prefab; } }
        public int Price { get { return price; } }
        public int Complexity { get { return complexity; } }
        public int Health { get { return health; } }
        public Attribute[] Attributes { get { return attributes; } }
        public Transformation Transformations { get { return transformations; } }
        public float MinScale { get { return minScale; } }
        public float MaxScale { get { return maxScale; } }
        public float ScaleIncrement { get { return scaleIncrement; } }
        #endregion
    }
}