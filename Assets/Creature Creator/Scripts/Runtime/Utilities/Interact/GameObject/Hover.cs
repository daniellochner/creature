using UnityEngine;
using UnityEngine.Events;

namespace DanielLochner.Assets.CreatureCreator
{
    public class Hover : MonoBehaviour
    {
        #region Properties
        public UnityEvent OnEnter { get; set; } = new UnityEvent();
        public UnityEvent OnExit { get; set; } = new UnityEvent();
        #endregion

        #region Methods
        private void OnMouseEnter()
        {
            OnEnter.Invoke();
        }
        private void OnMouseExit()
        {
            OnExit.Invoke();
        }
        #endregion
    }
}