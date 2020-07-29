using UnityEngine;

namespace DanielLochner.Assets.CreatureCreator
{
    [CreateAssetMenu(fileName = "New Mouth", menuName = "Creature Creator/Body Part/Mouth")]
    public class Mouth : BodyPart
    {
        [Header("Mouth")]
        [SerializeField] private Diet diet;
    }
}