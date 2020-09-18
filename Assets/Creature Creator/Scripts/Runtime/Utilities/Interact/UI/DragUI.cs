using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DanielLochner.Assets.CreatureCreator
{
    public class DragUI : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        #region Fields
        [SerializeField] private float smoothing = 10f;
        [SerializeField] private bool resetOnRelease = false;
        [SerializeField] private bool draggable = true;

        private Vector2 offsetPosition, targetPosition;
        private RectTransform rectTransform;

        private Camera mainCamera;
        private GridLayoutGroup gridLayoutGroup;
        #endregion

        #region Properties
        public UnityEvent OnPress { get; set; } = new UnityEvent();
        public UnityEvent OnRelease { get; set; } = new UnityEvent();
        public UnityEvent OnDrag { get; set; } = new UnityEvent();

        public bool IsPressing { get; set; }
        #endregion

        #region Methods
        private void Start()
        {
            mainCamera = Camera.main;

            rectTransform = transform as RectTransform;
        }
        private void Update()
        {
            if (IsPressing)
            {
                if (smoothing > 0)
                {
                    rectTransform.position = Vector3.Lerp(rectTransform.position, targetPosition, Time.deltaTime * smoothing);
                }
                else
                {
                    rectTransform.position = targetPosition;
                }

                targetPosition = (Vector2)Input.mousePosition - offsetPosition;

                OnDrag.Invoke();
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            offsetPosition = eventData.position - (Vector2)rectTransform.position;
            targetPosition = eventData.position - offsetPosition;

            OnPress.Invoke();

            IsPressing = true;
        }
        public void OnPointerUp(PointerEventData eventData)
        {
            OnRelease.Invoke();

            IsPressing = false;
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            // Unreliable, however necessary to prevent parent OnDrag() from invoking.
        }
        #endregion
    }
}