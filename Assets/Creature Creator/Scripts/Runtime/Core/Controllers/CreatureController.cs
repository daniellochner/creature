// Creature Creator - https://github.com/daniellochner/SPORE-Creature-Creator
// Version: 1.0.0
// Author: Daniel Lochner

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace DanielLochner.Assets.CreatureCreator
{
    public class CreatureController : MonoBehaviour
    {
        #region Fields
        [SerializeField] private Material bodyMaterial;
        [SerializeField] private Material bodyPartMaterial;
        [Space]
        [SerializeField] private GameObject boneTool;
        [SerializeField] private GameObject stretchTool;
        [SerializeField] private GameObject poofEffect;
        [Space]
        [SerializeField] private AudioClip stretchAudioClip;
        [SerializeField] private AudioClip sizeAudioClip;
        [SerializeField] private AudioClip poofAudioClip;
        [Space]
        [SerializeField] private CreatureSettings settings;
        [SerializeField] private CreatureStatistics statistics;
        [SerializeField] private CreatureData data;

        private SkinnedMeshRenderer skinnedMeshRenderer;
        private MeshCollider meshCollider;
        private AudioSource audioSource;
        private Mesh mesh;
        private Outline outline;
        private Transform root, frontArrow, backArrow;

        private List<LimbController> limbs = new List<LimbController>();
        #endregion

        #region Properties
        public CreatureSettings Settings { get { return settings; } }
        public CreatureStatistics Statistics { get { return statistics; } }
        public CreatureData Data { get { return data; } }

        public bool Selected { get; private set; }
        public bool Textured { get; private set; }
        public bool Interactable { get; private set; }
        #endregion

        #region Methods
        private void Awake()
        {
            Initialize();
        }

        private void Initialize()
        {
            #region Creature
            transform.position = Vector3.zero;

            GameObject model = new GameObject("Model");
            model.transform.SetParent(transform);
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;

            root = new GameObject("Root").transform;
            root.SetParent(transform);
            root.localPosition = Vector3.zero;
            root.localRotation = Quaternion.identity;

            skinnedMeshRenderer = model.AddComponent<SkinnedMeshRenderer>();
            skinnedMeshRenderer.sharedMesh = mesh = model.AddComponent<MeshFilter>().sharedMesh = new Mesh();
            skinnedMeshRenderer.rootBone = root.transform;
            skinnedMeshRenderer.updateWhenOffscreen = true;

            skinnedMeshRenderer.sharedMaterial = bodyMaterial;
            SetColours(Color.white, Color.black);
            SetPattern("");

            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.volume = 0.25f;

            outline = model.AddComponent<Outline>();
            outline.OutlineWidth = 5f;

            meshCollider = gameObject.AddComponent<MeshCollider>();
            mesh.name = "Body";
            #endregion

            #region Tools
            Drag drag = gameObject.AddComponent<Drag>();
            drag.OnPress.AddListener(delegate
            {
                CreatureCreator.Instance.CameraOrbit.Freeze();
            });
            drag.OnRelease.AddListener(delegate
            {
                CreatureCreator.Instance.CameraOrbit.Unfreeze();
                UpdateBoneConfiguration();

                foreach (LimbController limb in limbs)
                {
                    limb.UpdateMeshCollider();
                }
            });

            Click click = gameObject.AddComponent<Click>();
            click.OnClick.AddListener(delegate
            {
                SetSelected(true);
            });

            Hover hover = gameObject.AddComponent<Hover>();
            hover.OnEnter.AddListener(delegate
            {
                if (!Input.GetMouseButton(0))
                {
                    CreatureCreator.Instance.CameraOrbit.Freeze();
                    SetBonesVisibility(true);
                }
            });
            hover.OnExit.AddListener(delegate
            {
                if (!Input.GetMouseButton(0))
                {
                    CreatureCreator.Instance.CameraOrbit.Unfreeze();

                    if (!Selected)
                    {
                        SetBonesVisibility(false);
                    }
                }
            });

            backArrow = Instantiate(stretchTool).transform;
            frontArrow = Instantiate(stretchTool).transform;
            frontArrow.Find("Model").localPosition = backArrow.Find("Model").localPosition = Vector3.forward * (settings.Length / 2f + settings.Radius + 0.05f);

            Drag frontArrowDrag = frontArrow.GetComponentInChildren<Drag>();
            frontArrowDrag.OnPress.AddListener(delegate
            {
                CreatureCreator.Instance.CameraOrbit.Freeze();
            });
            frontArrowDrag.OnRelease.AddListener(delegate
            {
                CreatureCreator.Instance.CameraOrbit.Unfreeze();
            });
            frontArrowDrag.OnDrag.AddListener(delegate
            {
                Vector3 displacement = frontArrowDrag.TargetWorldPosition - frontArrow.position;
                if (displacement.magnitude > settings.Length)
                {
                    if (Vector3.Dot(displacement.normalized, frontArrow.forward) > 0.1f && (data.bones.Count + 1) <= settings.MinMaxBones.y)
                    {
                        UpdateBoneConfiguration();

                        Vector3 direction = (frontArrowDrag.TargetMousePosition - frontArrow.position).normalized;
                        Vector3 position = data.bones[0].Position + direction * settings.Length;
                        Quaternion rotation = Quaternion.LookRotation(-direction, frontArrow.up);

                        if (Add(0, position, rotation, Mathf.Clamp(data.bones[0].Size * 0.75f, 0f, 100f)))
                        {
                            audioSource.PlayOneShot(stretchAudioClip);
                        }

                        frontArrowDrag.OnMouseDown();
                    }
                    else if (Vector3.Dot(displacement.normalized, frontArrow.forward) < -0.1f)
                    {
                        if (RemoveFromFront())
                        {
                            audioSource.PlayOneShot(stretchAudioClip);
                        }
                    }
                }
            });

            Drag backArrowDrag = backArrow.GetComponentInChildren<Drag>();
            backArrowDrag.OnPress.AddListener(delegate
            {
                CreatureCreator.Instance.CameraOrbit.Freeze();
            });
            backArrowDrag.OnRelease.AddListener(delegate
            {
                CreatureCreator.Instance.CameraOrbit.Unfreeze();
            });
            backArrowDrag.OnDrag.AddListener(delegate
            {
                Vector3 displacement = backArrowDrag.TargetWorldPosition - backArrow.position;
                if (displacement.magnitude > settings.Length)
                {
                    if (Vector3.Dot(displacement.normalized, backArrow.forward) > 0.1f && (data.bones.Count + 1) <= settings.MinMaxBones.y)
                    {
                        UpdateBoneConfiguration();

                        Vector3 direction = (backArrowDrag.TargetMousePosition - backArrow.position).normalized;
                        Vector3 position = data.bones[root.childCount - 1].Position + direction * settings.Length;
                        Quaternion rotation = Quaternion.LookRotation(direction, backArrow.up);

                        if (Add(root.childCount, position, rotation, Mathf.Clamp(data.bones[root.childCount - 1].Size * 0.75f, 0f, 100f)))
                        {
                            audioSource.PlayOneShot(stretchAudioClip);
                        }

                        backArrowDrag.OnMouseDown();
                    }
                    else if (Vector3.Dot(displacement.normalized, backArrow.forward) < -0.1f)
                    {
                        if (RemoveFromBack())
                        {
                            audioSource.PlayOneShot(stretchAudioClip);
                        }
                    }
                }
            });
            #endregion
        }

        public void Save(string creatureName)
        {
            UpdateBoneConfiguration();
            UpdateAttachedBodyPartsConfiguration();

            CreatureData data = new CreatureData()
            {
                bones = this.data.bones,
                attachedBodyParts = this.data.attachedBodyParts,

                patternID = this.data.patternID,
                primaryColour = this.data.primaryColour,
                secondaryColour = this.data.secondaryColour
            };

            SaveUtility.Save(JsonUtility.ToJson(data), creatureName + ".json");
        }
        public void Load(string creatureName)
        {
            Clear();

            CreatureData data = JsonUtility.FromJson<CreatureData>(SaveUtility.Load(creatureName + ".json"));

            for (int i = 0; i < data.bones.Count; i++)
            {
                Bone bone = data.bones[i];
                Add(i, bone.Position, bone.Rotation, bone.Size);
            }
            for (int i = 0; i < data.attachedBodyParts.Count; i++)
            {
                AttachedBodyPart attachedBodyPart = data.attachedBodyParts[i];

                BodyPartController bpc = Instantiate(DatabaseManager.GetDatabaseEntry<BodyPart>("Body Parts", attachedBodyPart.BodyPartID).Prefab, root.GetChild(attachedBodyPart.BoneIndex)).GetComponent<BodyPartController>();
                bpc.gameObject.name = attachedBodyPart.BodyPartID;

                SerializableTransform.RecurseUpdate(bpc.transform, attachedBodyPart.Transform);

                SetupBodyPart(bpc);
                AttachBodyPart(bpc);
                AddToStatistics(bpc.name);

                if (Mathf.Abs(bpc.transform.position.x) > settings.MergeThreshold)
                {
                    bpc.Flipped.transform.position = new Vector3(-bpc.transform.position.x, bpc.transform.position.y, bpc.transform.position.z);
                    bpc.Flipped.transform.rotation = Quaternion.Euler(bpc.transform.rotation.eulerAngles.x, -bpc.transform.rotation.eulerAngles.y, -bpc.transform.rotation.eulerAngles.z);

                    if (bpc is LimbController)
                    {
                        LimbController limb = bpc as LimbController;
                        LimbController flippedLimb = limb.FlippedLimb;

                        for (int j = 0; j < limb.Bones.Length; j++)
                        {
                            float x = -limb.Bones[j].position.x;
                            float y = limb.Bones[j].position.y;
                            float z = limb.Bones[j].position.z;

                            flippedLimb.Bones[j].position = new Vector3(x, y, z);
                        }
                    }
                }
                else
                {
                    bpc.Flipped.gameObject.SetActive(false);
                }
            }
            SetColours(data.primaryColour, data.secondaryColour);
            SetPattern(data.patternID);

            SetSelected(false);
            SetTextured(Textured);
            SetInteractable(Interactable);
        }

        public void Setup()
        {
            #region Mesh Generation
            mesh.Clear();

            #region Vertices
            List<Vector3> vertices = new List<Vector3>();
            List<BoneWeight> boneWeights = new List<BoneWeight>();

            // Top Hemisphere.
            vertices.Add(new Vector3(0, 0, 0));
            boneWeights.Add(new BoneWeight() { boneIndex0 = 0, weight0 = 1 });
            for (int ringIndex = 1; ringIndex < settings.Segments / 2; ringIndex++)
            {
                float percent = (float)ringIndex / (settings.Segments / 2);
                float ringRadius = settings.Radius * Mathf.Sin(90f * percent * Mathf.Deg2Rad);
                float ringDistance = settings.Radius * (-Mathf.Cos(90f * percent * Mathf.Deg2Rad) + 1f);

                for (int i = 0; i < settings.Segments; i++)
                {
                    float angle = i * 360f / settings.Segments;

                    float x = ringRadius * Mathf.Cos(angle * Mathf.Deg2Rad);
                    float y = ringRadius * Mathf.Sin(angle * Mathf.Deg2Rad);
                    float z = ringDistance;

                    vertices.Add(new Vector3(x, y, z));
                    boneWeights.Add(new BoneWeight() { boneIndex0 = 0, weight0 = 1f });
                }
            }

            // Middle Cylinder.
            for (int ringIndex = 0; ringIndex < settings.Rings * data.bones.Count; ringIndex++)
            {
                float boneIndexFloat = (float)ringIndex / settings.Rings;
                int boneIndex = Mathf.FloorToInt(boneIndexFloat);

                float bonePercent = boneIndexFloat - boneIndex;

                int boneIndex0 = (boneIndex > 0) ? boneIndex - 1 : 0;
                int boneIndex2 = (boneIndex < data.bones.Count - 1) ? boneIndex + 1 : data.bones.Count - 1;
                int boneIndex1 = boneIndex;

                float weight0 = (boneIndex > 0) ? (1f - bonePercent) * 0.5f : 0f;
                float weight2 = (boneIndex < data.bones.Count - 1) ? bonePercent * 0.5f : 0f;
                float weight1 = 1f - (weight0 + weight2);

                for (int i = 0; i < settings.Segments; i++)
                {
                    float angle = i * 360f / settings.Segments;

                    float x = settings.Radius * Mathf.Cos(angle * Mathf.Deg2Rad);
                    float y = settings.Radius * Mathf.Sin(angle * Mathf.Deg2Rad);
                    float z = ringIndex * settings.Length / settings.Rings;

                    vertices.Add(new Vector3(x, y, settings.Radius + z));
                    boneWeights.Add(new BoneWeight()
                    {
                        boneIndex0 = boneIndex0,
                        boneIndex1 = boneIndex1,
                        boneIndex2 = boneIndex2,
                        weight0 = weight0,
                        weight1 = weight1,
                        weight2 = weight2
                    });
                }
            }

            // Bottom Hemisphere.
            for (int ringIndex = 0; ringIndex < settings.Segments / 2; ringIndex++)
            {
                float percent = (float)ringIndex / (settings.Segments / 2);
                float ringRadius = settings.Radius * Mathf.Cos(90f * percent * Mathf.Deg2Rad);
                float ringDistance = settings.Radius * Mathf.Sin(90f * percent * Mathf.Deg2Rad);

                for (int i = 0; i < settings.Segments; i++)
                {
                    float angle = i * 360f / settings.Segments;

                    float x = ringRadius * Mathf.Cos(angle * Mathf.Deg2Rad);
                    float y = ringRadius * Mathf.Sin(angle * Mathf.Deg2Rad);
                    float z = ringDistance;

                    vertices.Add(new Vector3(x, y, settings.Radius + (settings.Length * data.bones.Count) + z));
                    boneWeights.Add(new BoneWeight() { boneIndex0 = data.bones.Count - 1, weight0 = 1 });
                }
            }
            vertices.Add(new Vector3(0, 0, 2f * settings.Radius + (settings.Length * data.bones.Count)));
            boneWeights.Add(new BoneWeight() { boneIndex0 = data.bones.Count - 1, weight0 = 1 });

            mesh.vertices = vertices.ToArray();
            mesh.boneWeights = boneWeights.ToArray();
            #endregion

            #region Triangles
            List<int> triangles = new List<int>();

            // Top Cap.
            for (int i = 0; i < settings.Segments; i++)
            {
                int seamOffset = i != settings.Segments - 1 ? 0 : settings.Segments;

                triangles.Add(0);
                triangles.Add(i + 2 - seamOffset);
                triangles.Add(i + 1);
            }

            // Main.
            int rings = (settings.Rings * data.bones.Count) + (2 * (settings.Segments / 2 - 1));
            for (int ringIndex = 0; ringIndex < rings; ringIndex++)
            {
                int ringOffset = 1 + ringIndex * settings.Segments;

                for (int i = 0; i < settings.Segments; i++)
                {
                    int seamOffset = i != settings.Segments - 1 ? 0 : settings.Segments;

                    triangles.Add(ringOffset + i);
                    triangles.Add(ringOffset + i + 1 - seamOffset);
                    triangles.Add(ringOffset + i + 1 - seamOffset + settings.Segments);

                    triangles.Add(ringOffset + i + 1 - seamOffset + settings.Segments);
                    triangles.Add(ringOffset + i + settings.Segments);
                    triangles.Add(ringOffset + i);
                }
            }

            // Bottom Cap.
            int topIndex = 1 + (rings + 1) * settings.Segments;
            for (int i = 0; i < settings.Segments; i++)
            {
                int seamOffset = i != settings.Segments - 1 ? 0 : settings.Segments;

                triangles.Add(topIndex);
                triangles.Add(topIndex - i - 2 + seamOffset);
                triangles.Add(topIndex - i - 1);
            }

            mesh.triangles = triangles.ToArray();
            #endregion

            #region UV
            List<Vector2> uv = new List<Vector2>();

            uv.Add(Vector2.zero);
            for (int ringIndex = 0; ringIndex < rings + 1; ringIndex++)
            {
                float v = ringIndex / (float)rings;
                for (int i = 0; i < settings.Segments; i++)
                {
                    float u = i / (float)(settings.Segments - 1);
                    uv.Add(new Vector2(u, v * (data.bones.Count + 1)));
                }
            }
            uv.Add(Vector2.one);

            mesh.uv8 = uv.ToArray(); // Store copy of UVs in mesh.
            #endregion

            #region Bones
            Transform[] boneTransforms = new Transform[data.bones.Count];
            Matrix4x4[] bindPoses = new Matrix4x4[data.bones.Count];
            Vector3[] deltaZeroArray = new Vector3[vertices.Count];
            for (int vertIndex = 0; vertIndex < vertices.Count; vertIndex++)
            {
                deltaZeroArray[vertIndex] = Vector3.zero;
            }

            for (int boneIndex = 0; boneIndex < data.bones.Count; boneIndex++)
            {
                boneTransforms[boneIndex] = root.GetChild(boneIndex);

                #region Bind Pose
                boneTransforms[boneIndex].localPosition = Vector3.forward * (settings.Radius + settings.Length * (0.5f + boneIndex));
                boneTransforms[boneIndex].localRotation = Quaternion.identity;
                bindPoses[boneIndex] = boneTransforms[boneIndex].worldToLocalMatrix * transform.localToWorldMatrix;
                #endregion

                #region Hinge Joint
                if (boneIndex > 0)
                {
                    HingeJoint hingeJoint = boneTransforms[boneIndex].GetComponent<HingeJoint>();
                    hingeJoint.anchor = new Vector3(0, 0, -settings.Length / 2f);
                    hingeJoint.connectedBody = boneTransforms[boneIndex - 1].GetComponent<Rigidbody>();
                }
                #endregion

                #region Blend Shapes
                Vector3[] deltaVertices = new Vector3[vertices.Count];
                for (int vertIndex = 0; vertIndex < vertices.Count; vertIndex++)
                {
                    // Round
                    //float distanceToBone = Mathf.Clamp(Vector3.Distance(vertices[vertIndex], boneTransforms[boneIndex].localPosition), 0, 2f * settings.Length);
                    //Vector3 directionToBone = (vertices[vertIndex] - boneTransforms[boneIndex].localPosition).normalized;

                    //deltaVertices[vertIndex] = directionToBone * (2f * settings.Length - distanceToBone);


                    // Smooth - https://www.desmos.com/calculator/wmpvvtmor8
                    float maxDistanceAlongBone = settings.Length * 2f;
                    float maxHeightAboveBone = settings.Radius * 2f;

                    float displacementAlongBone = vertices[vertIndex].z - boneTransforms[boneIndex].localPosition.z;

                    float x = Mathf.Clamp(displacementAlongBone / maxDistanceAlongBone, -1, 1);
                    float a = maxHeightAboveBone;
                    float b = 1f / a;

                    float heightAboveBone = (Mathf.Cos(x * Mathf.PI) / b + a) / 2f;

                    deltaVertices[vertIndex] = new Vector2(vertices[vertIndex].x, vertices[vertIndex].y).normalized * heightAboveBone;
                }
                mesh.AddBlendShapeFrame("Bone." + boneIndex, 0, deltaZeroArray, deltaZeroArray, deltaZeroArray);
                mesh.AddBlendShapeFrame("Bone." + boneIndex, 100, deltaVertices, deltaZeroArray, deltaZeroArray);

                Scroll scroll = boneTransforms[boneIndex].GetComponent<Scroll>();
                int index = boneIndex;
                scroll.OnScrollUp.RemoveAllListeners();
                scroll.OnScrollDown.RemoveAllListeners();
                scroll.OnScrollUp.AddListener(delegate 
                {
                    audioSource.PlayOneShot(sizeAudioClip);

                    AddWeight(index, 5f);
                });
                scroll.OnScrollDown.AddListener(delegate 
                {
                    audioSource.PlayOneShot(sizeAudioClip);

                    RemoveWeight(index, 5f);
                });
                #endregion
            }

            mesh.bindposes = bindPoses;
            skinnedMeshRenderer.bones = boneTransforms;
            #endregion

            mesh.RecalculateNormals();
            #endregion

            #region Mesh Deformation
            for (int boneIndex = 0; boneIndex < data.bones.Count; boneIndex++)
            {
                boneTransforms[boneIndex].position = data.bones[boneIndex].Position;
                boneTransforms[boneIndex].rotation = data.bones[boneIndex].Rotation;
                SetWeight(boneIndex, data.bones[boneIndex].Size);
            }

            UpdateMeshCollider();
            #endregion
        }
        public void Clear()
        {
            Component[] components = transform.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] is Transform || components[i] is CreatureController)
                {
                    continue;
                }
                Destroy(components[i]);
            }
            for (int i = 0; i < transform.childCount; i++)
            {
                Destroy(transform.GetChild(i).gameObject);
            }

            data = new CreatureData();
            statistics = new CreatureStatistics();

            Initialize();
        }

        public bool Add(int index, Vector3 position, Quaternion rotation, float size)
        {
            if ((data.bones.Count + 1) <= settings.MinMaxBones.y && (statistics.Complexity + 1) <= settings.MaximumComplexity)
            {
                #region Detach Body Parts
                BodyPartController[] bpcs = root.GetComponentsInChildren<BodyPartController>();

                foreach (BodyPartController bpc in bpcs)
                {
                    DetachBodyPart(bpc);
                    bpc.transform.SetParent(null, true);
                }
                #endregion

                #region Bone
                GameObject boneGameObject = Instantiate(boneTool, root, false);
                boneGameObject.name = "Bone." + (root.childCount - 1);
                boneGameObject.layer = LayerMask.NameToLayer("Tools");

                if (data.bones.Count == 0)
                {
                    DestroyImmediate(boneGameObject.GetComponent<HingeJoint>());
                }
                #endregion

                #region Tools
                Drag drag = boneGameObject.GetComponent<Drag>();
                drag.OnPress.AddListener(delegate
                {
                    CreatureCreator.Instance.CameraOrbit.Freeze();
                });
                drag.OnRelease.AddListener(delegate
                {
                    CreatureCreator.Instance.CameraOrbit.Unfreeze();
                    UpdateMeshCollider();
                    UpdateBoneConfiguration();

                    foreach (LimbController limb in limbs)
                    {
                        limb.UpdateMeshCollider();
                    }
                });

                Hover hover = boneGameObject.GetComponent<Hover>();
                hover.OnEnter.AddListener(delegate
                {
                    if (!Input.GetMouseButton(0))
                    {
                        CreatureCreator.Instance.CameraOrbit.Freeze();
                        SetBonesVisibility(true);
                    }
                });
                hover.OnExit.AddListener(delegate
                {
                    if (!Input.GetMouseButton(0))
                    {
                        CreatureCreator.Instance.CameraOrbit.Unfreeze();

                        if (!Selected)
                        {
                            SetBonesVisibility(false);
                        }
                    }
                });

                Click click = boneGameObject.GetComponent<Click>();
                click.OnClick.AddListener(delegate
                {
                    SetSelected(true);
                });

                frontArrow.SetParent(root.GetChild(0));
                backArrow.SetParent(root.GetChild(root.childCount - 1));
                frontArrow.localPosition = backArrow.localPosition = Vector3.zero;
                frontArrow.localRotation = Quaternion.Euler(0, 180, 0);
                backArrow.localRotation = Quaternion.identity;
                #endregion

                data.bones.Insert(index, new Bone(position, rotation, size));
                statistics.Complexity++;

                Setup();

                #region Reattach Body Parts
                foreach (BodyPartController bpc in bpcs)
                {
                    AttachBodyPart(bpc);
                }
                #endregion

                return true;
            }
            return false;
        }
        public bool AddToFront()
        {
            UpdateBoneConfiguration();

            Vector3 position = data.bones[0].Position - root.GetChild(0).forward * settings.Length;
            Quaternion rotation = data.bones[0].Rotation;

            return Add(0, position, rotation, Mathf.Clamp(data.bones[0].Size * 0.75f, 0f, 100f));
        }
        public bool AddToBack()
        {
            UpdateBoneConfiguration();

            Vector3 position = data.bones[data.bones.Count - 1].Position + root.GetChild(data.bones.Count - 1).forward * settings.Length;
            Quaternion rotation = data.bones[data.bones.Count - 1].Rotation;

            return Add(data.bones.Count, position, rotation, Mathf.Clamp(data.bones[data.bones.Count - 1].Size * 0.75f, 0f, 100f));
        }

        public bool Remove(int index)
        {
            int bodyPartCount = 0;
            foreach (AttachedBodyPart attachedBodyPart in data.attachedBodyParts)
            {
                if (attachedBodyPart.BoneIndex == index)
                {
                    bodyPartCount++;
                }
            }

            if ((data.bones.Count - 1) >= settings.MinMaxBones.x && bodyPartCount == 0)
            {
                #region Detach Body Parts
                BodyPartController[] bpcs = root.GetComponentsInChildren<BodyPartController>();

                foreach (BodyPartController bpc in bpcs)
                {
                    DetachBodyPart(bpc);
                    bpc.transform.SetParent(null, true);
                }
                #endregion

                #region Tools
                backArrow.SetParent(root.GetChild(root.childCount - 2));

                backArrow.localPosition = frontArrow.localPosition = Vector3.zero;
                backArrow.localRotation = Quaternion.identity;
                #endregion

                #region Bone
                DestroyImmediate(root.GetChild(root.childCount - 1).gameObject);

                data.bones.RemoveAt(index);

                statistics.Complexity--;
                #endregion

                Setup();

                #region Reattach Body Parts
                foreach (BodyPartController bpc in bpcs)
                {
                    AttachBodyPart(bpc);
                }
                #endregion

                return true;
            }
            return false;
        }
        public bool RemoveFromFront()
        {
            UpdateBoneConfiguration();

            return Remove(0);
        }
        public bool RemoveFromBack()
        {
            UpdateBoneConfiguration();

            return Remove(root.childCount - 1);
        }

        public void SetupBodyPart(BodyPartController bpc)
        {
            BodyPartController flipped = Instantiate(bpc.gameObject, bpc.transform.parent).GetComponent<BodyPartController>();

            bpc.Flipped = flipped;
            flipped.Flipped = bpc;

            flipped.gameObject.name = bpc.gameObject.name + "(Flipped)";
            if (bpc is LimbController)
            {
                LimbController limb = bpc as LimbController;

                limbs.Add(limb);
                limbs.Add(limb.FlippedLimb);
            }
            else
            {
                flipped.Model.localScale = new Vector3(-flipped.Model.localScale.x, flipped.Model.localScale.y, flipped.Model.localScale.z);
            }

            #region Interact
            UnityAction onPress = delegate
            {
                CreatureCreator.Instance.CameraOrbit.Freeze();

                bpc.transform.SetParent(Dynamic.Transform);
                flipped.transform.SetParent(Dynamic.Transform);

                bpc.gameObject.SetLayerRecursively(LayerMask.NameToLayer("Ignore Raycast"), new List<string> { "Tools" });
                flipped.gameObject.SetLayerRecursively(LayerMask.NameToLayer("Ignore Raycast"), new List<string> { "Tools" });
            };
            UnityAction onRelease = delegate
            {
                CreatureCreator.Instance.CameraOrbit.Unfreeze();

                DetachBodyPart(bpc);
                DetachBodyPart(flipped);

                if (Physics.Raycast(RectTransformUtility.ScreenPointToRay(CreatureCreator.Instance.CameraOrbit.Camera, Input.mousePosition), out RaycastHit raycastHit) && raycastHit.collider.CompareTag("Player"))
                {
                    AttachBodyPart(bpc);
                    AttachBodyPart(flipped);
                }
                else
                {
                    audioSource.PlayOneShot(poofAudioClip);
                    Instantiate(poofEffect, bpc.Drag.IsPressing ? bpc.transform.position : flipped.transform.position, Quaternion.identity, Dynamic.Transform);
                    RemoveFromStatistics(bpc.name);

                    if (bpc is LimbController)
                    {
                        LimbController limb = bpc as LimbController;

                        limbs.Remove(limb);
                        limbs.Remove(limb.FlippedLimb);
                    }

                    Destroy(bpc.gameObject);
                    Destroy(flipped.gameObject);
                }
                
                bpc.gameObject.SetLayerRecursively(LayerMask.NameToLayer("Body"), new List<string> { "Tools" });
                flipped.gameObject.SetLayerRecursively(LayerMask.NameToLayer("Body"), new List<string> { "Tools" });

                bpc.Drag.Plane = flipped.Drag.Plane = new Plane(Vector3.right, Vector3.zero);
            };
            UnityAction onDrag = delegate
            {
                if (Physics.Raycast(RectTransformUtility.ScreenPointToRay(CreatureCreator.Instance.CameraOrbit.Camera, Input.mousePosition), out RaycastHit raycastHit) && raycastHit.collider.CompareTag("Player"))
                {
                    bpc.Drag.Draggable = false;

                    bpc.transform.position = raycastHit.point;
                    bpc.transform.rotation = Quaternion.LookRotation(raycastHit.normal);

                    if (Mathf.Abs(bpc.transform.position.x) > settings.MergeThreshold)
                    {
                        flipped.gameObject.SetActive(true);
                        flipped.transform.position = new Vector3(-bpc.transform.position.x, bpc.transform.position.y, bpc.transform.position.z);
                        flipped.transform.rotation = Quaternion.Euler(bpc.transform.rotation.eulerAngles.x, -bpc.transform.rotation.eulerAngles.y, -bpc.transform.rotation.eulerAngles.z);
                    }
                    else
                    {
                        flipped.gameObject.SetActive(false);
                        bpc.transform.position = new Vector3(0, bpc.transform.position.y, bpc.transform.position.z);
                        bpc.transform.rotation = Quaternion.LookRotation(new Vector3(0, raycastHit.normal.y, raycastHit.normal.z));

                        if (bpc is LimbController)
                        {
                            foreach (Transform bone in (bpc as LimbController).Bones)
                            {
                                bone.position = new Vector3(0, bone.position.y, bone.position.z);
                            }
                        }
                    }
                }
                else
                {
                    bpc.Drag.Draggable = true;
                    flipped.gameObject.SetActive(false);
                }
            };
            UnityAction onFlippedDrag = delegate
            {
                if (Physics.Raycast(RectTransformUtility.ScreenPointToRay(CreatureCreator.Instance.CameraOrbit.Camera, Input.mousePosition), out RaycastHit raycastHit) && raycastHit.collider.CompareTag("Player"))
                {
                    flipped.Drag.Draggable = false;

                    flipped.transform.position = raycastHit.point;
                    flipped.transform.rotation = Quaternion.LookRotation(raycastHit.normal);

                    if (Mathf.Abs(flipped.transform.position.x) > settings.MergeThreshold)
                    {
                        bpc.gameObject.SetActive(true);
                        bpc.transform.position = new Vector3(-flipped.transform.position.x, flipped.transform.position.y, flipped.transform.position.z);
                        bpc.transform.rotation = Quaternion.Euler(flipped.transform.rotation.eulerAngles.x, -flipped.transform.rotation.eulerAngles.y, -flipped.transform.rotation.eulerAngles.z);
                    }
                    else
                    {
                        bpc.gameObject.SetActive(false);
                        flipped.transform.position = new Vector3(0, flipped.transform.position.y, flipped.transform.position.z);
                        flipped.transform.rotation = Quaternion.LookRotation(new Vector3(0, raycastHit.normal.y, raycastHit.normal.z));

                        if (bpc is LimbController)
                        {
                            foreach (Transform bone in (flipped as LimbController).Bones)
                            {
                                bone.position = new Vector3(0, bone.position.y, bone.position.z);
                            }
                        }
                    }
                }
                else
                {
                    flipped.Drag.Draggable = true;
                    bpc.gameObject.SetActive(false);
                }
            };

            bpc.Drag.OnPress.AddListener(onPress);
            bpc.Drag.OnDrag.AddListener(onDrag);
            bpc.Drag.OnRelease.AddListener(onRelease);
            flipped.Drag.OnPress.AddListener(onPress);
            flipped.Drag.OnDrag.AddListener(onFlippedDrag);
            flipped.Drag.OnRelease.AddListener(onRelease);
            #endregion
        }
        public void AttachBodyPart(BodyPartController bpc)
        {
            #region Nearest Bone
            int nearestBoneIndex = -1;
            float minDistance = float.PositiveInfinity;

            for (int boneIndex = 0; boneIndex < root.childCount; boneIndex++)
            {
                float distance = Vector3.Distance(root.GetChild(boneIndex).position, bpc.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestBoneIndex = boneIndex;
                }
            }
            #endregion

            if (bpc.AttachedBodyPart == null)
            {
                AttachedBodyPart attachedBodyPart = new AttachedBodyPart(bpc.name.Replace("(Flipped)", ""), nearestBoneIndex, new SerializableTransform(bpc.transform));
                bpc.AttachedBodyPart = bpc.Flipped.AttachedBodyPart = attachedBodyPart;
                bpc.Drag.WorldBounds = bpc.Flipped.Drag.WorldBounds = new Bounds(new Vector3(0, 1, 0), new Vector3(4, 4, 4));

                data.attachedBodyParts.Add(attachedBodyPart);
            }
            bpc.transform.SetParent(root.GetChild(nearestBoneIndex), true);
        }
        public void DetachBodyPart(BodyPartController bpc)
        {
            for (int i = 0; i < data.attachedBodyParts.Count; i++)
            {
                if (data.attachedBodyParts[i] == bpc.AttachedBodyPart)
                {
                    data.attachedBodyParts.RemoveAt(i);
                    break;
                }
            }

            if (bpc.AttachedBodyPart != null)
            {
                bpc.AttachedBodyPart = bpc.Flipped.AttachedBodyPart = null;
            }
        }

        public float GetWeight(int index)
        {
            return skinnedMeshRenderer.GetBlendShapeWeight(index);
        }
        public void SetWeight(int index, float weight)
        {
            weight = Mathf.Clamp(weight, 0f, 100f);

            data.bones[index].Size = weight;
            skinnedMeshRenderer.SetBlendShapeWeight(index, weight);

            UpdateMeshCollider();
        }
        public void AddWeight(int index, float amount, int dir = 0)
        {
            SetWeight(index, GetWeight(index) + amount);

            if (index > 0 && (dir == -1 || dir == 0)) { AddWeight(index - 1, amount / 2f, -1); }
            if (index < data.bones.Count - 1 && (dir == 1 || dir == 0)) { AddWeight(index + 1, amount / 2f, 1); }
        }
        public void RemoveWeight(int index, float amount)
        {
            SetWeight(index, GetWeight(index) - amount);

            if (index > 0) { SetWeight(index - 1, GetWeight(index - 1) - amount / 2f); }
            if (index < data.bones.Count - 1) { SetWeight(index + 1, GetWeight(index + 1) - amount / 2f); }
        }

        public void SetColours(Color primaryColour, Color secondaryColour)
        {
            data.primaryColour = primaryColour;
            data.secondaryColour = secondaryColour;

            bodyMaterial.SetColor("_PrimaryCol", primaryColour);
            bodyMaterial.SetColor("_SecondaryCol", secondaryColour);
            bodyPartMaterial.color = primaryColour;
        }
        public void SetPattern(string patternID)
        {
            data.patternID = patternID;

            skinnedMeshRenderer.sharedMaterial.SetTexture("_PatternTex", DatabaseManager.GetDatabaseEntry<Texture>("Patterns", patternID));
        }

        public void SetTextured(bool textured)
        {
            mesh.uv = textured ? mesh.uv8 : null; // Must temporarily disable UVs for Quick Outline to work!
            skinnedMeshRenderer.sharedMaterial.SetTexture("_PatternTex", textured ? DatabaseManager.GetDatabaseEntry<Texture>("Patterns", data.patternID) : null);

            Textured = textured;
        }
        public void SetInteractable(bool interactable)
        {
            foreach (Collider collider in GetComponentsInChildren<Collider>())
            {
                collider.enabled = interactable;
            }
            Interactable = interactable;
        }
        public void SetSelected(bool selected)
        {
            outline.enabled = selected;

            SetBonesVisibility(selected);
            SetArrowsVisibility(selected);

            Selected = selected;
        }

        private void SetBonesVisibility(bool visible)
        {
            foreach (Transform bone in root)
            {
                bone.GetComponent<MeshRenderer>().enabled = visible;
            }
        }
        private void SetArrowsVisibility(bool visible)
        {
            frontArrow.gameObject.SetActive(visible);
            backArrow.gameObject.SetActive(visible);
        }
        private void SetBodyPartToolsVisibility(bool visible)
        {
        }

        public void AddToStatistics(string bodyPartID)
        {
            BodyPart bodyPart = DatabaseManager.GetDatabaseEntry<BodyPart>("Body Parts", bodyPartID);

            statistics.Complexity += bodyPart.Complexity;
            statistics.Health += bodyPart.Health;
            if (bodyPart is Mouth && statistics.Diet != Diet.Omnivore) // Omnivore is the preferred diet.
            {
                Mouth mouth = bodyPart as Mouth;
                if (mouth.Diet == Diet.Carnivore)
                {
                    statistics.Diet = (statistics.Diet == Diet.Herbivore) ? Diet.Omnivore : Diet.Carnivore;
                }
                else if (mouth.Diet == Diet.Herbivore)
                {
                    statistics.Diet = (statistics.Diet == Diet.Carnivore) ? Diet.Omnivore : Diet.Herbivore;
                }
                else
                {
                    statistics.Diet = Diet.Omnivore;
                }
            }
            else if (bodyPart is Limb)
            {
                statistics.Speed += (bodyPart as Limb).Speed;
            }

            CreatureCreator.Instance.AddCash(-bodyPart.Price);
        }
        public void RemoveFromStatistics(string bodyPartID)
        {
            BodyPart bodyPart = DatabaseManager.GetDatabaseEntry<BodyPart>("Body Parts", bodyPartID);

            statistics.Complexity -= bodyPart.Complexity;
            statistics.Health -= bodyPart.Health;
            if (bodyPart is Limb)
            {
                statistics.Speed -= (bodyPart as Limb).Speed;
            }

            statistics.Diet = Diet.None;
            foreach (AttachedBodyPart attachedBodyPart in data.attachedBodyParts)
            {
                if (bodyPart is Mouth)
                {
                    Mouth mouth = bodyPart as Mouth;
                    if (mouth.Diet == Diet.Carnivore)
                    {
                        statistics.Diet = (statistics.Diet == Diet.Herbivore) ? Diet.Omnivore : Diet.Carnivore;
                    }
                    else if (mouth.Diet == Diet.Herbivore)
                    {
                        statistics.Diet = (statistics.Diet == Diet.Carnivore) ? Diet.Omnivore : Diet.Herbivore;
                    }
                    else
                    {
                        statistics.Diet = Diet.Omnivore;
                    }

                    if (statistics.Diet == Diet.Omnivore) { break; } // Omnivore is the preferred diet.
                }
            }

            CreatureCreator.Instance.AddCash(bodyPart.Price);
        }

        private void UpdateBoneConfiguration()
        {
            for (int boneIndex = 0; boneIndex < data.bones.Count; boneIndex++)
            {
                data.bones[boneIndex].Position = root.GetChild(boneIndex).position;
                data.bones[boneIndex].Rotation = root.GetChild(boneIndex).rotation;
                data.bones[boneIndex].Size = skinnedMeshRenderer.GetBlendShapeWeight(boneIndex);
            }
        }
        private void UpdateAttachedBodyPartsConfiguration()
        {
            foreach (BodyPartController bpc in root.GetComponentsInChildren<BodyPartController>())
            {
                if (bpc.AttachedBodyPart == null || bpc.name.EndsWith("(Flipped)")) { continue; }

                bpc.AttachedBodyPart.Transform = new SerializableTransform(bpc.transform);
            }
        }
        private void UpdateMeshCollider()
        {
            Mesh skinnedMesh = new Mesh();
            skinnedMeshRenderer.BakeMesh(skinnedMesh);
            meshCollider.sharedMesh = skinnedMesh;
        }
        #endregion

        #region Debug
        private List<Vector3> gizmoVertices = new List<Vector3>();
        private Coroutine displayVerticesCoroutine;
        private float displayInterval = 0.001f;

        private void OnDrawGizmos()
        {
            if (gizmoVertices == null || gizmoVertices.Count == 0) { return; }

            foreach (Vector3 vertex in gizmoVertices)
            {
                Gizmos.DrawSphere(vertex + transform.position, 0.005f);
            }
        }

        [ContextMenu("Debug/Display")]
        public void DisplayVertices()
        {
            HideVertices();
            displayVerticesCoroutine = StartCoroutine(DisplayVerticesRoutine(displayInterval));
        }
        private IEnumerator DisplayVerticesRoutine(float time)
        {
            foreach (Vector3 vertex in mesh.vertices)
            {
                gizmoVertices.Add(vertex);

                if (time > 0)
                {
                    yield return new WaitForSeconds(time);
                }
            }
        }
        [ContextMenu("Debug/Hide")]
        private void HideVertices()
        {
            if (displayVerticesCoroutine != null)
            {
                StopCoroutine(displayVerticesCoroutine);
            }

            gizmoVertices.Clear();
        }
        #endregion
    }
}