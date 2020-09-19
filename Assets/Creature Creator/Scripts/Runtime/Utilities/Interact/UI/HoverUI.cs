using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace DanielLochner.Assets.CreatureCreator
{
    public class HoverUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        #region Fields
        [SerializeField] private UnityEvent onEnter;
        [SerializeField] private UnityEvent onExit;
        #endregion

        #region Properties
        public UnityEvent OnEnter { get { return onEnter; } }
        public UnityEvent OnExit { get { return onExit; } }
        #endregion

        #region Methods
        public void OnPointerEnter(PointerEventData eventData)
        {
            OnEnter.Invoke();
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            OnExit.Invoke();
        }
        #endregion
    }
}