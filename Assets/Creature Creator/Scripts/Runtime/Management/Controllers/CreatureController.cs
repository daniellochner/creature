// Creature Creator
// Version: 1.0.0
// Author: Daniel Lochner

using BasicTools.ButtonInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace DanielLochner.Assets.CreatureCreator
{
    public class CreatureController : MonoBehaviour
    {
        #region Fields
        [SerializeField] private CameraOrbit cameraOrbit;

        [Header("Settings")]
        [SerializeField] private int maximumComplexity = 20;
        [SerializeField] private float mergeThreshold = 0.01f;
        [SerializeField] private Vector2Int minMaxBones = new Vector2Int(2, 20);
        [SerializeField] private BoneSettings boneSettings;
        [SerializeField] private CreatureData creatureData;

        [Header("Tools")]
        [SerializeField] private GameObject boneTool;
        [SerializeField] private GameObject pivotTool;
        [SerializeField] private GameObject rotateTool;
        [SerializeField] private GameObject stretchTool;

        private SkinnedMeshRenderer skinnedMeshRenderer;
        private MeshCollider meshCollider;
        private Mesh mesh;
        private Outline outline;
        private Transform root, frontArrow, backArrow;
        #endregion

        #region Properties
        public int MaximumComplexity { get { return maximumComplexity; } }

        public CreatureData CreatureData { get { return creatureData; } }

        public bool Selected { get; private set; }
        public bool Textured { get; private set; }
        #endregion

        #region Methods
        private void Awake()
        {
            Initialize();
        }

        private void Initialize()
        {
            #region Body
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
            skinnedMeshRenderer.material = new Material(Shader.Find("Creature Creator/Body"));

            outline = model.AddComponent<Outline>();
            outline.OutlineWidth = 5f;

            meshCollider = gameObject.AddComponent<MeshCollider>();
            mesh.name = "Body";
            #endregion

            #region Tools
            Drag drag = gameObject.AddComponent<Drag>();
            drag.OnPress.AddListener(delegate
            {
                cameraOrbit.Freeze();
            });
            drag.OnRelease.AddListener(delegate
            {
                cameraOrbit.Unfreeze();
                UpdateBoneConfiguration();
            });

            Hover hover = gameObject.AddComponent<Hover>();
            hover.OnEnter.AddListener(delegate
            {
                if (!Input.GetMouseButton(0))
                {
                    cameraOrbit.Freeze();
                    SetBonesVisibility(true);
                }
            });
            hover.OnExit.AddListener(delegate
            {
                if (!Input.GetMouseButton(0))
                {
                    cameraOrbit.Unfreeze();

                    if (!Selected)
                    {
                        SetBonesVisibility(false);
                    }
                }
            });

            Click click = gameObject.AddComponent<Click>();
            click.OnClick.AddListener(delegate
            {
                SetSelected(true);
            });

            backArrow = Instantiate(stretchTool).transform;
            frontArrow = Instantiate(stretchTool).transform;
            frontArrow.Find("Model").localPosition = backArrow.Find("Model").localPosition = Vector3.forward * (boneSettings.Length / 2f + boneSettings.Radius + 0.05f);

            Drag frontArrowDrag = frontArrow.GetComponentInChildren<Drag>();
            frontArrowDrag.OnPress.AddListener(delegate
            {
                cameraOrbit.Freeze();
            });
            frontArrowDrag.OnRelease.AddListener(delegate
            {
                cameraOrbit.Unfreeze();
            });
            frontArrowDrag.OnDrag.AddListener(delegate
            {
                Vector3 displacement = frontArrowDrag.TargetWorldPosition - frontArrow.position;
                if (displacement.magnitude > boneSettings.Length)
                {
                    if (Vector3.Dot(displacement.normalized, frontArrow.forward) > 0.1f && (creatureData.bones.Count + 1) <= minMaxBones.y)
                    {
                        UpdateBoneConfiguration();

                        Vector3 direction = (frontArrowDrag.TargetMousePosition - frontArrow.position).normalized;
                        Vector3 position = creatureData.bones[0].Position + direction * boneSettings.Length;
                        Quaternion rotation = Quaternion.LookRotation(-direction, frontArrow.up);

                        Add(0, position, rotation, Mathf.Clamp(creatureData.bones[0].Size * 0.75f, 0f, 100f));

                        frontArrowDrag.OnMouseDown();
                    }
                    else if (Vector3.Dot(displacement.normalized, frontArrow.forward) < -0.1f)
                    {
                        RemoveFromFront();
                    }
                }
            });

            Drag backArrowDrag = backArrow.GetComponentInChildren<Drag>();
            backArrowDrag.OnPress.AddListener(delegate
            {
                cameraOrbit.Freeze();
            });
            backArrowDrag.OnRelease.AddListener(delegate
            {
                cameraOrbit.Unfreeze();
            });
            backArrowDrag.OnDrag.AddListener(delegate
            {
                Vector3 displacement = backArrowDrag.TargetWorldPosition - backArrow.position;
                if (displacement.magnitude > boneSettings.Length)
                {
                    if (Vector3.Dot(displacement.normalized, backArrow.forward) > 0.1f && (creatureData.bones.Count + 1) <= minMaxBones.y)
                    {
                        UpdateBoneConfiguration();

                        Vector3 direction = (backArrowDrag.TargetMousePosition - backArrow.position).normalized;
                        Vector3 position = creatureData.bones[root.childCount - 1].Position + direction * boneSettings.Length;
                        Quaternion rotation = Quaternion.LookRotation(direction, backArrow.up);

                        Add(root.childCount, position, rotation, Mathf.Clamp(creatureData.bones[root.childCount - 1].Size * 0.75f, 0f, 100f));

                        backArrowDrag.OnMouseDown();
                    }
                    else if (Vector3.Dot(displacement.normalized, backArrow.forward) < -0.1f)
                    {
                        RemoveFromBack();
                    }
                }
            });
            #endregion
        }

        public void Save(string creatureName)
        {
            CreatureData creatureData = new CreatureData()
            {
                bones = this.creatureData.bones,
                attachedBodyParts = this.creatureData.attachedBodyParts,

                patternID = this.creatureData.patternID,
                primaryColour = this.creatureData.primaryColour,
                secondaryColour = this.creatureData.secondaryColour
            };

            SaveUtility.Save(JsonUtility.ToJson(creatureData), creatureName + ".json");
        }
        public void Load(string creatureName)
        {
            CreatureData creatureData = JsonUtility.FromJson<CreatureData>(SaveUtility.Load(creatureName + ".json"));

            Revert();

            for (int i = 0; i < creatureData.bones.Count; i++)
            {
                Bone bone = creatureData.bones[i];
                Add(i, bone.Position, bone.Rotation, bone.Size);
            }
            foreach (AttachedBodyPart attachedBodyPart in creatureData.attachedBodyParts)
            {
                BodyPartController bpc = Instantiate(DatabaseManager.GetDatabaseEntry<BodyPart>("Body Parts", attachedBodyPart.BodyPartID).Prefab, root.GetChild(attachedBodyPart.BoneIndex)).GetComponent<BodyPartController>();
                bpc.gameObject.name = attachedBodyPart.BodyPartID;

                bpc.transform.position = attachedBodyPart.Position;
                bpc.transform.rotation = attachedBodyPart.Rotation;

                SetupBodyPart(bpc);

                bpc.flipped.transform.position = new Vector3(-bpc.transform.position.x, bpc.transform.position.y, bpc.transform.position.z);
                bpc.flipped.transform.rotation = Quaternion.Euler(bpc.transform.rotation.eulerAngles.x, -bpc.transform.rotation.eulerAngles.y, -bpc.transform.rotation.eulerAngles.z);
            }
            SetColours(creatureData.primaryColour, creatureData.secondaryColour);
            SetPattern(creatureData.patternID);

            SetSelected(false);
        }
        public void Revert()
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
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
            transform.position = new Vector3(0f, 0.75f, 0f);

            creatureData = new CreatureData();

            Initialize();
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
            for (int ringIndex = 1; ringIndex < boneSettings.Segments / 2; ringIndex++)
            {
                float percent = (float)ringIndex / (boneSettings.Segments / 2);
                float ringRadius = boneSettings.Radius * Mathf.Sin(90f * percent * Mathf.Deg2Rad);
                float ringDistance = boneSettings.Radius * (-Mathf.Cos(90f * percent * Mathf.Deg2Rad) + 1f);

                for (int i = 0; i < boneSettings.Segments; i++)
                {
                    float angle = i * 360f / boneSettings.Segments;

                    float x = ringRadius * Mathf.Cos(angle * Mathf.Deg2Rad);
                    float y = ringRadius * Mathf.Sin(angle * Mathf.Deg2Rad);
                    float z = ringDistance;

                    vertices.Add(new Vector3(x, y, z));
                    boneWeights.Add(new BoneWeight() { boneIndex0 = 0, weight0 = 1f });
                }
            }

            // Middle Cylinder.
            for (int ringIndex = 0; ringIndex < boneSettings.Rings * creatureData.bones.Count; ringIndex++)
            {
                float boneIndexFloat = (float)ringIndex / boneSettings.Rings;
                int boneIndex = Mathf.FloorToInt(boneIndexFloat);

                float bonePercent = boneIndexFloat - boneIndex;

                int boneIndex0 = (boneIndex > 0) ? boneIndex - 1 : 0;
                int boneIndex2 = (boneIndex < creatureData.bones.Count - 1) ? boneIndex + 1 : creatureData.bones.Count - 1;
                int boneIndex1 = boneIndex;

                float weight0 = (boneIndex > 0) ? (1f - bonePercent) * 0.5f : 0f;
                float weight2 = (boneIndex < creatureData.bones.Count - 1) ? bonePercent * 0.5f : 0f;
                float weight1 = 1f - (weight0 + weight2);

                for (int i = 0; i < boneSettings.Segments; i++)
                {
                    float angle = i * 360f / boneSettings.Segments;

                    float x = boneSettings.Radius * Mathf.Cos(angle * Mathf.Deg2Rad);
                    float y = boneSettings.Radius * Mathf.Sin(angle * Mathf.Deg2Rad);
                    float z = ringIndex * boneSettings.Length / boneSettings.Rings;

                    vertices.Add(new Vector3(x, y, boneSettings.Radius + z));
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
            for (int ringIndex = 0; ringIndex < boneSettings.Segments / 2; ringIndex++)
            {
                float percent = (float)ringIndex / (boneSettings.Segments / 2);
                float ringRadius = boneSettings.Radius * Mathf.Cos(90f * percent * Mathf.Deg2Rad);
                float ringDistance = boneSettings.Radius * Mathf.Sin(90f * percent * Mathf.Deg2Rad);

                for (int i = 0; i < boneSettings.Segments; i++)
                {
                    float angle = i * 360f / boneSettings.Segments;

                    float x = ringRadius * Mathf.Cos(angle * Mathf.Deg2Rad);
                    float y = ringRadius * Mathf.Sin(angle * Mathf.Deg2Rad);
                    float z = ringDistance;

                    vertices.Add(new Vector3(x, y, boneSettings.Radius + (boneSettings.Length * creatureData.bones.Count) + z));
                    boneWeights.Add(new BoneWeight() { boneIndex0 = creatureData.bones.Count - 1, weight0 = 1 });
                }
            }
            vertices.Add(new Vector3(0, 0, 2f * boneSettings.Radius + (boneSettings.Length * creatureData.bones.Count)));
            boneWeights.Add(new BoneWeight() { boneIndex0 = creatureData.bones.Count - 1, weight0 = 1 });

            mesh.vertices = vertices.ToArray();
            mesh.boneWeights = boneWeights.ToArray();
            #endregion

            #region Triangles
            List<int> triangles = new List<int>();

            // Top Cap.
            for (int i = 0; i < boneSettings.Segments; i++)
            {
                int seamOffset = i != boneSettings.Segments - 1 ? 0 : boneSettings.Segments;

                triangles.Add(0);
                triangles.Add(i + 2 - seamOffset);
                triangles.Add(i + 1);
            }

            // Main.
            int rings = (boneSettings.Rings * creatureData.bones.Count) + (2 * (boneSettings.Segments / 2 - 1));
            for (int ringIndex = 0; ringIndex < rings; ringIndex++)
            {
                int ringOffset = 1 + ringIndex * boneSettings.Segments;

                for (int i = 0; i < boneSettings.Segments; i++)
                {
                    int seamOffset = i != boneSettings.Segments - 1 ? 0 : boneSettings.Segments;

                    triangles.Add(ringOffset + i);
                    triangles.Add(ringOffset + i + 1 - seamOffset);
                    triangles.Add(ringOffset + i + 1 - seamOffset + boneSettings.Segments);

                    triangles.Add(ringOffset + i + 1 - seamOffset + boneSettings.Segments);
                    triangles.Add(ringOffset + i + boneSettings.Segments);
                    triangles.Add(ringOffset + i);
                }
            }

            // Bottom Cap.
            int topIndex = 1 + (rings + 1) * boneSettings.Segments;
            for (int i = 0; i < boneSettings.Segments; i++)
            {
                int seamOffset = i != boneSettings.Segments - 1 ? 0 : boneSettings.Segments;

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
                for (int i = 0; i < boneSettings.Segments; i++)
                {
                    float u = i / (float)(boneSettings.Segments - 1);
                    uv.Add(new Vector2(u, v * (creatureData.bones.Count + 1)));
                }
            }
            uv.Add(Vector2.one);

            mesh.uv8 = uv.ToArray(); // Store copy of UVs in mesh.
            #endregion

            #region Bones
            Transform[] boneTransforms = new Transform[creatureData.bones.Count];
            Matrix4x4[] bindPoses = new Matrix4x4[creatureData.bones.Count];
            Vector3[] deltaZeroArray = new Vector3[vertices.Count];
            for (int vertIndex = 0; vertIndex < vertices.Count; vertIndex++)
            {
                deltaZeroArray[vertIndex] = Vector3.zero;
            }

            for (int boneIndex = 0; boneIndex < creatureData.bones.Count; boneIndex++)
            {
                boneTransforms[boneIndex] = root.GetChild(boneIndex);

                #region Bind Pose
                boneTransforms[boneIndex].localPosition = Vector3.forward * (boneSettings.Radius + boneSettings.Length * (0.5f + boneIndex));
                boneTransforms[boneIndex].localRotation = Quaternion.identity;
                bindPoses[boneIndex] = boneTransforms[boneIndex].worldToLocalMatrix * transform.localToWorldMatrix;
                #endregion

                #region Hinge Joint
                if (boneIndex > 0)
                {
                    HingeJoint hingeJoint = boneTransforms[boneIndex].GetComponent<HingeJoint>();
                    hingeJoint.anchor = new Vector3(0, 0, -boneSettings.Length / 2f);
                    hingeJoint.connectedBody = boneTransforms[boneIndex - 1].GetComponent<Rigidbody>();
                }
                #endregion

                #region Blend Shapes
                Vector3[] deltaVertices = new Vector3[vertices.Count];
                for (int vertIndex = 0; vertIndex < vertices.Count; vertIndex++)
                {
                    // ROUND
                    //float distanceToBone = Mathf.Clamp(Vector3.Distance(vertices[vertIndex], boneTransforms[boneIndex].localPosition), 0, 2f * boneSettings.Length);
                    //Vector3 directionToBone = (vertices[vertIndex] - boneTransforms[boneIndex].localPosition).normalized;

                    //deltaVertices[vertIndex] = directionToBone * (2f * boneSettings.Length - distanceToBone);


                    // SMOOTH - https://www.desmos.com/calculator/wmpvvtmor8
                    float maxDistanceAlongBone = boneSettings.Length * 2f;
                    float maxHeightAboveBone = boneSettings.Radius * 2f;

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
                    AddWeight(index, 5f);
                });
                scroll.OnScrollDown.AddListener(delegate 
                {
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
            for (int boneIndex = 0; boneIndex < creatureData.bones.Count; boneIndex++)
            {
                boneTransforms[boneIndex].position = creatureData.bones[boneIndex].Position;
                boneTransforms[boneIndex].rotation = creatureData.bones[boneIndex].Rotation;
                SetWeight(boneIndex, creatureData.bones[boneIndex].Size);
            }

            UpdateMeshCollider();
            #endregion
        }
        
        public void Add(int index, Vector3 position, Quaternion rotation, float size)
        {
            if ((creatureData.bones.Count + 1) <= minMaxBones.y)
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

                if (creatureData.bones.Count == 0)
                {
                    DestroyImmediate(boneGameObject.GetComponent<HingeJoint>());
                }
                #endregion

                #region Tools
                Drag drag = boneGameObject.GetComponent<Drag>();
                drag.OnPress.AddListener(delegate
                {
                    cameraOrbit.Freeze();
                });
                drag.OnRelease.AddListener(delegate
                {
                    cameraOrbit.Unfreeze();
                    UpdateMeshCollider();
                    UpdateBoneConfiguration();
                });

                Hover hover = boneGameObject.GetComponent<Hover>();
                hover.OnEnter.AddListener(delegate
                {
                    if (!Input.GetMouseButton(0))
                    {
                        cameraOrbit.Freeze();
                        SetBonesVisibility(true);
                    }
                });
                hover.OnExit.AddListener(delegate
                {
                    if (!Input.GetMouseButton(0))
                    {
                        cameraOrbit.Unfreeze();

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

                creatureData.bones.Insert(index, new Bone(position, rotation, size));

                Setup();

                #region Reattach Body Parts
                foreach (BodyPartController bpc in bpcs)
                {
                    AttachBodyPart(bpc);
                }
                #endregion
            }
        }
        public void AddToFront()
        {
            UpdateBoneConfiguration();

            Vector3 position = creatureData.bones[0].Position - root.GetChild(0).forward * boneSettings.Length;
            Quaternion rotation = creatureData.bones[0].Rotation;

            Add(0, position, rotation, Mathf.Clamp(creatureData.bones[0].Size * 0.75f, 0f, 100f));
        }
        public void AddToBack()
        {
            UpdateBoneConfiguration();

            Vector3 position = creatureData.bones[creatureData.bones.Count - 1].Position + root.GetChild(creatureData.bones.Count - 1).forward * boneSettings.Length;
            Quaternion rotation = creatureData.bones[creatureData.bones.Count - 1].Rotation;

            Add(creatureData.bones.Count, position, rotation, Mathf.Clamp(creatureData.bones[creatureData.bones.Count - 1].Size * 0.75f, 0f, 100f));
        }

        public void Remove(int index)
        {
            int bodyPartCount = 0;
            foreach (AttachedBodyPart attachedBodyPart in creatureData.attachedBodyParts)
            {
                if (attachedBodyPart.BoneIndex == index)
                {
                    bodyPartCount++;
                }
            }

            if ((creatureData.bones.Count - 1) >= minMaxBones.x && bodyPartCount == 0)
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

                creatureData.bones.RemoveAt(index);
                #endregion

                Setup();

                #region Reattach Body Parts
                foreach (BodyPartController bpc in bpcs)
                {
                    AttachBodyPart(bpc);
                }
                #endregion
            }
        }
        public void RemoveFromFront()
        {
            UpdateBoneConfiguration();

            Remove(0);
        }
        public void RemoveFromBack()
        {
            UpdateBoneConfiguration();

            Remove(root.childCount - 1);
        }

        public void SetupBodyPart(BodyPartController bpc)
        {
            BodyPartController flipped = Instantiate(bpc.gameObject, bpc.transform.parent).GetComponent<BodyPartController>();

            flipped.gameObject.name = bpc.gameObject.name;

            bpc.flipped = flipped;
            flipped.flipped = bpc;

            UnityAction onPress = delegate
            {
                bpc.transform.SetParent(Dynamic.Transform);
                flipped.transform.SetParent(Dynamic.Transform);

                bpc.gameObject.layer = flipped.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
                cameraOrbit.Freeze();
            };
            UnityAction onRelease = delegate
            {
                DetachBodyPart(bpc);
                DetachBodyPart(flipped);

                if (Physics.Raycast(RectTransformUtility.ScreenPointToRay(cameraOrbit.Camera, Input.mousePosition), out RaycastHit raycastHit) && raycastHit.collider.CompareTag("Player"))
                {
                    AttachBodyPart(bpc);
                    AttachBodyPart(flipped);
                }
                else
                {
                    Destroy(bpc.gameObject);
                    Destroy(flipped.gameObject);
                }

                bpc.gameObject.layer = flipped.gameObject.layer = LayerMask.NameToLayer("Body");
                cameraOrbit.Unfreeze();

                bpc.drag.Plane = flipped.drag.Plane = new Plane(Vector3.right, Vector3.zero);

                CreatureCreator.Instance.UpdateStatistics();
            };
            UnityAction onDrag = delegate
            {
                if (Physics.Raycast(RectTransformUtility.ScreenPointToRay(cameraOrbit.Camera, Input.mousePosition), out RaycastHit raycastHit) && raycastHit.collider.CompareTag("Player"))
                {
                    bpc.drag.Draggable = false;

                    bpc.transform.position = raycastHit.point;
                    bpc.transform.rotation = Quaternion.LookRotation(raycastHit.normal);

                    if (Mathf.Abs(bpc.transform.position.x) > mergeThreshold)
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
                    }
                }
                else
                {
                    bpc.drag.Draggable = true;
                    flipped.gameObject.SetActive(false);
                }
            };
            UnityAction onFlippedDrag = delegate
            {
                if (Physics.Raycast(RectTransformUtility.ScreenPointToRay(cameraOrbit.Camera, Input.mousePosition), out RaycastHit raycastHit) && raycastHit.collider.CompareTag("Player"))
                {
                    flipped.drag.Draggable = false;

                    flipped.transform.position = raycastHit.point;
                    flipped.transform.rotation = Quaternion.LookRotation(raycastHit.normal);

                    if (Mathf.Abs(flipped.transform.position.x) > mergeThreshold)
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
                    }
                }
                else
                {
                    flipped.drag.Draggable = true;
                    bpc.gameObject.SetActive(false);
                }
            };

            bpc.drag.OnPress.AddListener(onPress);
            bpc.drag.OnDrag.AddListener(onDrag);
            bpc.drag.OnRelease.AddListener(onRelease);
            flipped.drag.OnPress.AddListener(onPress);
            flipped.drag.OnDrag.AddListener(onFlippedDrag);
            flipped.drag.OnRelease.AddListener(onRelease);
        }
        public void AttachBodyPart(BodyPartController bpc)
        {
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

            if (bpc.attachedBodyPart == null)
            {
                AttachedBodyPart attachedBodyPart = new AttachedBodyPart(bpc.name, nearestBoneIndex, bpc.transform.position, bpc.transform.rotation);
                bpc.attachedBodyPart = bpc.flipped.attachedBodyPart = attachedBodyPart;

                creatureData.attachedBodyParts.Add(attachedBodyPart);
            }
            bpc.transform.SetParent(root.GetChild(nearestBoneIndex), true);
        }
        public void DetachBodyPart(BodyPartController bpc)
        {
            for (int i = 0; i < creatureData.attachedBodyParts.Count; i++)
            {
                if (creatureData.attachedBodyParts[i] == bpc.attachedBodyPart)
                {
                    creatureData.attachedBodyParts.RemoveAt(i);
                    break;
                }
            }
            bpc.attachedBodyPart = bpc.flipped.attachedBodyPart = null;
        }

        public float GetWeight(int index)
        {
            return skinnedMeshRenderer.GetBlendShapeWeight(index);
        }
        public void SetWeight(int index, float weight)
        {
            weight = Mathf.Clamp(weight, 0f, 100f);

            creatureData.bones[index].Size = weight;
            skinnedMeshRenderer.SetBlendShapeWeight(index, weight);

            UpdateMeshCollider();
        }
        public void AddWeight(int index, float amount, int dir = 0)
        {
            SetWeight(index, GetWeight(index) + amount);

            if (index > 0 && (dir == -1 || dir == 0)) { AddWeight(index - 1, amount / 2f, -1); }
            if (index < creatureData.bones.Count - 1 && (dir == 1 || dir == 0)) { AddWeight(index + 1, amount / 2f, 1); }
        }
        public void RemoveWeight(int index, float amount)
        {
            SetWeight(index, GetWeight(index) - amount);

            if (index > 0) { SetWeight(index - 1, GetWeight(index - 1) - amount / 2f); }
            if (index < creatureData.bones.Count - 1) { SetWeight(index + 1, GetWeight(index + 1) - amount / 2f); }
        }

        public void SetColours(Color primaryColour, Color secondaryColour)
        {
            creatureData.primaryColour = primaryColour;
            creatureData.secondaryColour = secondaryColour;

            skinnedMeshRenderer.material.SetColor("_PrimaryCol", primaryColour);
            skinnedMeshRenderer.material.SetColor("_SecondaryCol", secondaryColour);
        }
        public void SetPattern(string patternID)
        {
            creatureData.patternID = patternID;

            skinnedMeshRenderer.material.SetTexture("_PatternTex", DatabaseManager.GetDatabaseEntry<Texture>("Patterns", patternID));
        }

        private void UpdateBoneConfiguration()
        {
            for (int boneIndex = 0; boneIndex < creatureData.bones.Count; boneIndex++)
            {
                creatureData.bones[boneIndex].Position = root.GetChild(boneIndex).position;
                creatureData.bones[boneIndex].Rotation = root.GetChild(boneIndex).rotation;
                creatureData.bones[boneIndex].Size = skinnedMeshRenderer.GetBlendShapeWeight(boneIndex);
            }
        }
        private void UpdateMeshCollider()
        {
            Mesh skinnedMesh = new Mesh();
            skinnedMeshRenderer.BakeMesh(skinnedMesh);
            meshCollider.sharedMesh = skinnedMesh;
        }

        public void SetTextured(bool textured)
        {
            mesh.uv = textured ? mesh.uv8 : null; // Must temporarily disable UVs for Quick Outline to work!
            skinnedMeshRenderer.material.SetTexture("_PatternTex", textured ? DatabaseManager.GetDatabaseEntry<Texture>("Patterns", creatureData.patternID) : null);

            this.Textured = textured;
        }
        public void SetInteractable(bool interactable)
        {
            foreach (Collider collider in GetComponentsInChildren<Collider>())
            {
                collider.enabled = interactable;
            }
        }
        public void SetSelected(bool selected)
        {
            outline.enabled = selected;

            SetBonesVisibility(selected);
            SetArrowsVisibility(selected);

            this.Selected = selected;
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
            frontArrow.GetComponentInChildren<MeshRenderer>().enabled = visible;
            backArrow.GetComponentInChildren<MeshRenderer>().enabled = visible;

            frontArrow.GetComponentInChildren<Collider>().enabled = visible;
            backArrow.GetComponentInChildren<Collider>().enabled = visible;
        }
        private void SetBodyPartToolsVisibility(bool visible)
        {
        }

        #region Debugging
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
        #endregion

        #region Inner Classes
        [Serializable] public class BoneSettings
        {
            #region Fields
            [SerializeField] private float radius = 0.05f;
            [SerializeField] private float length = 0.1f;
            [SerializeField] [Range(4, 25)] private int segments = 12;
            [SerializeField] [Range(2, 25)] private int rings = 4;
            #endregion

            #region Properties
            public float Radius { get { return radius; } }
            public float Length { get { return length; } }
            public int Segments { get { return segments; } }
            public int Rings { get { return rings; } }
            #endregion
        }

        [Serializable] public class Bone
        {
            #region Fields
            [SerializeField] private Vector3 position;
            [SerializeField] private Quaternion rotation;
            [SerializeField] private float size;
            #endregion

            #region Properties
            public Vector3 Position { get { return position; } set { position = value; } }
            public Quaternion Rotation { get { return rotation; } set { rotation = value; } }
            public float Size { get { return size; } set { size = value; } }
            #endregion

            #region Methods
            public Bone(Vector3 position, Quaternion rotation, float size)
            {
                this.position = position;
                this.rotation = rotation;
                this.size = size;
            }
            #endregion
        }

        [Serializable] public class AttachedBodyPart
        {
            #region Fields
            [SerializeField] private string bodyPartID;
            [SerializeField] private int boneIndex;
            [SerializeField] private Vector3 position;
            [SerializeField] private Quaternion rotation;
            #endregion

            #region Properties
            public string BodyPartID { get { return bodyPartID; } }
            public int BoneIndex { get { return boneIndex; } }
            public Vector3 Position { get { return position; } }
            public Quaternion Rotation { get { return rotation; } }
            #endregion

            #region Methods
            public AttachedBodyPart(string bodyPartID, int boneIndex, Vector3 position, Quaternion rotation)
            {
                this.bodyPartID = bodyPartID;
                this.boneIndex = boneIndex;
                this.position = position;
                this.rotation = rotation;
            }
            #endregion
        }
        #endregion
    }
}