// Creature Creator - https://github.com/daniellochner/SPORE-Creature-Creator
// Version: 1.0.0
// Author: Daniel Lochner

using UnityEngine;

namespace DanielLochner.Assets.CreatureCreator
{
    public class LegController : LimbController
    {
        #region Methods
        protected override void LateUpdate()
        {
            base.LateUpdate();

            if (Bones[1].position.y > 0)
            {
                Model.gameObject.SetActive(true);

                Vector3 bonePosition = Bones[Bones.Length - 1].position;
                bonePosition.x = Mathf.Clamp(bonePosition.x, Drag.WorldBounds.center.x - Drag.WorldBounds.extents.x / 2f, Drag.WorldBounds.center.x + Drag.WorldBounds.extents.x / 2f);
                bonePosition.y = 0;
                bonePosition.z = Mathf.Clamp(bonePosition.z, Drag.WorldBounds.center.z - Drag.WorldBounds.extents.z / 2f, Drag.WorldBounds.center.z + Drag.WorldBounds.extents.z / 2f);

                Bones[Bones.Length - 1].position = bonePosition;
            }
            else
            {
                Model.gameObject.SetActive(false);
            }
        }
        #endregion
    }
}