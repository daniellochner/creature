// Creature Creator - https://github.com/daniellochner/SPORE-Creature-Creator
// Version: 1.0.0
// Author: Daniel Lochner

using System;
using UnityEngine;
using UnityEngine.Events;

namespace DanielLochner.Assets.CreatureCreator
{
    public class Drag : MonoBehaviour
    {
        #region Fields
        [SerializeField] private MousePlaneAlignment mousePlaneAlignment = MousePlaneAlignment.ToLocalDirection;
        [SerializeField] private Vector3 localDirection = new Vector3(1, 0, 0);
        [SerializeField] private Vector3 worldDirection = new Vector3(1, 0, 0);
        [Space]
        [SerializeField] private float maxDistance = Mathf.Infinity;
        [SerializeField] private EnabledAxes localMovement = new EnabledAxes()
        {
            x = true,
            y = true,
            z = true
        };
        [SerializeField] private Bounds worldBounds = new Bounds(Vector3.zero, Mathf.Infinity * Vector3.one);
        [Space]
        [SerializeField] private float smoothing = 0f;
        [SerializeField] private bool resetOnRelease = false;
        [SerializeField] private bool useOffsetPosition = true;
        [SerializeField] private bool draggable = true;
        [SerializeField] private bool dragFromPosition = false;

        private Vector3 startWorldPosition, targetWorldPosition, offsetPosition;
        private Camera mainCamera;
        private Plane plane;
        #endregion

        #region Properties
        public UnityEvent OnPress { get; set; } = new UnityEvent();
        public UnityEvent OnRelease { get; set; } = new UnityEvent();
        public UnityEvent OnDrag { get; set; } = new UnityEvent();

        public bool IsPressing { get; set; }
        public Vector3 TargetWorldPosition { get { return targetWorldPosition; } }
        public bool UseOffsetPosition { get { return useOffsetPosition; } set { useOffsetPosition = value; } }

        public Vector3 TargetMousePosition { get; private set; }

        public Plane Plane { get { return plane; } set { plane = value; } }
        public bool Draggable { get { return draggable; } set { draggable = value; } }
        public Bounds WorldBounds { get { return worldBounds; } set { worldBounds = value; } }
        #endregion

        #region Methods
        private void Awake()
        {
            mainCamera = Camera.main;

            UpdatePlane();
        }
        private void Update()
        {
            if (Input.GetMouseButtonUp(0) && IsPressing) // "OnMouseUp()" is unreliable.
            {
                if (resetOnRelease)
                {
                    transform.position = startWorldPosition;
                }

                OnRelease.Invoke();

                IsPressing = false;
            }
        }
        private void FixedUpdate()
        {
            if (IsPressing)
            {
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                if (plane.Raycast(ray, out float distance))
                {
                    // Restrict movement along the specified LOCAL axes, then
                    // clamp the targeted position based on the specified WORLD
                    // bounds and maximum distance from starting position.

                    Vector3 targetLocalPosition = transform.InverseTransformPoint(TargetMousePosition = ray.GetPoint(distance)) - offsetPosition;
                    targetLocalPosition.x = localMovement.x ? targetLocalPosition.x : 0;
                    targetLocalPosition.y = localMovement.y ? targetLocalPosition.y : 0;
                    targetLocalPosition.z = localMovement.z ? targetLocalPosition.z : 0;

                    targetWorldPosition = startWorldPosition + Vector3.ClampMagnitude(transform.TransformPoint(targetLocalPosition) - startWorldPosition, maxDistance);
                    targetWorldPosition.x = Mathf.Clamp(targetWorldPosition.x, worldBounds.center.x - worldBounds.extents.x / 2f, worldBounds.center.x + worldBounds.extents.x / 2f);
                    targetWorldPosition.y = Mathf.Clamp(targetWorldPosition.y, worldBounds.center.y - worldBounds.extents.y / 2f, worldBounds.center.y + worldBounds.extents.y / 2f);
                    targetWorldPosition.z = Mathf.Clamp(targetWorldPosition.z, worldBounds.center.z - worldBounds.extents.z / 2f, worldBounds.center.z + worldBounds.extents.z / 2f);

                    if (draggable) { transform.position = targetWorldPosition; }

                    OnDrag.Invoke();
                }
            }
        }

        public void OnMouseDown()
        {
            if (dragFromPosition) UpdatePlane();

            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (plane.Raycast(ray, out float distance))
            {
                startWorldPosition = transform.position;
                if (UseOffsetPosition) { offsetPosition = transform.InverseTransformPoint(ray.GetPoint(distance)); }
            }

            OnPress.Invoke();

            IsPressing = true;
        }
        private void OnDrawGizmos()
        {
            #if UNITY_EDITOR
            if (UnityEditor.Selection.activeTransform == transform)
            {
                Gizmos.DrawWireCube(worldBounds.center, worldBounds.extents);
            }
            #endif
        }

        private void UpdatePlane()
        {
            if (mousePlaneAlignment == MousePlaneAlignment.WithCamera)
            {
                plane = new Plane(mainCamera.transform.forward, transform.position);
            }
            else if (mousePlaneAlignment == MousePlaneAlignment.ToLocalDirection)
            {
                plane = new Plane(transform.TransformDirection(localDirection), transform.position);
            }
            else if (mousePlaneAlignment == MousePlaneAlignment.ToWorldDirection)
            {
                plane = new Plane(worldDirection, transform.position);
            }
        }
        #endregion

        #region Enumerators
        public enum MousePlaneAlignment
        {
            ToLocalDirection,
            ToWorldDirection,
            WithCamera
        }
        #endregion

        #region Inner Classes
        [Serializable] public class EnabledAxes
        {
            public bool x, y, z;
        }
        #endregion
    }
}