// Creature Creator - https://github.com/daniellochner/SPORE-Creature-Creator
// Version: 1.0.0
// Author: Daniel Lochner

using DitzelGames.FastIK;
using System.Collections.Generic;
using UnityEngine;

namespace DanielLochner.Assets.CreatureCreator
{
    public class LimbController : BodyPartController
    {
        #region Fields
        [SerializeField] private GameObject movePrefab;
        [SerializeField] private Transform[] bones;
        [SerializeField] protected Transform extremity;

        private SkinnedMeshRenderer skinnedMeshRenderer;
        private MeshCollider meshCollider;

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
        }
        protected override void Start()
        {
            base.Start();

            #region Interact
            for (int i = 0; i < bones.Length; i++)
            {
                Transform bone = bones[i];
                Transform flippedBone = FlippedLimb.bones[i];

                #region Interact
                GameObject moveGO = Instantiate(movePrefab, bone, false);

                Hover boneHover = bone.GetComponent<Hover>();
                boneHover.OnEnter.AddListener(delegate
                {
                    if (!Input.GetMouseButton(0))
                    {
                        SetToolsVisibility(true);
                        FlippedLimb.SetToolsVisibility(true);
                    }
                });
                boneHover.OnExit.AddListener(delegate
                {
                    if (!Input.GetMouseButton(0))
                    {
                        SetToolsVisibility(false);
                        FlippedLimb.SetToolsVisibility(false);
                    }
                });

                Drag boneDrag = bone.GetComponent<Drag>();
                boneDrag.OnPress.AddListener(delegate
                {
                    CreatureCreator.Instance.CameraOrbit.Freeze();

                    SetToolsVisibility(true);
                    FlippedLimb.SetToolsVisibility(true);
                });
                boneDrag.OnDrag.AddListener(delegate
                {
                    flippedBone.position = new Vector3(-bone.position.x, bone.position.y, bone.position.z);
                });
                boneDrag.OnRelease.AddListener(delegate
                {
                    CreatureCreator.Instance.CameraOrbit.Unfreeze();

                    if (!boneHover.IsOver && !hover.IsOver)
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
            #endregion

            UpdateMeshCollider();
            SetToolsVisibility(false);
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