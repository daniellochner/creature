// Creature Creator - https://github.com/daniellochner/SPORE-Creature-Creator
// Version: 1.0.0
// Author: Daniel Lochner

using System.Collections.Generic;
using UnityEngine;

namespace DanielLochner.Assets.CreatureCreator
{
    public class LimbController : BodyPartController
    {
        #region Fields
        [SerializeField] private GameObject movePrefab;
        [SerializeField] private Transform root;
        [SerializeField] private Transform extremity;

        private SkinnedMeshRenderer skinnedMeshRenderer;
        private MeshCollider meshCollider;

        private Transform[] bones;
        private List<MeshRenderer> tools = new List<MeshRenderer>();
        #endregion

        #region Properties
        public LimbController FlippedLimb { get { return Flipped as LimbController; } }
        public Transform Extremity { get { return extremity; } }
        #endregion

        #region Methods
        protected override void Awake()
        {
            base.Awake();

            skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
            meshCollider = GetComponentInChildren<MeshCollider>();

            bones = root.GetComponentsInChildren<Transform>();
        }
        protected override void Start()
        {
            base.Start();

            AddTools(root);

            Hover hover = GetComponent<Hover>();
            hover.OnEnter.AddListener(delegate
            {
                if (!Input.GetMouseButton(0))
                {
                    SetToolsVisibility(true);
                    FlippedLimb.SetToolsVisibility(true);
                }
            });
            hover.OnExit.AddListener(delegate
            {
                if (!Input.GetMouseButton(0))
                {
                    SetToolsVisibility(false);
                    FlippedLimb.SetToolsVisibility(false);
                }
            });

            UpdateMeshCollider();
            SetToolsVisibility(false);
        }

        private void AddTools(Transform root)
        {
            for (int i = 1; i < bones.Length - 1; i++)
            {
                Transform bone = bones[i];
                Transform flippedBone = FlippedLimb.bones[i];

                #region Interact
                GameObject moveGO = Instantiate(movePrefab, bone, false);

                Hover hover = moveGO.GetComponent<Hover>();
                hover.OnEnter.AddListener(delegate
                {
                    if (!Input.GetMouseButton(0))
                    {
                        SetToolsVisibility(true);
                        FlippedLimb.SetToolsVisibility(true);
                    }
                });
                hover.OnExit.AddListener(delegate
                {
                    if (!Input.GetMouseButton(0))
                    {
                        SetToolsVisibility(false);
                        FlippedLimb.SetToolsVisibility(false);
                    }
                });

                Drag drag = moveGO.GetComponent<Drag>();
                drag.OnPress.AddListener(delegate
                {
                    drag.transform.SetParent(Dynamic.Transform);

                    CreatureCreator.Instance.CameraOrbit.Freeze();

                    SetToolsVisibility(true);
                    FlippedLimb.SetToolsVisibility(true);
                });
                drag.OnDrag.AddListener(delegate
                {
                    bone.position = drag.transform.position;
                    flippedBone.position = new Vector3(-bone.position.x, bone.position.y, bone.position.z);
                });
                drag.OnRelease.AddListener(delegate
                {
                    drag.transform.SetParent(bone);

                    CreatureCreator.Instance.CameraOrbit.Unfreeze();

                    if (!hover.IsOver && !GetComponent<Hover>().IsOver)
                    {
                        SetToolsVisibility(false);
                        FlippedLimb.SetToolsVisibility(false);
                    }

                    UpdateMeshCollider();
                    FlippedLimb.UpdateMeshCollider();
                });

                tools.Add(moveGO.GetComponent<MeshRenderer>());
                #endregion
            }
        }
        private void SetToolsVisibility(bool visible)
        {
            foreach (MeshRenderer tool in tools)
            {
                tool.enabled = visible;
            }
        }
        private void UpdateMeshCollider()
        {
            Mesh skinnedMesh = new Mesh();
            skinnedMeshRenderer.BakeMesh(skinnedMesh);
            meshCollider.sharedMesh = skinnedMesh;
        }
        #endregion
    }
}