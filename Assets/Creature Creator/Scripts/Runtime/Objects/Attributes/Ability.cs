using UnityEngine;

namespace DanielLochner.Assets.CreatureCreator
{
    public abstract class Ability : Attribute
    {
        [Header("Ability")]
        [SerializeField] private int level;

        public int Level { get { return level; } }
    }
}