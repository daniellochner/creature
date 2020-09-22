using UnityEngine;
using static DanielLochner.Assets.CreatureCreator.CreatureController;

namespace DanielLochner.Assets.CreatureCreator
{
    public class BodyPartController : MonoBehaviour
    {
        #region Fields
        [SerializeField] private float minScale = 0.25f;
        [SerializeField] private float maxScale = 2.5f;
        [SerializeField] private float scaleIncrement = 0.1f;

        private Hover hover;
        private Scroll scroll;
        #endregion

        #region Properties
        public AttachedBodyPart AttachedBodyPart { get; set; }
        public BodyPartController Flipped { get; set; }

        public Drag Drag { get; set; }
        #endregion

        #region Methods
        protected virtual void Awake()
        {
            Drag = GetComponent<Drag>();

            hover = GetComponent<Hover>();
            scroll = GetComponent<Scroll>();
        }
        private void Start()
        {
            hover.OnEnter.AddListener(delegate
            {
                if (!Input.GetMouseButton(0))
                {
                    CreatureCreator.Instance.CameraOrbit.Freeze();
                }
            });
            hover.OnExit.AddListener(delegate
            {
                if (!Input.GetMouseButton(0))
                {
                    CreatureCreator.Instance.CameraOrbit.Unfreeze();
                }
            });

            scroll.OnScrollUp.AddListener(delegate
            {
                if (transform.localScale.x < maxScale - scaleIncrement)
                {
                    transform.localScale += Vector3.one * scaleIncrement;
                    Flipped.transform.localScale = transform.localScale;
                }
            });
            scroll.OnScrollDown.AddListener(delegate
            {
                if (transform.localScale.x > minScale + scaleIncrement)
                {
                    transform.localScale -= Vector3.one * scaleIncrement;
                    Flipped.transform.localScale = transform.localScale;
                }
            });
        }
        #endregion
    }
}