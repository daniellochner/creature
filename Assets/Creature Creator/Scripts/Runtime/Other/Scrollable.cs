using UnityEngine;

namespace DanielLochner.Assets.CreatureCreator
{
    public class Scrollable : MonoBehaviour
    {
        protected bool isPressing;
        private Camera mainCamera;
        private Vector3 initialPosition;
        private Plane plane;

        private void Start()
        {
            mainCamera = Camera.main;
            plane = new Plane(transform.right, transform.position);
        }

        private void Update()
        {
            
        }
    }
}