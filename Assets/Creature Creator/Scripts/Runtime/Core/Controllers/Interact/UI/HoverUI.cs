// Creature Creator - https://github.com/daniellochner/SPORE-Creature-Creator
// Version: 1.0.0
// Author: Daniel Lochner

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace DanielLochner.Assets.CreatureCreator
{
    public class HoverUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        #region Fields
        [SerializeField] private UnityEvent onEnter;
        [SerializeField] private UnityEvent onHover;
        [SerializeField] private UnityEvent onExit;
        #endregion

        #region Properties
        public UnityEvent OnEnter { get { return onEnter; } }
        public UnityEvent OnHover { get { return onHover; } }
        public UnityEvent OnExit { get { return onExit; } }

        public bool Entered { get; private set; }
        #endregion

        #region Methods
        private void Update()
        {
            if (Entered && onHover != null) { onHover.Invoke(); }
        }
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (onEnter != null)
            {
                onEnter.Invoke();
            }
            Entered = true;
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            if (onExit != null)
            {
                onExit.Invoke();
            }
            Entered = false;
        }
        #endregion
    }
}