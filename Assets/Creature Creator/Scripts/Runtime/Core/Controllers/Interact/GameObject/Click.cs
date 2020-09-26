// Creature Creator - https://github.com/daniellochner/SPORE-Creature-Creator
// Version: 1.0.0
// Author: Daniel Lochner

using UnityEngine;
using UnityEngine.Events;

namespace DanielLochner.Assets.CreatureCreator
{
    public class Click : MonoBehaviour
    {
        #region Fields
        [SerializeField] private float threshold = Mathf.Infinity;
        [SerializeField] private UnityEvent onClick = new UnityEvent();

        private Vector2 initialMousePosition;
        #endregion

        #region Properties
        public UnityEvent OnClick { get { return onClick; } }
        #endregion

        #region Methods
        private void OnMouseDown()
        {
            initialMousePosition = Input.mousePosition;
        }
        private void OnMouseUpAsButton()
        {
            if (Vector2.Distance(Input.mousePosition, initialMousePosition) <= threshold)
            {
                OnClick.Invoke();
            }
        }
        #endregion
    }
}