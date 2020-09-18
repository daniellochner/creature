using UnityEngine;
using static DanielLochner.Assets.CreatureCreator.CreatureController;

namespace DanielLochner.Assets.CreatureCreator
{
    public class BodyPartController : MonoBehaviour
    {
        public AttachedBodyPart attachedBodyPart { get; set; }

        public Drag drag;
        public BodyPartController flipped;

        private void Awake()
        {
            drag = GetComponent<Drag>();
        }
    }
}