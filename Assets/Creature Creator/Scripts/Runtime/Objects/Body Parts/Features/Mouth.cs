using UnityEngine;

namespace DanielLochner.Assets.CreatureCreator
{
    [CreateAssetMenu(fileName = "New Mouth", menuName = "Creature Creator/Body Part/Feature/Mouth")]
    public class Mouth : Feature
    {
        [Header("Mouth")]
        [SerializeField] private Diet diet;

        public Diet Diet { get { return diet; } }
    }
}