using Cinemachine;
using UnityEngine;

namespace DanielLochner.Assets.CreatureCreator
{
    public class Draggable : MonoBehaviour
    {
        [SerializeField] private MovementPlane movementPlane;
        [SerializeField] private bool xAxis;
        [SerializeField] private bool yAxis;
        [SerializeField] private bool zAxis;


        protected bool isPressing;
        private Camera mainCamera;
        private Vector3 initialPosition;
        private Plane plane;

        private void Start()
        {
            mainCamera = Camera.main;
            plane = new Plane(transform.right, transform.position);
        }

        private void OnMouseDown()
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (plane.Raycast(ray, out float distance))
            {
                initialPosition = ray.GetPoint(distance);
            }
            isPressing = true;
        }
        private void OnMouseUp()
        {
            isPressing = false;
        }

        private enum MovementPlane
        {
            Body,
            Camera
        }
    }
}