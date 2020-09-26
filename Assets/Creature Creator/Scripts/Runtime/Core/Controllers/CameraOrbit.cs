// Creature Creator - https://github.com/daniellochner/SPORE-Creature-Creator
// Version: 1.0.0
// Author: Daniel Lochner

using UnityEngine;
using UnityEngine.EventSystems;

namespace DanielLochner.Assets.CreatureCreator
{
    public class CameraOrbit : MonoBehaviour
    {
        #region Fields
        [Header("Rotate")]
        [SerializeField] private bool freezeRotation;
        [SerializeField] private Vector2 mouseSensitivity;
        [SerializeField] private float rotationSmoothing;
        [SerializeField] private Vector2 minMaxRotation;

        [Header("Zoom")]
        [SerializeField] private bool freezeZoom;
        [SerializeField] private float scrollWheelSensitivity;
        [SerializeField] private float zoomSmoothing;
        [SerializeField] private Vector2 minMaxZoom;

        private float targetZoom = 1f;
        private Vector3 targetRotation;
        private Vector2 velocity;

        private Transform offsetCamera;
        #endregion

        #region Properties
        public bool IsFrozen { get; private set; }
        public Vector3 OffsetPosition { get; set; }

        public Camera Camera { get; private set; }
        #endregion

        #region Methods
        private void Awake()
        {
            targetRotation = transform.eulerAngles;

            offsetCamera = transform.GetChild(0);
            OffsetPosition = offsetCamera.localPosition;

            Camera = GetComponentInChildren<Camera>();
        }
        private void LateUpdate()
        {
            if (Input.GetMouseButtonDown(0) && EventSystem.current.IsPointerOverGameObject())
            {
                Freeze();
            }
            else if (Input.GetMouseButtonUp(0))
            {
                Unfreeze();
            }

            OnRotate();
            OnZoom();
        }

        private void OnZoom()
        {
            if (!freezeZoom && !IsFrozen)
            {
                targetZoom = Mathf.Clamp(targetZoom - Input.mouseScrollDelta.y * scrollWheelSensitivity, minMaxZoom.x, minMaxZoom.y);
            }

            offsetCamera.localPosition = Vector3.Lerp(offsetCamera.localPosition, OffsetPosition * targetZoom, Time.deltaTime * zoomSmoothing);
        }
        private void OnRotate()
        {
            if (Input.GetMouseButton(0) && !freezeRotation && !IsFrozen)
            {
                velocity.x += mouseSensitivity.x * Input.GetAxis("Mouse X");
                velocity.y += mouseSensitivity.y * Input.GetAxis("Mouse Y");
            }

            targetRotation.y += velocity.x;
            targetRotation.x -= velocity.y;
            targetRotation.x = ClampAngle(targetRotation.x, minMaxRotation.x, minMaxRotation.y);

            transform.rotation = Quaternion.Euler(targetRotation.x, targetRotation.y, 0);

            velocity.x = Mathf.Lerp(velocity.x, 0, Time.deltaTime * rotationSmoothing);
            velocity.y = Mathf.Lerp(velocity.y, 0, Time.deltaTime * rotationSmoothing);
        }

        private void SetFrozen(bool isFrozen)
        {
            IsFrozen = isFrozen;
        }
        public void Freeze()
        {
            SetFrozen(true);
        }
        public void Unfreeze()
        {
            SetFrozen(false);
        }

        public static float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360f) { angle += 360f; }
            if (angle > 360f) { angle -= 360f; }

            return Mathf.Clamp(angle, min, max);
        }
        #endregion
    }
}