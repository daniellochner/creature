using UnityEngine;

namespace DanielLochner.Assets.CreatureCreator
{
    public class LimbController : BodyPartController
    {
        [SerializeField] private GameObject movePrefab;
        [SerializeField] private Transform root;

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
    }
}