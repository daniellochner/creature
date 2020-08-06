// Creature Creator
// Version: 1.0.0
// Author: Daniel Lochner

using BasicTools.ButtonInspector;
using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DanielLochner.Assets.CreatureCreator
{
    public class Body : MonoBehaviour
    {
        #region Fields
        [SerializeField] private BodySettings bodySettings;

        [Header("Tools")]
        [SerializeField] private GameObject boneTool;
        [SerializeField] private GameObject pivotTool;
        [SerializeField] private GameObject rotateTool;
        [SerializeField] private GameObject stretchTool;

        [Space]

        [SerializeField] [Button("Add to Front", "AddToFront")] private bool addToFront;
        [SerializeField] [Button("Add to Back", "AddToBack")] private bool addToBack;
        [SerializeField] [Button("Remove from Front", "RemoveFromFront")] private bool removeFromFront;
        [SerializeField] [Button("Remove from Back", "RemoveFromBack")] private bool removeFromBack;

        private SkinnedMeshRenderer skinnedMeshRenderer;
        private MeshCollider meshCollider;
        private Transform root;
        private Mesh mesh;

        private List<Bone> bones = new List<Bone>();
        #endregion

        #region Properties
        public static bool IsModifyingMesh { get; set; }
        #endregion

        #region Methods
        private void Awake()
        {
            Initialize();
        }

        private void Initialize()
        {
            if (!mesh)
            {
                GameObject model = new GameObject("Model");
                model.transform.SetParent(transform);
                model.transform.localPosition = Vector3.zero;
                model.transform.localRotation = Quaternion.identity;

                root = new GameObject("Root").transform;
                root.SetParent(transform);
                root.localPosition = Vector3.zero;
                root.localRotation = Quaternion.identity;

                skinnedMeshRenderer = model.AddComponent<SkinnedMeshRenderer>();
                skinnedMeshRenderer.sharedMaterial = new Material(Shader.Find("Standard"));
                skinnedMeshRenderer.sharedMesh = mesh = model.AddComponent<MeshFilter>().sharedMesh = new Mesh();
                skinnedMeshRenderer.rootBone = root.transform;

                meshCollider = model.AddComponent<MeshCollider>();

                model.AddComponent<Test>();

                mesh.name = "Body";
            }

            Add(0, Vector3.zero, Quaternion.identity, 0f);
            AddToBack();

            Setup();
        }
        private void Setup()
        {
            #region Mesh Generation
            mesh.Clear();

            #region Vertices
            List<Vector3> vertices = new List<Vector3>();
            List<BoneWeight> boneWeights = new List<BoneWeight>();

            // Top Hemisphere.
            vertices.Add(new Vector3(0, 0, 0));
            boneWeights.Add(new BoneWeight() { boneIndex0 = 0, weight0 = 1 });
            for (int ringIndex = 1; ringIndex < bodySettings.Segments / 2; ringIndex++)
            {
                float percent = (float)ringIndex / (bodySettings.Segments / 2);
                float ringRadius = bodySettings.Radius * Mathf.Sin(90f * percent * Mathf.Deg2Rad);
                float ringDistance = bodySettings.Radius * (-Mathf.Cos(90f * percent * Mathf.Deg2Rad) + 1f);

                for (int i = 0; i < bodySettings.Segments; i++)
                {
                    float angle = i * 360f / bodySettings.Segments;

                    float x = ringRadius * Mathf.Cos(angle * Mathf.Deg2Rad);
                    float y = ringRadius * Mathf.Sin(angle * Mathf.Deg2Rad);
                    float z = ringDistance;

                    vertices.Add(new Vector3(x, y, z));
                    boneWeights.Add(new BoneWeight() { boneIndex0 = 0, weight0 = 1f });
                }
            }

            // Middle Cylinder.
            for (int ringIndex = 0; ringIndex < bodySettings.Rings * bones.Count; ringIndex++)
            {
                float boneIndexFloat = (float)ringIndex / bodySettings.Rings;
                int boneIndex = Mathf.FloorToInt(boneIndexFloat);

                float bonePercent = boneIndexFloat - boneIndex;

                int boneIndex0 = (boneIndex > 0) ? boneIndex - 1 : 0;
                int boneIndex2 = (boneIndex < bones.Count - 1) ? boneIndex + 1 : bones.Count - 1;
                int boneIndex1 = boneIndex;

                float weight0 = (boneIndex > 0) ? (1f - bonePercent) * 0.5f : 0f;
                float weight2 = (boneIndex < bones.Count - 1) ? bonePercent * 0.5f : 0f;
                float weight1 = 1f - (weight0 + weight2);

                for (int i = 0; i < bodySettings.Segments; i++)
                {
                    float angle = i * 360f / bodySettings.Segments;

                    float x = bodySettings.Radius * Mathf.Cos(angle * Mathf.Deg2Rad);
                    float y = bodySettings.Radius * Mathf.Sin(angle * Mathf.Deg2Rad);
                    float z = ringIndex * bodySettings.Length / bodySettings.Rings;

                    vertices.Add(new Vector3(x, y, bodySettings.Radius + z));
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
            for (int ringIndex = 0; ringIndex < bodySettings.Segments / 2; ringIndex++)
            {
                float percent = (float)ringIndex / (bodySettings.Segments / 2);
                float ringRadius = bodySettings.Radius * Mathf.Cos(90f * percent * Mathf.Deg2Rad);
                float ringDistance = bodySettings.Radius * Mathf.Sin(90f * percent * Mathf.Deg2Rad);

                for (int i = 0; i < bodySettings.Segments; i++)
                {
                    float angle = i * 360f / bodySettings.Segments;

                    float x = ringRadius * Mathf.Cos(angle * Mathf.Deg2Rad);
                    float y = ringRadius * Mathf.Sin(angle * Mathf.Deg2Rad);
                    float z = ringDistance;

                    vertices.Add(new Vector3(x, y, bodySettings.Radius + (bodySettings.Length * bones.Count) + z));
                    boneWeights.Add(new BoneWeight() { boneIndex0 = bones.Count - 1, weight0 = 1 });
                }
            }
            vertices.Add(new Vector3(0, 0, 2f * bodySettings.Radius + (bodySettings.Length * bones.Count)));
            boneWeights.Add(new BoneWeight() { boneIndex0 = bones.Count - 1, weight0 = 1 });

            mesh.vertices = vertices.ToArray();
            mesh.boneWeights = boneWeights.ToArray();
            #endregion

            #region Triangles
            List<int> triangles = new List<int>();

            for (int i = 0; i < bodySettings.Segments; i++)
            {
                int seamOffset = i != bodySettings.Segments - 1 ? 0 : bodySettings.Segments;

                triangles.Add(0);
                triangles.Add(i + 2 - seamOffset);
                triangles.Add(i + 1);
            }

            int rings = (bodySettings.Rings * bones.Count) + (2 * (bodySettings.Segments / 2 - 1));
            for (int ringIndex = 0; ringIndex < rings; ringIndex++)
            {
                int ringOffset = 1 + ringIndex * bodySettings.Segments;

                for (int i = 0; i < bodySettings.Segments; i++)
                {
                    int seamOffset = i != bodySettings.Segments - 1 ? 0 : bodySettings.Segments;

                    triangles.Add(ringOffset + i);
                    triangles.Add(ringOffset + i + 1 - seamOffset);
                    triangles.Add(ringOffset + i + 1 - seamOffset + bodySettings.Segments);

                    triangles.Add(ringOffset + i + 1 - seamOffset + bodySettings.Segments);
                    triangles.Add(ringOffset + i + bodySettings.Segments);
                    triangles.Add(ringOffset + i);
                }
            }

            int topIndex = 1 + (rings + 1) * bodySettings.Segments;
            for (int i = 0; i < bodySettings.Segments; i++)
            {
                int seamOffset = i != bodySettings.Segments - 1 ? 0 : bodySettings.Segments;

                triangles.Add(topIndex);
                triangles.Add(topIndex - i - 2 + seamOffset);
                triangles.Add(topIndex - i - 1);
            }

            mesh.triangles = triangles.ToArray();
            #endregion

            #region Bones
            Transform[] boneTransforms = new Transform[bones.Count];
            Matrix4x4[] bindPoses = new Matrix4x4[bones.Count];
            Vector3[] deltaZeroArray = new Vector3[vertices.Count];
            for (int vertIndex = 0; vertIndex < vertices.Count; vertIndex++)
            {
                deltaZeroArray[vertIndex] = Vector3.zero;
            }

            for (int boneIndex = 0; boneIndex < bones.Count; boneIndex++)
            {
                boneTransforms[boneIndex] = root.GetChild(boneIndex);

                boneTransforms[boneIndex].localPosition = Vector3.forward * (bodySettings.Radius + bodySettings.Length * (0.5f + boneIndex));
                boneTransforms[boneIndex].localRotation = Quaternion.identity;
                bindPoses[boneIndex] = boneTransforms[boneIndex].worldToLocalMatrix * transform.localToWorldMatrix;

                if (boneIndex > 0)
                {
                    HingeJoint hingeJoint = boneTransforms[boneIndex].GetComponent<HingeJoint>();
                    hingeJoint.anchor = new Vector3(0, 0, -bodySettings.Length / 2f);
                    hingeJoint.connectedBody = boneTransforms[boneIndex - 1].GetComponent<Rigidbody>();
                }

                Vector3[] deltaVertices = new Vector3[vertices.Count];
                for (int vertIndex = 0; vertIndex < vertices.Count; vertIndex++)
                {
                    float distanceToBone = Mathf.Clamp(Vector3.Distance(vertices[vertIndex], boneTransforms[boneIndex].localPosition), 0, 2f * bodySettings.Length);
                    Vector3 directionToBone = (vertices[vertIndex] - boneTransforms[boneIndex].localPosition).normalized;

                    deltaVertices[vertIndex] = directionToBone * (2f * bodySettings.Length - distanceToBone);
                }

                mesh.AddBlendShapeFrame("Bone." + boneIndex, 0, deltaZeroArray, deltaZeroArray, deltaZeroArray);
                mesh.AddBlendShapeFrame("Bone." + boneIndex, 100, deltaVertices, deltaZeroArray, deltaZeroArray);
            }

            mesh.bindposes = bindPoses;
            skinnedMeshRenderer.bones = boneTransforms;
            #endregion

            mesh.RecalculateNormals();
            mesh.Optimize();
            #endregion

            #region Mesh Deformation
            for (int boneIndex = 0; boneIndex < bones.Count; boneIndex++)
            {
                boneTransforms[boneIndex].localPosition = bones[boneIndex].Position;
                boneTransforms[boneIndex].localRotation = bones[boneIndex].Rotation;
                skinnedMeshRenderer.SetBlendShapeWeight(boneIndex, bones[boneIndex].Size);
            }

            Mesh skinnedMesh = new Mesh();
            skinnedMeshRenderer.BakeMesh(skinnedMesh);
            meshCollider.sharedMesh = skinnedMesh;
            #endregion
        }

        public void Add(int index, Vector3 position, Quaternion rotation, float size)
        {
            if ((bones.Count + 1) <= bodySettings.MinMaxBones.y)
            {
                GameObject boneGameObject = Instantiate(boneTool, root, false);
                boneGameObject.name = "Bone." + (root.childCount - 1);
                boneGameObject.layer = LayerMask.NameToLayer("Tools");

                if (bones.Count == 0)
                {
                    DestroyImmediate(boneGameObject.GetComponent<HingeJoint>());
                }

                bones.Insert(index, new Bone(position, rotation, size));

                Setup();
            }
        }
        public void AddToFront()
        {
            UpdateBoneConfiguration();

            Vector3 position = bones[0].Position - root.GetChild(0).forward * bodySettings.Length;
            Quaternion rotation = bones[0].Rotation;

            Add(0, position, rotation, Mathf.Clamp(bones[0].Size * 0.75f, 0f, 100f));
        }
        public void AddToBack()
        {
            UpdateBoneConfiguration();

            Vector3 position = bones[bones.Count - 1].Position + root.GetChild(bones.Count - 1).forward * bodySettings.Length;
            Quaternion rotation = bones[bones.Count - 1].Rotation;

            Add(bones.Count, position, rotation, Mathf.Clamp(bones[bones.Count - 1].Size * 0.75f, 0f, 100f));
        }

        public void Remove(int index)
        {
            if ((bones.Count - 1) >= bodySettings.MinMaxBones.x)
            {
                DestroyImmediate(root.GetChild(root.childCount - 1).gameObject);

                bones.RemoveAt(index);

                Setup();
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

        private void UpdateBoneConfiguration()
        {
            for (int boneIndex = 0; boneIndex < bones.Count; boneIndex++)
            {
                bones[boneIndex].Position = root.GetChild(boneIndex).localPosition;
                bones[boneIndex].Rotation = root.GetChild(boneIndex).localRotation;
                bones[boneIndex].Size = skinnedMeshRenderer.GetBlendShapeWeight(boneIndex);
            }
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
        [Serializable]
        public class BodySettings
        {
            #region Fields
            [Header("General")]
            [SerializeField] private Vector2Int minMaxBones = new Vector2Int(2, 10);

            [Header("Bone")]
            [SerializeField] private float radius = 0.25f;
            [SerializeField] private float length = 0.5f;
            [SerializeField] [Range(4, 100)] private int segments = 20;
            [SerializeField] [Range(2, 100)] private int rings = 20;
            #endregion

            #region Properties
            public Vector2Int MinMaxBones { get { return minMaxBones; } }

            public float Radius { get { return radius; } }
            public float Length { get { return length; } }
            public int Segments { get { return segments; } }
            public int Rings { get { return rings; } }
            #endregion
        }

        [Serializable]
        public class Bone
        {
            [SerializeField] private Vector3 position;
            [SerializeField] private Quaternion rotation;
            [SerializeField] private float size;

            public Vector3 Position { get { return position; } set { position = value; } }
            public Quaternion Rotation { get { return rotation; } set { rotation = value; } }
            public float Size { get { return size; } set { size = value; } }

            public Bone() { }

            public Bone(Vector3 position, Quaternion rotation, float size)
            {
                this.position = position;
                this.rotation = rotation;
                this.size = size;
            }
        }
        #endregion
    }
}