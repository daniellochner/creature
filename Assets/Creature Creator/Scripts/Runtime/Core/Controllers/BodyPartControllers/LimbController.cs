// Creature Creator - https://github.com/daniellochner/SPORE-Creature-Creator
// Version: 1.0.0
// Author: Daniel Lochner

using UnityEngine;

namespace DanielLochner.Assets.CreatureCreator
{
    public class LimbController : BodyPartController
    {
        #region Fields
        [SerializeField] private GameObject movePrefab;
        [SerializeField] private Transform root;
        #endregion

        #region Methods
        protected override void Awake()
        {
            base.Awake();

            AddTools(root);
        }

        private void AddTools(Transform root)
        {
            foreach (Transform childBone in root)
            {
                RecurseAdd(childBone);
            }
        }
        private void RecurseAdd(Transform bone)
        {
            foreach (Transform childBone in bone)
            {
                RecurseAdd(childBone);
            }

            Drag drag = Instantiate(movePrefab, bone, false).GetComponent<Drag>();
            drag.OnPress.AddListener(delegate
            {
                drag.transform.SetParent(Dynamic.Transform);
                CreatureCreator.Instance.CameraOrbit.Freeze();
            });
            drag.OnDrag.AddListener(delegate
            {
                bone.transform.position = drag.transform.position;
            });
            drag.OnRelease.AddListener(delegate
            {
                drag.transform.SetParent(bone);
                CreatureCreator.Instance.CameraOrbit.Unfreeze();
            });
        }
        #endregion
    }
}