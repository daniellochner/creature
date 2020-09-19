using UnityEngine;
using static DanielLochner.Assets.CreatureCreator.CreatureController;

namespace DanielLochner.Assets.CreatureCreator
{
    public class BodyPartController : MonoBehaviour
    {
        public AttachedBodyPart AttachedBodyPart { get; set; }
        public Drag Drag { get; set; }
        public BodyPartController Flipped { get; set; }

        private void Awake()
        {
            Drag = GetComponent<Drag>();
        }
    }
}