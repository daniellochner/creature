using UnityEngine;

namespace DanielLochner.Assets.CreatureCreator
{
    public abstract class Limb : BodyPart
    {
        [Header("Limb")]
        [SerializeField] private int speed;

        public int Speed { get { return speed; } }
    }
}