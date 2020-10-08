// Creature Creator - https://github.com/daniellochner/SPORE-Creature-Creator
// Version: 1.0.0
// Author: Daniel Lochner

using UnityEngine;

namespace DanielLochner.Assets.CreatureCreator
{
    public class LegController : LimbController
    {
        private Transform moveTransform;

        protected override void Start()
        {
            base.Start();

            moveTransform = new GameObject("Move").transform;

            moveTransform.parent = transform.parent;
            moveTransform.position = extremity.position;
        }

        private void OnDestroy()
        {
            if (moveTransform) Destroy(moveTransform.gameObject);
        }
    }
}